using AutoMapper;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using Services.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Services.Core
{
    public interface IIntervalService
    {
        ResultModel Get(Guid? id);
        ResultModel Create(IntervalCreateModel model);
        ResultModel Update(Guid id, IntervalUpdateModel model, string userId);
        ResultModel UpdateStatus(Guid id, bool isDisable, string userId);
    }
    public class IntervalService : IIntervalService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly IBookingService _bookingService;
        private readonly Utils _utils = new Utils();

        public IntervalService(IMapper mapper, AppDbContext dbContext, IBookingService bookingService)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _bookingService = bookingService;
        }

        public ResultModel Get(Guid? id)
        {
            var result = new ResultModel();
            try
            {
                var intervals = _dbContext.Intervals.Where(s => id == null || (s.IsDisable == false && s.Id == id)).ToList();

                result.Data = _mapper.Map<List<Interval>, List<IntervalViewModel>>(intervals);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel IsExistedBooking(DateTime date, string timeFrom, string timeTo, string userId)
        {
            var result = new ResultModel();
            try
            {
                bool isExistedBooking = false;
                TimeSpan timeFromInterval = _utils.ConverStringToTimeSpan(timeFrom);
                TimeSpan timeToInterval = _utils.ConverStringToTimeSpan(timeTo);

                var listBookingWaiting = (List<BookingEmployeeByStatusViewModel>)_bookingService.GetBookingEmployeeWaitingEnables(userId).Data;
                if (listBookingWaiting != null && listBookingWaiting.Count() > 0)
                {

                    foreach (var b in listBookingWaiting)
                    {
                        if (date.Date == b.DateBegin.Date)
                        {
                            TimeSpan timeBooking = b.DateBegin.TimeOfDay;
                            if (TimeSpan.Compare(timeFromInterval, timeBooking) <= 0 && TimeSpan.Compare(timeToInterval, timeBooking) >= 0)
                            {
                                isExistedBooking = true;
                                break;
                            }
                        }

                    }

                }

                if (isExistedBooking == false)
                {
                    var listBookingProcessing = (List<BookingLogEmployeeByStatusViewModel>)_bookingService.GetBookingEmployeeProcessingEnables(userId).Data;
                    if (listBookingProcessing != null && listBookingProcessing.Count() > 0)
                    {
                        foreach (var b in listBookingProcessing)
                        {
                            if (date.Date == b.Booking.DateBegin.Date)
                            {
                                TimeSpan timeBooking = b.Booking.DateBegin.TimeOfDay;
                                if (TimeSpan.Compare(timeFromInterval, timeBooking) <= 0 && TimeSpan.Compare(timeToInterval, timeBooking) >= 0)
                                {
                                    isExistedBooking = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                result.Data = isExistedBooking;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Create(IntervalCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var interval = _mapper.Map<IntervalCreateModel, Interval>(model);

                _dbContext.Add(interval);
                _dbContext.SaveChanges();

                result.Data = interval.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Update(Guid id, IntervalUpdateModel model, string userId)
        {
            var result = new ResultModel();
            try
            {
                var interval = _dbContext.Intervals.Include(i => i.Schedule).FirstOrDefault(s => s.Id == id && s.IsDisable == false);

                if (interval == null)
                {
                    throw new Exception("Invalid Id");
                }

                bool isExistedBooking = (bool)IsExistedBooking(interval.Schedule.DateWorking, interval.TimeFrom, interval.TimeTo, userId).Data;
                if (isExistedBooking)
                {
                    result.Data = false;
                }
                else
                {
                    interval.TimeFrom = model.TimeFrom;
                    interval.TimeTo = model.TimeTo;
                    interval.DateUpdated = DateTime.UtcNow.AddHours(7);

                    _dbContext.Update(interval);
                    _dbContext.SaveChanges();

                    result.Data = true;
                }
                
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel UpdateStatus(Guid id, bool isDisable, string userId)
        {
            var result = new ResultModel();
            try
            {
                var interval = _dbContext.Intervals.Include(i => i.Schedule).FirstOrDefault(s => s.Id == id && s.IsDisable == false);

                if (interval == null)
                {
                    throw new Exception("Invalid Id");
                }

                bool isExistedBooking = (bool)IsExistedBooking(interval.Schedule.DateWorking, interval.TimeFrom, interval.TimeTo, userId).Data;
                if (isExistedBooking)
                {
                    result.Data = false;
                }
                else
                {
                    interval.IsDisable = isDisable;
                    interval.DateUpdated = DateTime.UtcNow.AddHours(7);

                    _dbContext.Update(interval);
                    _dbContext.SaveChanges();

                    result.Data = true;
                }

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
