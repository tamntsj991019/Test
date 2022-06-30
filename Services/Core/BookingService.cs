using AutoMapper;
using Data.ConstData;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Directions.Request;
using GoogleApi.Entities.Maps.Directions.Response;
using Services.Util;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Http;
using Hangfire;
using Hubs;
using MoreLinq;
using Microsoft.EntityFrameworkCore;

namespace Services.Core
{
    public interface IBookingService
    {
        ResultModel Get(Guid? id, int pageIndex, int pageSize);
        ResultModel GetNewBooking(Guid? id, int pageIndex, int pageSize);
        ResultModel GetFinishedBooking(Guid? id, int pageIndex, int pageSize);
        ResultModel GetBookingInWorking(string empName, int pageIndex, int pageSize);
        Task<ResultModel> GetNewBookingWithRecomendEmp(Guid id, double radius);

        ResultModel GetBookingEnableById(Guid id);
        ResultModel GetBookingDetailAdmin(Guid id);

        ResultModel GetBookingEmployeeWaitingEnables(string userId);
        ResultModel GetBookingEmployeeProcessingEnables(string userId);
        ResultModel GetHistoryBookingEmployeeEnables(string userId);

        ResultModel GetBookingCustomerWaitingEnables(string userId);
        ResultModel GetBookingCustomerProcessingEnables(string userId);
        ResultModel GetHistoryBookingCustomerEnables(string userId);

        // Task<ResultModel> CreateOld(BookingCreateModel model, string customerId, bool isRandomEmployee);
        Task<ResultModel> Create(BookingCreateModel model, string customerId, bool isRandomEmployee);
        ResultModel CheckEmployeeSuitable(string empCode, BookingCreateModel model);
        Task<ResultModel> AssignWorkToEmployeeAsync(Guid bookingId, string empId);
        //ResultModel RebookingEmployee(BookingEmployeeAgainCreateModel model, string customerId);
        Task<ResultModel> ReFindSuitableEmployee(Guid bookingId);

        Task<ResultModel> CheckBeforeUpdateServiceDetails(Guid bookingId, BookingUpdateServiceDetailsModel model, string role);
        Task<ResultModel> UpdateBookingStatus(Guid id, string statusId, string bookingDescription = null);
        ResultModel Delete(Guid id);

        // Backup
        //Task<ResultModel> UpdateBookingLocation(Guid bookingId, BookingUpdateLocationModel model);
    }

    public class BookingService : IBookingService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly ITransactionService _transactionService;
        private readonly INotificationService _notificationService;
        private readonly IBookingLogService _bookingLogService;
        private readonly IBookingHub _bookingHub;
        private readonly IAlgorithmService _algorithmService;

        private readonly Utils _utils = new Utils();

        public BookingService(IMapper mapper, AppDbContext dbContext, ITransactionService transactionService, INotificationService notificationService,
                              IBookingLogService bookingLogService, IBookingHub bookingHub, IAlgorithmService algorithmService)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _transactionService = transactionService;
            _notificationService = notificationService;
            _bookingLogService = bookingLogService;
            _bookingHub = bookingHub;
            _algorithmService = algorithmService;

        }

        #region Admin Booking

        // admin get all
        public ResultModel Get(Guid? id, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var bookings = _dbContext.Bookings.Include(b => b.Customer)
                                                  .Include(b => b.Employee)
                                                  .Include(b => b.Province)
                                                  .Include(b => b.District)
                                                  .Include(b => b.Ward)
                                                  //.Include(b => b.ServiceBookings)
                                                  //.Include(b => b.BookingLogs)
                                                  .Where(s => s.Id.ToString().ToUpper() == id.ToString().ToUpper()
                                                              || id == null).ToList();

                List<BookingListAdminViewModel> listBooking = new List<BookingListAdminViewModel>();
                foreach (var b in bookings)
                {
                    var bStatusId = b.BookingLogs.OrderByDescending(bl => bl.DateCreated).FirstOrDefault().BookingStatusId;

                    BookingListAdminViewModel model = _mapper.Map<Booking, BookingListAdminViewModel>(b);
                    model.BookingStatusId = bStatusId;

                    var bStatus = _dbContext.BookingStatuses.FirstOrDefault(bs => bs.Id == bStatusId);
                    model.BookingStatus = _mapper.Map<BookingStatus, BaseStringModel>(bStatus);

                    listBooking.Add(model);
                }

                result.Data = new PagingModel
                {
                    Total = listBooking.Count,
                    Data = listBooking.Skip((pageIndex - 1) * pageSize).Take(pageSize).OrderBy(_d => _d.DateCreated).ToList()
                };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        // admin get new
        public ResultModel GetNewBooking(Guid? id, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var bLogs = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                  .Include(bl => bl.Booking.Customer)
                                                  .Include(bl => bl.Booking.Employee)
                                                  .Include(bl => bl.Booking.Province)
                                                  .Include(bl => bl.Booking.District)
                                                  .Include(bl => bl.Booking.Ward)
                                                  .Where(bl => bl.BookingId.ToString().ToUpper() == id.ToString().ToUpper() || id == null)
                                                  .Where(bl => bl.Booking.IsDisable == false && bl.IsDisable == false)
                                                  .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                                  .Where(bl => bl.BookingStatusId == ConstBookingStatus.CREATE_SUCCESS)
                                                  .OrderBy(bl => bl.Booking.DateBegin).ToList();

                List<BookingListAdminViewModel> listBooking = new List<BookingListAdminViewModel>();
                if (bLogs != null && bLogs.Count() > 0)
                {
                    foreach (var bLog in bLogs)
                    {
                        var booking = _mapper.Map<Booking, BookingListAdminViewModel>(bLog.Booking);
                        booking.BookingStatusId = bLog.BookingStatusId;
                        booking.BookingStatus = _mapper.Map<BookingStatus, BaseStringModel>(bLog.BookingStatus);
                        listBooking.Add(booking);
                    }
                }

                result.Data = new PagingModel
                {
                    Total = listBooking.Count,
                    Data = listBooking.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        // admin get finish
        public ResultModel GetFinishedBooking(Guid? id, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var bLogs = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                  .Include(bl => bl.Booking.Customer)
                                                  .Include(bl => bl.Booking.Employee)
                                                  .Include(bl => bl.Booking.Province)
                                                  .Include(bl => bl.Booking.District)
                                                  .Include(bl => bl.Booking.Ward)
                                                  .Where(bl => bl.Booking.IsDisable == false && bl.IsDisable == false)
                                                  .Where(bl => bl.BookingId.ToString().ToUpper() == id.ToString().ToUpper() || id == null)
                                                  .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                                  .Where(bl => bl.BookingStatusId == ConstBookingStatus.DONE ||
                                                               bl.BookingStatusId == ConstBookingStatus.CANCELLED ||
                                                               bl.BookingStatusId == ConstBookingStatus.REJECTED ||
                                                               bl.BookingStatusId == ConstBookingStatus.BALANCE_NOT_ENOUGH)
                                                  .OrderBy(bl => bl.Booking.DateBegin).ToList();

                List<BookingListAdminViewModel> listBooking = new List<BookingListAdminViewModel>();
                if (bLogs != null && bLogs.Count() > 0)
                {
                    foreach (var bLog in bLogs)
                    {
                        var booking = _mapper.Map<Booking, BookingListAdminViewModel>(bLog.Booking);
                        booking.BookingStatusId = bLog.BookingStatusId;
                        booking.BookingStatus = _mapper.Map<BookingStatus, BaseStringModel>(bLog.BookingStatus);
                        listBooking.Add(booking);
                    }
                }

                result.Data = new PagingModel
                {
                    Total = listBooking.Count,
                    Data = listBooking.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        // admin get processing
        public ResultModel GetBookingInWorking(string empName, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var bLogs = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                  .Include(bl => bl.Booking.Customer)
                                                  .Include(bl => bl.Booking.Employee)
                                                  .Include(bl => bl.Booking.Province)
                                                  .Include(bl => bl.Booking.District)
                                                  .Include(bl => bl.Booking.Ward)
                                                  .Where(bl => bl.Booking.IsDisable == false && bl.IsDisable == false)
                                                  .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                                  .Where(bl => bl.BookingStatusId == ConstBookingStatus.PROCESSING ||
                                                               bl.BookingStatusId == ConstBookingStatus.CONFIRM_WAITING ||
                                                               bl.BookingStatusId == ConstBookingStatus.RE_CLEANING)
                                                  .OrderBy(bl => bl.Booking.DateBegin).ToList();

                List<BookingListInWorkingAdminViewModel> listBooking = new List<BookingListInWorkingAdminViewModel>();
                if (bLogs != null && bLogs.Count() > 0)
                {
                    if (empName != null && empName != "")
                    {
                        string nameInput = _utils.RepalceSignForVietnameseString(empName).ToUpper();
                        bLogs = bLogs.Where(bl => _utils.RepalceSignForVietnameseString(bl.Booking.Employee.Fullname)
                                                                                .ToUpper().Contains(nameInput)).ToList();
                    }

                    foreach (var bLog in bLogs)
                    {
                        var booking = _mapper.Map<Booking, BookingListInWorkingAdminViewModel>(bLog.Booking);
                        booking.BookingStatusId = bLog.BookingStatusId;
                        booking.BookingStatus = _mapper.Map<BookingStatus, BaseStringModel>(bLog.BookingStatus);
                        listBooking.Add(booking);
                    }
                }

                result.Data = new PagingModel
                {
                    Total = listBooking.Count,
                    Data = listBooking.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetBookingDetailAdmin(Guid id)
        {
            var result = new ResultModel();
            try
            {
                var booking = _dbContext.Bookings.Include(b => b.Customer)
                                                  .Include(b => b.Employee)
                                                  .Include(b => b.Province)
                                                  .Include(b => b.District)
                                                  .Include(b => b.Ward)
                                                  .Where(s => s.Id == id).FirstOrDefault();

                if (booking == null)
                {
                    throw new Exception("Invalid Id");
                }

                var bStatusId = booking.BookingLogs.OrderByDescending(bl => bl.DateCreated).FirstOrDefault().BookingStatusId;

                BookingDetailsAdminViewModel rsBooking = _mapper.Map<Booking, BookingDetailsAdminViewModel>(booking);
                rsBooking.BookingStatusId = bStatusId;

                var bStatus = _dbContext.BookingStatuses.FirstOrDefault(bs => bs.Id == bStatusId);
                rsBooking.BookingStatus = _mapper.Map<BookingStatus, BaseStringModel>(bStatus);

                result.Data = rsBooking;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetEmployeeAvgRating(string empId)
        {
            var result = new ResultModel();
            try
            {
                var feedbacks = _dbContext.Feedbacks.Include(f => f.User).Include(f => f.Booking)
                                                    .Where(f => f.Booking.EmployeeId == empId)
                                                    .Where(f => f.UserId != empId)
                                                    .Where(f => f.IsDisable == false)
                                                    .OrderByDescending(f => f.DateCreated).ToList();

                if(feedbacks.Count == 0)
                {
                    result.Data = 0.0;
                }
                else
                {
                    int count = feedbacks.Count;
                    double totalRating = feedbacks.Sum(f => f.Rating);

                    result.Data = Math.Round((totalRating / count), 1);
                }
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> GetRecomendEmpsForBooking(Guid id, double radius)
        {
            var result = new ResultModel();
            try
            {
                var booking = _dbContext.Bookings.FirstOrDefault(b => b.Id == id && b.IsDisable == false);

                List<EmployeeRecommendedModel> listResultReturn = new List<EmployeeRecommendedModel>();

                List<EmployeeWithBookingLogs> listEmpWB = await _algorithmService.GetEmployeeSuitable(booking, radius);
                if (listEmpWB.Count() > 0)
                {
                    foreach (var ewb in listEmpWB)
                    {
                        var emp = _dbContext.Users.FirstOrDefault(u => u.Id == ewb.EmployeeId);

                        EmployeeRecommendedModel empR = _mapper.Map<User, EmployeeRecommendedModel>(emp);
                        empR.AvgRating = (double)GetEmployeeAvgRating(emp.Id).Data;
                        empR.Distance = ewb.Distance;
                        empR.NumberOfBooking = ewb.bLogs.Count();

                        listResultReturn.Add(empR);
                    }
                }

                result.Data = listResultReturn;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> GetNewBookingWithRecomendEmp(Guid id, double radius)
        {
            var result = new ResultModel();
            try
            {
                var resultBookingDetail = GetBookingDetailAdmin(id);
                if (!resultBookingDetail.Succeed)
                {
                    throw new Exception(resultBookingDetail.ErrorMessage);
                }

                if (radius == 0)
                {
                    var setting = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.MAX_DISTANCE);
                    radius = double.Parse(setting.Data);
                }

                var listRecomendEmp = await GetRecomendEmpsForBooking(id, radius);
                if (!listRecomendEmp.Succeed)
                {
                    throw new Exception(listRecomendEmp.ErrorMessage);
                }

                result.Data = new BookingWithRecomendEmpModel()
                {
                    Distance = radius,
                    BookingDetail = (BookingDetailsAdminViewModel)resultBookingDetail.Data,
                    ListEmp = (List<EmployeeRecommendedModel>)listRecomendEmp.Data
                };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }
        #endregion

        public ResultModel GetBookingEnableById(Guid id)
        {
            var result = new ResultModel();
            try
            {
                var bLog = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                  .Include(bl => bl.Booking.Customer)
                                                  .Include(bl => bl.Booking.Employee)
                                                  .Include(bl => bl.Booking.Province)
                                                  .Include(bl => bl.Booking.District)
                                                  .Include(bl => bl.Booking.Ward)
                                                  .Include(bl => bl.Booking.ServiceBookings)
                                                  .Where(bl => bl.Booking.IsDisable == false && bl.BookingId == id)
                                                  .ToList().OrderByDescending(bl => bl.DateCreated).FirstOrDefault();

                if (bLog == null)
                {
                    throw new Exception("Invalid Id");
                }

                BookingLogMainViewModel rsModel = _mapper.Map<BookingLog, BookingLogMainViewModel>(bLog);

                Guid serviceId = rsModel.Booking.ServiceBookings.FirstOrDefault().ServiceId;
                Guid serviceGroupId = _dbContext.Services.FirstOrDefault(s => s.Id == serviceId).ServiceGroupId;

                string serviceGroupType = _dbContext.ServiceGroups.FirstOrDefault(s => s.Id == serviceGroupId).Type;
                rsModel.Booking.IsCleaningAll = serviceGroupType == ConstServiceGroupType.OVERALL;

                string empId = rsModel.Booking.EmployeeId;
                if (empId != null)
                {
                    rsModel.Booking.Employee.AvgRating = (double)GetEmployeeAvgRating(empId).Data;
                }

                result.Data = rsModel;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        #region Employee booking list
        public ResultModel GetBookingEmployeeWaitingEnables(string userId)
        {
            var result = new ResultModel();
            try
            {
                var bLogs = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                  .Include(bl => bl.Booking.Customer)
                                                  .Include(bl => bl.Booking.Province)
                                                  .Include(bl => bl.Booking.District)
                                                  .Include(bl => bl.Booking.Ward)
                                                  .Where(bl => bl.Booking.EmployeeId == userId && bl.Booking.IsDisable == false
                                                            && bl.IsDisable == false)
                                                  .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                                  .Where(bl => bl.BookingStatusId == ConstBookingStatus.WAITING).ToList();

                List<BookingEmployeeByStatusViewModel> listBooking = new List<BookingEmployeeByStatusViewModel>();
                if (bLogs != null && bLogs.Count() > 0)
                {
                    foreach (var bl in bLogs)
                    {
                        //if (bl.BookingStatusId == ConstBookingStatus.WAITING)
                        //{
                        var bLog = _mapper.Map<BookingLog, BookingLogEmployeeByStatusViewModel>(bl);
                        listBooking.Add(bLog.Booking);
                        //}
                    }
                }

                result.Data = listBooking;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetBookingEmployeeProcessingEnables(string userId)
        {
            var result = new ResultModel();
            try
            {
                var bLogs = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                  .Include(bl => bl.Booking.Customer)
                                                  .Include(bl => bl.Booking.Province)
                                                  .Include(bl => bl.Booking.District)
                                                  .Include(bl => bl.Booking.Ward)
                                                  .Where(bl => bl.Booking.EmployeeId == userId && bl.Booking.IsDisable == false
                                                            && bl.IsDisable == false)
                                                  .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                                  .Where(bl => bl.BookingStatusId == ConstBookingStatus.PROCESSING ||
                                                               bl.BookingStatusId == ConstBookingStatus.CONFIRM_WAITING ||
                                                               bl.BookingStatusId == ConstBookingStatus.RE_CLEANING).ToList();

                List<BookingLogEmployeeByStatusViewModel> listBLog = new List<BookingLogEmployeeByStatusViewModel>();
                if (bLogs != null && bLogs.Count() > 0)
                {
                    listBLog = _mapper.Map<List<BookingLog>, List<BookingLogEmployeeByStatusViewModel>>(bLogs);
                    //foreach (var bl in bLogs)
                    //{
                    //    if (bl.BookingStatusId == ConstBookingStatus.PROCESSING ||
                    //       bl.BookingStatusId == ConstBookingStatus.CONFIRM_WAITING ||
                    //       bl.BookingStatusId == ConstBookingStatus.RE_CLEANING)
                    //    {
                    //        BookingLogEmployeeByStatusViewModel bLog = _mapper.Map<BookingLog, BookingLogEmployeeByStatusViewModel>(bl);
                    //        if (bl.Booking.Customer != null && bl.Booking.Customer.Avatar != null)
                    //        {
                    //            bLog.Booking.Customer.HasAvatar = bLog.Booking.Customer.Id;
                    //        }
                    //        listBLog.Add(bLog);
                    //    }
                    //}
                }

                result.Data = listBLog.OrderBy(bl => bl.Booking.DateBegin).ToList();
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetHistoryBookingEmployeeEnables(string userId)
        {
            var result = new ResultModel();
            try
            {
                var bLogs = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                  .Include(bl => bl.Booking.Customer)
                                                  .Include(bl => bl.Booking.Province)
                                                  .Include(bl => bl.Booking.District)
                                                  .Include(bl => bl.Booking.Ward)
                                                  .Where(bl => bl.Booking.EmployeeId == userId && bl.Booking.IsDisable == false
                                                            && bl.IsDisable == false)
                                                  .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                                  .Where(bl => bl.BookingStatusId == ConstBookingStatus.CANCELLED ||
                                                               bl.BookingStatusId == ConstBookingStatus.REJECTED ||
                                                               bl.BookingStatusId == ConstBookingStatus.DONE).ToList();

                List<BookingLogEmployeeByStatusViewModel> listBLog = new List<BookingLogEmployeeByStatusViewModel>();
                if (bLogs != null && bLogs.Count() > 0)
                {
                    listBLog = _mapper.Map<List<BookingLog>, List<BookingLogEmployeeByStatusViewModel>>(bLogs);
                }

                result.Data = listBLog.OrderByDescending(bl => bl.Booking.DateBegin).ToList();
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }
        #endregion

        #region Customer booking list
        public ResultModel GetBookingCustomerWaitingEnables(string userId)
        {
            var result = new ResultModel();
            try
            {
                var bLogs = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                  .Include(bl => bl.Booking.Employee)
                                                  .Include(bl => bl.Booking.Province)
                                                  .Include(bl => bl.Booking.District)
                                                  .Include(bl => bl.Booking.Ward)
                                                  .Where(bl => bl.Booking.CustomerId == userId && bl.Booking.IsDisable == false
                                                            && bl.IsDisable == false)
                                                  .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                                  .Where(bl => bl.BookingStatusId == ConstBookingStatus.WAITING ||
                                                         bl.BookingStatusId == ConstBookingStatus.CREATE_SUCCESS).ToList();

                List<BookingLogCustomerByStatusViewModel> listBLog = new List<BookingLogCustomerByStatusViewModel>();
                if (bLogs != null && bLogs.Count() > 0)
                {
                    listBLog = _mapper.Map<List<BookingLog>, List<BookingLogCustomerByStatusViewModel>>(bLogs);
                    //foreach (var bl in bLogs)
                    //{
                    //    if (bl.BookingStatusId == ConstBookingStatus.EMPLOYEE_NOT_FOUND ||
                    //       bl.BookingStatusId == ConstBookingStatus.PAYMENT_WAITING ||
                    //       bl.BookingStatusId == ConstBookingStatus.WAITING)
                    //    {
                    //        BookingLogCustomerByStatusViewModel bLog = _mapper.Map<BookingLog, BookingLogCustomerByStatusViewModel>(bl);
                    //        if (bl.Booking.Employee != null && bl.Booking.Employee.Avatar != null)
                    //        {
                    //            bLog.Booking.Employee.HasAvatar = bLog.Booking.Employee.Id;
                    //        }
                    //        listBLog.Add(bLog);
                    //    }
                    //}
                }

                result.Data = listBLog.OrderBy(bl => bl.Booking.DateBegin).ToList();
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetBookingCustomerProcessingEnables(string userId)
        {
            var result = new ResultModel();
            try
            {
                var bLogs = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                  .Include(bl => bl.Booking.Employee)
                                                  .Include(bl => bl.Booking.Province)
                                                  .Include(bl => bl.Booking.District)
                                                  .Include(bl => bl.Booking.Ward)
                                                  .Where(bl => bl.Booking.CustomerId == userId && bl.Booking.IsDisable == false
                                                            && bl.IsDisable == false)
                                                  .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                                  .Where(bl => bl.BookingStatusId == ConstBookingStatus.PROCESSING ||
                                                               bl.BookingStatusId == ConstBookingStatus.CONFIRM_WAITING ||
                                                               bl.BookingStatusId == ConstBookingStatus.RE_CLEANING).ToList();

                List<BookingLogCustomerByStatusViewModel> listBLog = new List<BookingLogCustomerByStatusViewModel>();
                if (bLogs != null && bLogs.Count() > 0)
                {
                    listBLog = _mapper.Map<List<BookingLog>, List<BookingLogCustomerByStatusViewModel>>(bLogs);
                }

                result.Data = listBLog.OrderBy(bl => bl.Booking.DateBegin).ToList();
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetHistoryBookingCustomerEnables(string userId)
        {
            var result = new ResultModel();
            try
            {
                var bLogs = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                  .Include(bl => bl.Booking.Employee)
                                                  .Include(bl => bl.Booking.Province)
                                                  .Include(bl => bl.Booking.District)
                                                  .Include(bl => bl.Booking.Ward)
                                                  .Where(bl => bl.Booking.CustomerId == userId && bl.Booking.IsDisable == false
                                                            && bl.IsDisable == false)
                                                  .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                                  .Where(bl => bl.BookingStatusId == ConstBookingStatus.BALANCE_NOT_ENOUGH ||
                                                               bl.BookingStatusId == ConstBookingStatus.CANCELLED ||
                                                               bl.BookingStatusId == ConstBookingStatus.REJECTED ||
                                                               bl.BookingStatusId == ConstBookingStatus.DONE).ToList();

                List<BookingLogCustomerByStatusViewModel> listBLog = new List<BookingLogCustomerByStatusViewModel>();
                if (bLogs != null && bLogs.Count() > 0)
                {
                    listBLog = _mapper.Map<List<BookingLog>, List<BookingLogCustomerByStatusViewModel>>(bLogs);
                }

                result.Data = listBLog.OrderByDescending(bl => bl.Booking.DateBegin).ToList();
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }
        #endregion

        public ResultModel CheckBalance(Guid bookingId)
        {
            var result = new ResultModel();
            try
            {

                var newBooking = _dbContext.Bookings.FirstOrDefault(b => b.Id == bookingId);

                var oldBookings = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                        .Include(bl => bl.Booking.Employee)
                                                        .Include(bl => bl.Booking.Province)
                                                        .Include(bl => bl.Booking.District)
                                                        .Include(bl => bl.Booking.Ward)
                                                        .Where(bl => bl.Booking.CustomerId == newBooking.CustomerId && bl.Booking.IsDisable == false
                                                                  && bl.IsDisable == false && bl.BookingId != bookingId)
                                                        .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                                        .Where(bl => bl.BookingStatusId == ConstBookingStatus.WAITING ||
                                                                     bl.BookingStatusId == ConstBookingStatus.PROCESSING ||
                                                                     bl.BookingStatusId == ConstBookingStatus.CONFIRM_WAITING ||
                                                                     bl.BookingStatusId == ConstBookingStatus.RE_CLEANING).ToList();

                var userBalance = _dbContext.Users.FirstOrDefault(u => u.Id == newBooking.CustomerId).Balance;


                bool isEnoughBalance = userBalance - newBooking.TotalPrice - oldBookings.Sum(b => b.Booking.TotalPrice) >= 0;

                result.Data = isEnoughBalance;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        #region Random or Select Emp NEW

        public async Task<ResultModel> Create(BookingCreateModel model, string customerId, bool isRandomEmployee)
        {
            var result = new ResultModel();
            try
            {
                var booking = _mapper.Map<BookingCreateModel, Booking>(model);
                booking.CustomerId = customerId;
                if (isRandomEmployee)
                {
                    booking.EmployeeId = null;
                }

                _dbContext.Add(booking);
                _dbContext.SaveChanges();


                string bLogStatusId;
                string bLogDescription;

                string notiDescription;

                bool isEnoughBalance = (bool)CheckBalance(booking.Id).Data;
                if (!isEnoughBalance)
                {
                    bLogStatusId = ConstBookingStatus.BALANCE_NOT_ENOUGH;
                    bLogDescription = "Không đủ số dư để đặt lịch";

                    // thong bao không đủ tiền đạt dịch vụ dọn dẹp
                    notiDescription = "Bạn không đủ số dư để đặt dịch vụ dọn dẹp! vui lòng nạp thêm số dư";
                }
                else
                {
                    if (isRandomEmployee)
                    {
                        bLogStatusId = ConstBookingStatus.CREATE_SUCCESS;
                        bLogDescription = "Tạo đặt lịch dọn dẹp thành công";
                    }
                    else
                    {
                        //BookingLog
                        bLogStatusId = ConstBookingStatus.WAITING;
                        bLogDescription = "Đặt lịch thành công";

                        Conversation conv = new Conversation()
                        {
                            BookingId = booking.Id
                        };
                        _dbContext.Add(conv);
                        _dbContext.SaveChanges();
                    }

                    // thong bao dat lich thanh cong
                    notiDescription = "Đặt dịch vụ vệ sinh thành công thành công!";
                }

                // Write log for booking
                BookingLogCreateModel blcModel = new BookingLogCreateModel()
                {
                    BookingId = booking.Id,
                    BookingStatusId = bLogStatusId,
                    Description = bLogDescription
                };
                _bookingLogService.Create(blcModel);

                // thong bao cho khach hang ve dat lich
                NotificationCreateModel notiCus = new NotificationCreateModel()
                {
                    BookingId = booking.Id,
                    UserId = booking.CustomerId,
                    Type = ConstNotificationType.BOOKING,
                    Description = notiDescription
                };
                _notificationService.Create(notiCus, false);

                if (isEnoughBalance == true && isRandomEmployee == true)
                {
                    // thong bao cho admin ve dat lich
                    NotificationCreateModel notiModel = new NotificationCreateModel()
                    {
                        BookingId = booking.Id,
                        Type = ConstNotificationType.BOOKING,
                        Description = "Có 1 đặt lịch vệ sinh mới đang chờ nhân viên"
                    };
                    await _notificationService.CreateForAdmin(notiModel, true);
                }
                else if (isEnoughBalance == true && isRandomEmployee == false)
                {
                    // thong bao cho nhan vien ve dat lich
                    NotificationCreateModel notiModel = new NotificationCreateModel()
                    {
                        BookingId = booking.Id,
                        UserId = booking.EmployeeId,
                        Type = ConstNotificationType.BOOKING,
                        Description = "Bạn có 1 đặt lịch vệ sinh mới"
                    };
                    await _notificationService.CreateForAdmin(notiModel, true);
                }

                BookingAfterCreatingModel bacModel = new BookingAfterCreatingModel()
                {
                    BookingId = booking.Id,
                    BookingStatusId = isEnoughBalance ? ConstBookingStatus.WAITING : ConstBookingStatus.BALANCE_NOT_ENOUGH
                };

                result.Data = bacModel;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> AssignWorkToEmployeeAsync(Guid bookingId, string empId)
        {
            var result = new ResultModel();
            try
            {
                BookingAssigningEmployeeModel returnModel = new BookingAssigningEmployeeModel();

                var booking = _dbContext.Bookings.FirstOrDefault(b => b.Id == bookingId && b.IsDisable == false);
                if (booking == null)
                {
                    returnModel.Message = "Id đặt lịch không hợp lệ";
                }

                var emp = _dbContext.Users.FirstOrDefault(u => u.Id == empId && u.IsDisable == false);
                if (emp == null)
                {
                    returnModel.Message = "Id nhân viên không hợp lệ";
                }

                booking.EmployeeId = emp.Id;
                booking.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(booking);

                Conversation conv = new Conversation()
                {
                    BookingId = booking.Id
                };
                _dbContext.Add(conv);
                _dbContext.SaveChanges();

                // Write log for booking
                BookingLogCreateModel blcModel = new BookingLogCreateModel()
                {
                    BookingId = booking.Id,
                    BookingStatusId = ConstBookingStatus.WAITING,
                    Description = "Điều phối nhân viên thành công"
                };
                _bookingLogService.Create(blcModel);

                // thong bao cho nhân viên về dat lich
                NotificationCreateModel notiEmp = new NotificationCreateModel()
                {
                    BookingId = booking.Id,
                    UserId = booking.EmployeeId,
                    Type = ConstNotificationType.BOOKING,
                    Description = "Bạn có 1 đặt lịch vệ sinh mới"
                };
                _notificationService.Create(notiEmp, true);

                // thong bao cho nhân viên về dat lich
                NotificationCreateModel notiCus = new NotificationCreateModel()
                {
                    BookingId = booking.Id,
                    UserId = booking.CustomerId,
                    Type = ConstNotificationType.BOOKING,
                    Description = "Đã tìm được nhân viên cho đặt lịch của bạn"
                };
                _notificationService.Create(notiCus, true);

                returnModel.IsSuceed = true;
                returnModel.Message = "Thành công";

                await _bookingHub.ChangeBooking(booking.CustomerId, booking.EmployeeId);


                result.Data = returnModel;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel CheckEmployeeSuitable(string empCode, BookingCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var emp = _dbContext.Users.FirstOrDefault(e => e.EmployeeCode == empCode && e.IsDisable == false);

                BookingAfterCheckingEmployeeFree afterChecking = new BookingAfterCheckingEmployeeFree();

                if (emp == null)
                {
                    afterChecking.IsEmployeeSuitable = false;
                    afterChecking.EmployeeStatus = "Nhân viên không tồn tại";
                }
                else
                {
                    DatetimeNewBookingModel datetimeModel = new DatetimeNewBookingModel()
                    {
                        NewBookingDate = model.DateBegin.Date,
                        NewBookingBegin = model.DateBegin.TimeOfDay,
                        NewBookingEnd = model.DateBegin.TimeOfDay
                          .Add(new TimeSpan(0, model.EstimatedTime + _algorithmService.GetEstimatedMovingTime(model.DateBegin), 0))
                    };

                    Booking newBooking = _mapper.Map<BookingCreateModel, Booking>(model);
                    bool checkingEmpFree = _algorithmService.CheckEmployeeFree(emp.Id, datetimeModel, newBooking).Check;

                    if (checkingEmpFree)
                    {
                        afterChecking.IsEmployeeSuitable = true;
                        afterChecking.EmployeeStatus = "Nhân viên CÓ thể nhận đặt lịch này";
                    }
                    else
                    {
                        afterChecking.IsEmployeeSuitable = false;
                        afterChecking.EmployeeStatus = "Nhân viên KHÔNG thể nhận đặt lịch này";
                    }
                    afterChecking.EmployeeId = emp.Id;
                    afterChecking.Employee = _mapper.Map<User, EmployeeBookingDetailsModel>(emp);
                }

                result.Data = afterChecking;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }
        #endregion

        public async Task<ResultModel> CheckBeforeUpdateServiceDetails(Guid bookingId, BookingUpdateServiceDetailsModel model, string role)
        {
            var result = new ResultModel();
            try
            {

                var bLog = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                             .Where(bl => bl.BookingId == bookingId).ToList()
                                             .OrderByDescending(bl => bl.DateCreated).FirstOrDefault();

                if (bLog == null)
                {
                    throw new Exception("Invalid Id");
                }

                BookingAfterUpdateServiceModel bausModel = new BookingAfterUpdateServiceModel
                {
                    BookingId = bLog.BookingId
                };

                string logDescription = "";

                string notiUserId = "";
                string notiDescription = "";

                var booking = bLog.Booking;

                bool isEnoughBalance = (bool)CheckBalance(bLog.BookingId).Data;
                if (isEnoughBalance)
                {
                    if (role == ConstRole.EMPLOYEE)
                    {
                        ResultModel updateResultEmp = UpdateServiceDetails(booking, model);
                        if (updateResultEmp.Succeed)
                        {
                            // write log 
                            logDescription = "Thay đổi dịch vụ thành công";

                            // noti
                            notiUserId = booking.CustomerId;
                            notiDescription = "Nhân viên đã thay đổi dịch vụ. Vui lòng kiểm tra lại";

                            // retrun result
                            bausModel.IsUpdateSuccess = true;
                            bausModel.Message = "Đã cập nhật dịch vụ mới";
                        }
                        else
                        {
                            throw new Exception(updateResultEmp.ErrorMessage);
                        }
                    }
                    else if (role == ConstRole.CUSTOMER)
                    {
                        DatetimeNewBookingModel datetimeModel = new DatetimeNewBookingModel()
                        {
                            NewBookingDate = booking.DateBegin.Value.Date,
                            NewBookingBegin = booking.DateBegin.Value.TimeOfDay,
                            NewBookingEnd = booking.DateBegin.Value.TimeOfDay
                                 .Add(new TimeSpan(0, model.EstimatedTime + _algorithmService.GetEstimatedMovingTime(booking.DateBegin.Value), 0))
                        };

                        BookingCheckEmployeeModel bcEmpFree = _algorithmService.CheckEmployeeFree(booking.EmployeeId, datetimeModel, booking);

                        // nếu nhân viên rãnh
                        if (bcEmpFree.Check)
                        {
                            ResultModel updateResultCusFree = UpdateServiceDetails(booking, model);
                            if (updateResultCusFree.Succeed)
                            {
                                // write log 
                                logDescription = "Thay đổi dịch vụ thành công";

                                // noti
                                notiUserId = booking.EmployeeId;
                                notiDescription = "Khách hàng đã thay đổi dịch vụ. Vui lòng kiểm tra lại";

                                // retrun result
                                bausModel.IsUpdateSuccess = true;
                                bausModel.Message = "Đã cập nhật dịch vụ mới";
                            }
                            else
                            {
                                throw new Exception(updateResultCusFree.ErrorMessage);
                            }
                        }
                        else // nếu nhân viên không rãnh thì thông báo cho khách hàng
                        {
                            // write log 
                            logDescription = "Thay đổi dịch vụ thất bại, nhân viên không còn phù hợp";

                            // retrun result
                            bausModel.IsUpdateSuccess = false;
                            bausModel.Message = "Nhân viên hiện tại không phù hợp với thay đổi của bạn. Vui lòng đặt lại dịch vụ mới";

                        }
                    }
                }
                else
                {
                    logDescription = "Không đủ số dư để thay đổi dịch vụ";

                    // retrun result
                    bausModel.IsUpdateSuccess = false;
                    if (role == ConstRole.CUSTOMER)
                    {
                        bausModel.Message = "Bạn không đủ số dư để thay đổi dịch vụ";
                    }
                    else if (role == ConstRole.EMPLOYEE)
                    {
                        bausModel.Message = "Khách hàng không đủ số dư để thay đổi dịch vụ";
                    }
                }

                #region Log, Noti
                // Write log for booking
                BookingLogCreateModel blcModel = new BookingLogCreateModel()
                {
                    BookingId = bLog.BookingId,
                    BookingStatusId = bLog.BookingStatusId,
                    Description = logDescription
                };
                _bookingLogService.Create(blcModel);

                // noti for update
                NotificationCreateModel notiModel = new NotificationCreateModel()
                {
                    BookingId = bLog.BookingId,
                    UserId = notiUserId,
                    Type = ConstNotificationType.BOOKING,
                    Description = notiDescription
                };
                _notificationService.Create(notiModel);
                #endregion

                await _bookingHub.ChangeBooking(booking.CustomerId, booking.EmployeeId);

                result.Data = bausModel;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel UpdateServiceDetails(Booking booking, BookingUpdateServiceDetailsModel model)
        {
            var result = new ResultModel();
            try
            {
                booking.EstimatedTime = model.EstimatedTime;
                booking.TotalPrice = model.TotalPrice;
                booking.UseCompanyTool = model.UseCompanyTool;
                booking.UseCompanyToolPrice = model.UseCompanyTool ? model.UseCompanyToolPrice : 0;
                //booking.UseCompanyToolPrice =  model.UseCompanyToolPrice;
                booking.DateUpdated = DateTime.UtcNow.AddHours(7);

                foreach (var sB in model.ServiceBookings)
                {
                    var serviceBooking = _dbContext.ServiceBookings.FirstOrDefault(s => s.BookingId == booking.Id && s.ServiceId == sB.ServiceId);
                    if (serviceBooking == null)
                    {
                        ServiceBooking newServiceBooking = new ServiceBooking()
                        {
                            ServiceId = sB.ServiceId,
                            BookingId = booking.Id,
                            Quantity = sB.Quantity,
                            UnitPrice = sB.UnitPrice,
                            EstiamtedMinutes = sB.EstiamtedMinutes
                        };
                        _dbContext.Add(newServiceBooking);
                    }
                    else
                    {
                        serviceBooking.Quantity = sB.Quantity;
                        serviceBooking.UnitPrice = sB.UnitPrice;
                        serviceBooking.EstiamtedMinutes = sB.EstiamtedMinutes;
                        serviceBooking.IsDisable = sB.IsDisable;
                        serviceBooking.DateUpdated = DateTime.UtcNow.AddHours(7);

                        _dbContext.Update(serviceBooking);
                    }
                }

                _dbContext.Update(booking);
                _dbContext.SaveChanges();

                result.Data = booking.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> UpdateBookingStatus(Guid id, string statusId, string bookingDescription = null)
        {
            var result = new ResultModel();
            try
            {
                var booking = _dbContext.Bookings.FirstOrDefault(b => b.Id == id);

                if (booking == null)
                {
                    throw new Exception("Invalid Booking Id");
                }

                if (bookingDescription != null)
                {
                    booking.Description = bookingDescription;
                    booking.DateUpdated = DateTime.UtcNow.AddHours(7);

                    _dbContext.Update(booking);
                    _dbContext.SaveChanges();
                }

                BookingLogCreateModel blcModel = new BookingLogCreateModel()
                {
                    BookingId = id,
                    BookingNote = bookingDescription,
                    BookingStatusId = statusId,
                    Description = "Đặt dịch vụ dọn dẹp đã thay đổi trạng thái thành \"" +
                                  _dbContext.BookingStatuses.FirstOrDefault(bs => bs.Id == statusId).Description + "\""
                };
                Guid bLogId = (Guid)_bookingLogService.Create(blcModel).Data;

                string notiDescription = "";
                string notiUserId = "";

                if (statusId == ConstBookingStatus.REJECTED)
                {
                    notiDescription = "Đặt lịch dọn dẹp đã bị từ chối.";
                    notiUserId = booking.CustomerId;
                }
                else if (statusId == ConstBookingStatus.CANCELLED || statusId == ConstBookingStatus.DONE)
                {
                    // Update date end of booking
                    booking.DateEnd = DateTime.UtcNow.AddHours(7);

                    _dbContext.Update(booking);
                    _dbContext.SaveChanges();

                    if (statusId == ConstBookingStatus.CANCELLED)
                    {
                        notiDescription = "Đặt lịch dọn dẹp đã bị huỷ.";
                        notiUserId = booking.EmployeeId;
                    }
                    else if (statusId == ConstBookingStatus.DONE)
                    {
                        notiDescription = "Đặt lịch dọn dẹp đã hoàn thành";
                        notiUserId = booking.EmployeeId;

                        // Transaction Customer
                        TransactionCreateModel cusTrans = new TransactionCreateModel()
                        {
                            UserId = booking.CustomerId,
                            Deposit = -booking.TotalPrice,
                            BookingId = booking.Id,
                            Description = "Thanh toán đặt dịch vụ",
                            Type = ConstTransactionType.USER
                        };
                        _transactionService.Create(cusTrans);

                        // Transaction Company
                        TransactionCreateModel companyTrans = new TransactionCreateModel()
                        {
                            UserId = _dbContext.Users.FirstOrDefault(u => u.UserName == ConstGeneral.COMPANY_USERNAME).Id,
                            Deposit = booking.TotalPrice,
                            BookingId = booking.Id,
                            Description = "Đơn mã " + booking.Id + " đã hoàn thành",
                            Type = ConstTransactionType.COMPANY
                        };
                        _transactionService.Create(companyTrans);

                    }
                }
                else if (statusId == ConstBookingStatus.RE_CLEANING)
                {
                    notiDescription = "Khách hàng yêu cầu dọn lại";
                    notiUserId = booking.EmployeeId;
                }

                // thong bao thay doi trang thai yeu cau
                NotificationCreateModel notiModel = new NotificationCreateModel()
                {
                    BookingId = booking.Id,
                    UserId = notiUserId,
                    Type = ConstNotificationType.BOOKING,
                    Description = notiDescription
                };
                _notificationService.Create(notiModel);

                result.Data = bLogId;
                result.Succeed = true;

                //if (statusId == ConstBookingStatus.REJECTED)
                //{
                //    var conv = _dbContext.Conversations.FirstOrDefault(c => c.BookingId == id);
                //    conv.IsDisable = true;
                //    conv.DateUpdated = DateTime.UtcNow.AddHours(7);

                //    _dbContext.Update(conv);
                //    _dbContext.SaveChanges();

                //    // Find employee against
                //    await FindSuitableEmployee(id);
                //}

                //await _bookingHub.ChangeBooking(booking.CustomerId, booking.EmployeeId, statusId);

                await _bookingHub.ChangeBooking(booking.CustomerId, booking.EmployeeId);
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Delete(Guid id)
        {
            var result = new ResultModel();
            try
            {
                var booking = _dbContext.Bookings.FirstOrDefault(s => s.Id == id);

                if (booking == null)
                {
                    throw new Exception("Invalid Id");
                }

                booking.IsDisable = true;
                booking.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(booking);
                _dbContext.SaveChanges();

                result.Data = booking.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        #region Comment Backup Fn

        //public async Task<ResultModel> UpdateBookingLocation(Guid id, BookingUpdateLocationModel model)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        var bLog = _dbContext.BookingLogs.Include(bl => bl.Booking)
        //                                          .Where(bl => bl.BookingId == id).ToList()
        //                                          .OrderByDescending(bl => bl.DateCreated).FirstOrDefault();

        //        string bookingStatusId = bLog.BookingStatusId;

        //        var booking = bLog.Booking;

        //        if (booking == null)
        //        {
        //            throw new Exception("Invalid Id");
        //        }

        //        booking.Description = model.Description;
        //        booking.DateBegin = model.DateBegin;
        //        booking.ProvinceId = model.ProvinceId;
        //        booking.DistrictId = model.DistrictId;
        //        booking.WardId = model.WardId;
        //        booking.Address = model.Address;
        //        booking.AddressDetail = model.AddressDetail;
        //        booking.Latitude = model.Latitude;
        //        booking.Longitude = model.Longitude;
        //        booking.DateUpdated = DateTime.UtcNow.AddHours(7);

        //        _dbContext.Update(booking);
        //        _dbContext.SaveChanges();

        //        result.Data = booking.Id;
        //        result.Succeed = true;

        //        // Write log for booking
        //        BookingLogCreateModel blcModel = new BookingLogCreateModel()
        //        {
        //            BookingId = booking.Id,
        //            BookingNote = booking.Description,
        //            BookingStatusId = bookingStatusId,
        //            Description = "Cập nhật địa điểm làm việc thành công"
        //        };
        //        _bookingLogService.Create(blcModel);

        //        // thong bao thay đổi ngày giờ địa điểm làm việc
        //        NotificationCreateModel notiModel = new NotificationCreateModel()
        //        {
        //            BookingId = booking.Id,
        //            UserId = booking.EmployeeId,
        //            Type = ConstNotificationType.BOOKING,
        //            Description = "Khách hàng đã thay đổi địa điểm, thời gian làm việc, yêu cầu dọn dẹp này có thể không còn phù hợp với bạn nữa, vui lòng kiểm tra lại"
        //        };
        //        _notificationService.Create(notiModel);

        //        // tìm lại nhân viên
        //        await ReFindSuitableEmployee(id);
        //    }
        //    catch (Exception e)
        //    {
        //        result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        //    }
        //    return result;
        //}


        //public ResultModel RebookingEmployee(BookingEmployeeAgainCreateModel model, string customerId)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        var booking = _mapper.Map<BookingEmployeeAgainCreateModel, Booking>(model);
        //        booking.CustomerId = customerId;

        //        _dbContext.Add(booking);
        //        _dbContext.SaveChanges();

        //        // Write log for new booking
        //        BookingLogCreateModel blcModel = new BookingLogCreateModel()
        //        {
        //            BookingId = booking.Id,
        //            BookingNote = booking.Description,
        //            BookingStatusId = ConstBookingStatus.PAYING,
        //            Description = "Đặt lịch dọn dẹp thành công"
        //        };
        //        _bookingLogService.Create(blcModel);

        //        bool isEnoughBalance = (bool)CheckBalance(booking.Id).Data;

        //        string bookingStatusId = isEnoughBalance ? ConstBookingStatus.WAITING : ConstBookingStatus.CONFIRM_WAITING;
        //        // Write log after check balance
        //        BookingLogCreateModel blcModelAfter = new BookingLogCreateModel()
        //        {
        //            BookingId = booking.Id,
        //            BookingNote = booking.Description,
        //            BookingStatusId = bookingStatusId,
        //            Description = isEnoughBalance ? "Đặt lịch dọn dẹp thành công" : "Không đủ số dư để thanh toán"
        //        };
        //        _bookingLogService.Create(blcModelAfter);

        //        BookingAfterCreatingModel bacModel = new BookingAfterCreatingModel()
        //        {
        //            BookingId = booking.Id,
        //            BookingStatusId = bookingStatusId
        //        };

        //        result.Data = bacModel;
        //        result.Succeed = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        //    }
        //    return result;
        //}
        #endregion


        #region Random or Select Employee OLD
        public async Task<ResultModel> FindSuitableEmployee(Guid bookingId)
        {
            var result = new ResultModel();
            try
            {
                var booking = _dbContext.Bookings.FirstOrDefault(b => b.Id == bookingId);

                string notiDescription;
                string bLogStatusId;
                string bLogDescription;

                string empId = await _algorithmService.GetSuitableEmployee(booking);
                if (empId != null)
                {
                    booking.EmployeeId = empId;

                    _dbContext.Update(booking);
                    _dbContext.SaveChanges();

                    //Notification
                    notiDescription = "Đã tìm thấy nhân viên phù hợp cho bạn";

                    //BookingLog
                    bLogStatusId = ConstBookingStatus.WAITING;
                    bLogDescription = "Đã tìm thấy nhân viên phù hợp";

                    Conversation conv = new Conversation()
                    {
                        BookingId = bookingId
                    };
                    _dbContext.Add(conv);
                    _dbContext.SaveChanges();

                }
                else
                {
                    // Booking Notification
                    notiDescription = "Không tìm thấy nhân viên phù hợp cho bạn";

                    // Booking Log
                    bLogStatusId = ConstBookingStatus.EMPLOYEE_NOT_FOUND;
                    bLogDescription = "Không tìm thấy nhân viên phù hợp";
                }

                // Write log for booking
                BookingLogCreateModel blcModel = new BookingLogCreateModel()
                {
                    BookingId = booking.Id,
                    BookingStatusId = bLogStatusId,
                    Description = bLogDescription
                };
                _bookingLogService.Create(blcModel);

                // thong bao Tim nhan vien phu hop
                NotificationCreateModel notiCus = new NotificationCreateModel()
                {
                    BookingId = booking.Id,
                    UserId = booking.CustomerId,
                    Type = ConstNotificationType.BOOKING,
                    Description = notiDescription
                };
                _notificationService.Create(notiCus, false);

                // thong bao có booking mới cho emp
                if (empId != null)
                {
                    NotificationCreateModel notiEmp = new NotificationCreateModel()
                    {
                        BookingId = booking.Id,
                        UserId = booking.EmployeeId,
                        Type = ConstNotificationType.BOOKING,
                        Description = "Bạn có một đặt lịch dọn dẹp mới"
                    };
                    _notificationService.Create(notiEmp);
                }

                result.Data = empId;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> CheckBalanceAndFindEmployee(Guid bookingId, bool isRandomEmployee)
        {
            var result = new ResultModel();
            try
            {
                var booking = _dbContext.Bookings.FirstOrDefault(b => b.Id == bookingId);

                string bLogStatusId;
                string bLogDescription;

                bool findEmp = false;

                bool isEnoughBalance = (bool)CheckBalance(bookingId).Data;
                if (!isEnoughBalance)
                {
                    bLogStatusId = ConstBookingStatus.BALANCE_NOT_ENOUGH;
                    bLogDescription = "Không đủ số dư để đặt lịch";

                    result.Data = ConstBookingStatus.BALANCE_NOT_ENOUGH;

                    // thong bao không đủ tiền đạt dịch vụ dọn dẹp
                    NotificationCreateModel notiCus = new NotificationCreateModel()
                    {
                        BookingId = booking.Id,
                        UserId = booking.CustomerId,
                        Type = ConstNotificationType.BOOKING,
                        Description = "Bạn không đủ số dư để đặt dịch vụ dọn dẹp! vui lòng nạp thêm số dư"
                    };
                    _notificationService.Create(notiCus, false);
                }
                else
                {
                    if (isRandomEmployee)
                    {
                        bLogStatusId = ConstBookingStatus.FINDING_EMPLOYEE;
                        bLogDescription = "Đang tìm nhân viên";

                        findEmp = true;
                    }
                    else
                    {
                        //BookingLog
                        bLogStatusId = ConstBookingStatus.WAITING;
                        bLogDescription = "Đặt lịch thành công";
                        result.Data = ConstBookingStatus.WAITING;

                        Conversation conv = new Conversation()
                        {
                            BookingId = bookingId
                        };
                        _dbContext.Add(conv);
                        _dbContext.SaveChanges();

                        // thong bao không đủ tiền đạt dịch vụ dọn dẹp
                        NotificationCreateModel notiCus = new NotificationCreateModel()
                        {
                            BookingId = booking.Id,
                            UserId = booking.CustomerId,
                            Type = ConstNotificationType.BOOKING,
                            Description = "Đặt dịch vụ vệ sinh thành công thành công!"
                        };
                        _notificationService.Create(notiCus, false);
                    }
                }

                // Write log for booking
                BookingLogCreateModel blcModel = new BookingLogCreateModel()
                {
                    BookingId = booking.Id,
                    BookingStatusId = bLogStatusId,
                    Description = bLogDescription
                };
                _bookingLogService.Create(blcModel);

                if (findEmp)
                {
                    ResultModel rsModel = await FindSuitableEmployee(booking.Id);

                    result.Data = (rsModel.Data != null) ? ConstBookingStatus.WAITING : ConstBookingStatus.EMPLOYEE_NOT_FOUND;
                }

                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> CreateOld(BookingCreateModel model, string customerId, bool isRandomEmployee)
        {
            var result = new ResultModel();
            try
            {
                var booking = _mapper.Map<BookingCreateModel, Booking>(model);
                booking.CustomerId = customerId;
                if (isRandomEmployee)
                {
                    booking.EmployeeId = null;
                }

                _dbContext.Add(booking);
                _dbContext.SaveChanges();

                // Write log for booking
                BookingLogCreateModel blcModel = new BookingLogCreateModel()
                {
                    BookingId = booking.Id,
                    BookingNote = booking.Description,
                    BookingStatusId = ConstBookingStatus.CREATE_SUCCESS,
                    Description = "Tạo đặt lịch dọn dẹp thành công"
                };
                _bookingLogService.Create(blcModel);

                ResultModel rsModel = await CheckBalanceAndFindEmployee(booking.Id, isRandomEmployee);

                BookingAfterCreatingModel bacModel = new BookingAfterCreatingModel()
                {
                    BookingId = booking.Id,
                    BookingStatusId = rsModel.Data.ToString()
                };

                result.Data = bacModel;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> ReFindSuitableEmployee(Guid bookingId)
        {
            var result = new ResultModel();
            try
            {
                ResultModel rsModel = await CheckBalanceAndFindEmployee(bookingId, true);

                BookingAfterCreatingModel bacModel = new BookingAfterCreatingModel()
                {
                    BookingId = bookingId,
                    BookingStatusId = rsModel.Data.ToString()
                };

                result.Data = bacModel;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }
        #endregion

    }
}
