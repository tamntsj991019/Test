using AutoMapper;
using Data.ConstData;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Core
{
    public interface ITransactionService
    {
        ResultModel GetById(Guid id);
        ResultModel GetByType(Guid? bookingId, string type, int pageIndex, int pageSize);

        ResultModel Create(TransactionCreateModel model);
        ResultModel Deposit(double money, string userId);
        ResultModel GetUserHistory(string userId);
    }
    public class TransactionService : ITransactionService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly IAccountService _accountService;
        private readonly INotificationService _notificationService;

        public TransactionService(IMapper mapper, AppDbContext dbContext, IAccountService accountService, INotificationService notificationService)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _accountService = accountService;
            _notificationService = notificationService;
        }

        public ResultModel GetById(Guid id)
        {
            var result = new ResultModel();
            try
            {
                var transactions = _dbContext.Transactions.Where(t => t.Id == id).ToList();

                result.Data = _mapper.Map<List<Transaction>, List<TransactionUserViewModel>>(transactions);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetByType(Guid? bookingId, string type, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var transactions = _dbContext.Transactions.Include(t => t.User)
                                                          .Include(t => t.Booking)
                                                          .Where(t => t.BookingId.ToString().ToUpper() == bookingId.ToString().ToUpper()
                                                                      || bookingId == null)
                                                          .Where(t => t.Type == type)
                                                          .OrderByDescending(t => t.DateCreated).ToList();

                if (type == ConstTransactionType.USER)
                {
                    result.Data = new PagingModel
                    {
                        Total = _mapper.Map<List<Transaction>, List<TransactionUserViewModel>>(transactions).Count,
                        Data = _mapper.Map<List<Transaction>, List<TransactionUserViewModel>>(transactions).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                    };
                }
                else if (type == ConstTransactionType.COMPANY)
                {
                    result.Data = new PagingModel
                    {
                        Total = _mapper.Map<List<Transaction>, List<TransactionCompanyViewModel>>(transactions).Count,
                        Data = _mapper.Map<List<Transaction>, List<TransactionCompanyViewModel>>(transactions).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                    };
                }
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Create(TransactionCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var transaction = _mapper.Map<TransactionCreateModel, Transaction>(model);

                _dbContext.Add(transaction);
                _dbContext.SaveChanges();

                string userId = model.UserId;
                _accountService.UpdateBalance(userId, transaction.Deposit.Value);

                result.Data = transaction.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Deposit(double money, string userId)
        {
            var result = new ResultModel();
            try
            {
                TransactionCreateModel tcModel = new TransactionCreateModel()
                {
                    UserId = userId,
                    Deposit = money,
                    Description = "Nạp thêm tiền vào tài khoản thành công",
                    Type = ConstTransactionType.USER
                };
                Create(tcModel);

                string userBalance = _dbContext.Users.FirstOrDefault(u => u.Id == userId).Balance.ToString();
                NotificationCreateModel ncModel = new NotificationCreateModel()
                {
                    UserId = userId,
                    Description = "Nạp tiền thành công (" + money.ToString() + "). Số dư hiện tại: " + userBalance,
                    Type = ConstNotificationType.DEPOSIT
                };
                _notificationService.Create(ncModel);

                result.Data = userBalance;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetUserHistory(string userId)
        {
            var result = new ResultModel();
            try
            {
                var transactions = _dbContext.Transactions.Where(t => t.IsDisable == false)
                                                         .Where(t => t.UserId == userId)
                                                         .OrderByDescending(t => t.DateCreated).ToList();

                List<TransactionUserHistoryModel> listTrans = new List<TransactionUserHistoryModel>();
                
                foreach (var tran in transactions)
                {
                    if ((tran.Booking != null && tran.Booking.IsDisable == false) || tran.Booking == null)
                    {
                        TransactionUserHistoryModel tuhModel = new TransactionUserHistoryModel()
                        {
                            Title = (tran.BookingId != null) ? "Thanh toán đặt lịch" : "Nạp tiền vào ví",
                            DateCreated = tran.DateCreated.Value,
                            Money = (tran.Deposit.Value >= 0) ? "+" + tran.Deposit.Value.ToString() : tran.Deposit.Value.ToString(),
                            IsBooking = (tran.BookingId != null) ? true : false
                        };
                        listTrans.Add(tuhModel);
                    }
                }

                result.Data = listTrans;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }
    }
}
