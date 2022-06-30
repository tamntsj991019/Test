using AutoMapper;
using Data.ConstData;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Services.Core
{
    public interface IFeedbackService
    {
        //ResultModel Get(Guid? id);
        ResultModel GetFeedbacksForCustomer(string userId);
        ResultModel GetFeedbacksForEmployee(string userId);

        ResultModel Create(FeedbackCreateModel model, string userId, string role);
        //ResultModel Update(Guid id, FeedbackUpdateModel model);
        //ResultModel Delete(Guid id);
    }
    public class FeedbackService : IFeedbackService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly INotificationService _notificationService;

        public FeedbackService(IMapper mapper, AppDbContext dbContext, INotificationService notificationService)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        //public ResultModel Get(Guid? booking)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        var feedbacks = _dbContext.Feedbacks.Include(f => f.User)
        //                                            .Include(f => f.Booking)
        //                                            .Where(s => id == null || (s.Id == id).ToList();

        //        result.Data = _mapper.Map<List<Feedback>, List<FeedbackViewModel>>(feedbacks);
        //        result.Succeed = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        //    }
        //    return result;
        //}

        public ResultModel GetFeedbacksForCustomer(string userId)
        {
            var result = new ResultModel();
            try
            {
                var feedbacks = _dbContext.Feedbacks.Include(f => f.User).Include(f => f.Booking)
                                                    .Where(f => f.Booking.CustomerId == userId)
                                                    .Where(f => f.UserId != userId)
                                                    .Where(f => f.IsDisable == false)
                                                    .OrderByDescending(f => f.DateCreated).ToList();

                result.Data = _mapper.Map<List<Feedback>, List<FeedbackWithUserBookingViewModel>>(feedbacks);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetFeedbacksForEmployee(string userId)
        {
            var result = new ResultModel();
            try
            {
                var feedbacks = _dbContext.Feedbacks.Include(f => f.User).Include(f => f.Booking)
                                                    .Where(f => f.Booking.EmployeeId == userId)
                                                    .Where(f => f.UserId != userId)
                                                    .Where(f => f.IsDisable == false)
                                                    .OrderByDescending(f => f.DateCreated).ToList();

                result.Data = _mapper.Map<List<Feedback>, List<FeedbackWithUserBookingViewModel>>(feedbacks);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Create(FeedbackCreateModel model, string userId, string role)
        {
            var result = new ResultModel();
            try
            {
                var booking = _dbContext.Bookings.FirstOrDefault(b => b.Id == model.BookingId);

                if (booking == null)
                {
                    throw new Exception("Invalid BookingId");
                }

                var feedback = _mapper.Map<FeedbackCreateModel, Feedback>(model);
                feedback.UserId = userId;

                _dbContext.Add(feedback);
                _dbContext.SaveChanges();

                if (role == ConstRole.CUSTOMER)
                {
                    var settingJson = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.CALCULATE_CREDIT).Data;
                    StarCalculateModel starCalculateModel = JsonConvert.DeserializeObject<StarCalculateModel>(settingJson);

                    if (feedback.Rating != starCalculateModel.RatingPoint)
                    {
                        var emp = _dbContext.Users.FirstOrDefault(u => u.Id == booking.EmployeeId);
                        if (feedback.Rating > starCalculateModel.RatingPoint)
                        {
                            emp.EmployeeCredit += starCalculateModel.AbovePoint;
                            if (emp.EmployeeCredit > 100)
                            {
                                emp.EmployeeCredit = 100;
                            }
                        }
                        else if (feedback.Rating < starCalculateModel.RatingPoint)
                        {
                            emp.EmployeeCredit += starCalculateModel.UnderPoint;
                            if (emp.EmployeeCredit < 0)
                            {
                                emp.EmployeeCredit = 0;
                            }
                        }

                        _dbContext.Update(emp);
                        _dbContext.SaveChanges();
                    }

                    booking.IsCustomerFeedback = true;

                    string cusFullname = _dbContext.Users.FirstOrDefault(u => u.Id == booking.CustomerId).Fullname;

                    NotificationCreateModel notiModel = new NotificationCreateModel()
                    {
                        UserId = booking.EmployeeId,
                        Description = "Đánh giá từ " + cusFullname + ". Chi tiết trong chức năng Đánh giá",
                        FeedbackId = feedback.Id,
                        Type = ConstNotificationType.FEEDBACK

                    };
                    _notificationService.Create(notiModel, true);
                }
                else if(role == ConstRole.EMPLOYEE)
                {
                    booking.IsEmployeeFeedback = true;
                }
                booking.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(booking);
                _dbContext.SaveChanges();

                result.Data = feedback.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }
        //public ResultModel Update(Guid id, FeedbackUpdateModel model)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        var feedback = _dbContext.Feedbacks.FirstOrDefault(s => s.Id == id);

        //        if (feedback == null)
        //        {
        //            throw new Exception("Invalid Id");
        //        }

        //        //feedback = _mapper.Map<FeedbackUpdateModel, Feedback>(model);
        //        feedback.Rating = model.Rating;
        //        feedback.Description = model.Description;
        //        feedback.DateUpdated = DateTime.UtcNow.AddHours(7);

        //        _dbContext.Update(feedback);
        //        _dbContext.SaveChanges();

        //        result.Data = feedback.Id;
        //        result.Succeed = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        //    }
        //    return result;
        //}

        //public ResultModel Delete(Guid id)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        var feedback = _dbContext.Feedbacks.FirstOrDefault(s => s.Id == id);

        //        if (feedback == null)
        //        {
        //            throw new Exception("Invalid Id");
        //        }

        //        feedback.IsDisable = true;
        //        feedback.DateUpdated = DateTime.UtcNow.AddHours(7);

        //        _dbContext.Update(feedback);
        //        _dbContext.SaveChanges();

        //        result.Data = feedback.Id;
        //        result.Succeed = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        //    }
        //    return result;
        //}
    }
}
