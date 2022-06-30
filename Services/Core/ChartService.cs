using Data.ConstData;
using Data.DbContext;
using Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Services.Core
{
    public interface IChartService
    {
        ResultModel GetChartBooking();
        ResultModel GetTransactionCompanyByDay();
    }

    public class ChartService : IChartService
    {
        private readonly AppDbContext _dbContext;

        public ChartService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region BOOKING
        public ResultModel GetNumberOfBookingByStatusId(string statusId)
        {
            var result = new ResultModel();
            try
            {
                result.Data = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                   .Where(bl => bl.IsDisable == false && bl.Booking.IsDisable == false)
                                                   .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                                   .Where(bl => bl.BookingStatusId == statusId).ToList().Count();
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetChartBooking()
        {
            var result = new ResultModel();
            try
            {
                BookingElement bookingWaiting = new BookingElement()
                {
                    Name = "Đang chờ xử lý",
                    Quantity = (int)GetNumberOfBookingByStatusId(ConstBookingStatus.WAITING).Data
                };
                
                BookingElement bookingCancelled = new BookingElement()
                {
                    Name = "Bị huỷ",
                    Quantity = (int)GetNumberOfBookingByStatusId(ConstBookingStatus.CANCELLED).Data
                };
                
                BookingElement bookingDone = new BookingElement()
                {
                    Name = "Hoàn thành",
                    Quantity = (int)GetNumberOfBookingByStatusId(ConstBookingStatus.DONE).Data
                };
                
                BookingElement bookingOther = new BookingElement()
                {
                    Name = "Khác",
                    Quantity = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                   .Where(bl => bl.IsDisable == false && bl.Booking.IsDisable == false)
                                                   .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                                   .Where(bl => bl.BookingStatusId != ConstBookingStatus.WAITING)
                                                   .Where(bl => bl.BookingStatusId != ConstBookingStatus.CANCELLED)
                                                   .Where(bl => bl.BookingStatusId != ConstBookingStatus.DONE)
                                                   .ToList().Count()
                };

                ChartBookingViewModel chartModel = new ChartBookingViewModel()
                {
                    BookingWaiting = bookingWaiting,
                    BookingCancelled = bookingCancelled,
                    BookingDone = bookingDone,
                    BookingOther = bookingOther
                };

                result.Data = chartModel;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }
        #endregion

        public ResultModel GetTransactionCompanyByDay()
        {
            var result = new ResultModel();
            try
            {
                List < ChartTransactionViewModel > data = new List<ChartTransactionViewModel>();
                for (int i = 0; i < 7; i++)
                {
                    var total = _dbContext.Transactions.Where(s => s.Type == ConstTransactionType.COMPANY && s.DateCreated.Value.Date == DateTime.Now.AddDays(-i).Date).Sum(s => s.Deposit);
                    ChartTransactionViewModel chart = new ChartTransactionViewModel() {
                        Date = DateTime.Now.AddDays(-i),
                        Total = total.Value
                    };
                    data.Add(chart);
                }
                result.Data = data;
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
