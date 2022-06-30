using AutoMapper;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Services.Core
{
    public interface IScheduleService
    {
        ResultModel Get(Guid? id);
        ResultModel GetEnableByUserId(Guid? id, string userId);
        ResultModel Create(List<ScheduleCreateModel> model, string userId);
        ResultModel Update(Guid id, ScheduleUpdateModel model);
        ResultModel UpdateStatus(Guid id, bool isDisable, string userId);
    }
    public class ScheduleService : IScheduleService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly IBookingService _bookingService;

        public ScheduleService(IMapper mapper, AppDbContext dbContext, IBookingService bookingService)
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
                var Schedules = _dbContext.Schedules.Where(s => id == null || s.Id == id).ToList();

                result.Data = _mapper.Map<List<Schedule>, List<ScheduleViewModel>>(Schedules);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetEnableByUserId(Guid? id, string userId)
        {
            var result = new ResultModel();
            try
            {
                var schedules = _dbContext.Schedules.Include(s => s.Employee)
                                                    .Include(s => s.Intervals)
                                                    .Where(s => s.EmployeeId == userId)
                                                    .Where(s => s.Id == id || id == null)
                                                    .Where(s => s.IsDisable == false).ToList();

                foreach (var sche in schedules)
                {
                    sche.Intervals = sche.Intervals.Where(i => i.IsDisable == false)
                                                   .OrderBy(i => i.TimeFrom)
                                                   .ToList();
                }

                result.Data = _mapper.Map<List<Schedule>, List<ScheduleWithIntervalViewModel>>(schedules); 
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel IsExistedBooking(DateTime date, string userId)
        {
            var result = new ResultModel();
            try
            {
                bool isExistedBooking = false;

                var listBookingWaiting = (List<BookingEmployeeByStatusViewModel>)_bookingService.GetBookingEmployeeWaitingEnables(userId).Data;
                if (listBookingWaiting != null && listBookingWaiting.Count() > 0)
                {
                    foreach (var b in listBookingWaiting)
                    {
                        if (date.Date == b.DateBegin.Date)
                        {
                            isExistedBooking = true;
                            break;
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
                                isExistedBooking = true;
                                break;
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

        public ResultModel Create(List<ScheduleCreateModel> models, string userId)
        {
            var result = new ResultModel();
            try
            {
                bool isExistDateWorking = _dbContext.Schedules.Any(s => s.DateWorking.Date == models.FirstOrDefault().DateWorking.Date && 
                                                                        s.EmployeeId == userId && s.IsDisable == false);

                if (isExistDateWorking)
                {
                    throw new Exception("Existed DateWorking");
                }

                var schedule = _mapper.Map<ScheduleCreateModel, Schedule>(models.FirstOrDefault());

                schedule.EmployeeId = userId;
                schedule.DateWorking = schedule.DateWorking.Date;

                _dbContext.Add(schedule);
                _dbContext.SaveChanges();

                result.Data = "Successfully";
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Update(Guid id, ScheduleUpdateModel model)
        {
            var result = new ResultModel();
            try
            {
                var Schedule = _dbContext.Schedules.FirstOrDefault(s => s.Id == id);

                if (Schedule == null)
                {
                    throw new Exception("Invalid Id");
                }

                Schedule = _mapper.Map<ScheduleUpdateModel, Schedule>(model);
                Schedule.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(Schedule);
                _dbContext.SaveChanges();

                result.Data = Schedule.Id;
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
                var schedule = _dbContext.Schedules.FirstOrDefault(s => s.Id == id);

                if (schedule == null)
                {
                    throw new Exception("Invalid Id");
                }

                bool isExistedBooking = (bool)IsExistedBooking(schedule.DateWorking, userId).Data;

                if (isExistedBooking)
                {
                    result.Data = false;
                }
                else
                {
                    schedule.IsDisable = isDisable;
                    schedule.DateUpdated = DateTime.UtcNow.AddHours(7);

                    _dbContext.Update(schedule);
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
