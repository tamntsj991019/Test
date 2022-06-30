using AutoMapper;
using Data.ConstData;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Core
{
    public interface INotificationService
    {
        ResultModel Get(Guid? id);
        ResultModel GetUserNotification(string userId, string role);
        ResultModel CountUnSeen(string userId);

        ResultModel Create(NotificationCreateModel model, bool isSentSignalR = true);
        Task<ResultModel> CreateForAdmin(NotificationCreateModel model, bool isSentSignalR = true);

        ResultModel Update(Guid id, NotificationUpdateModel model);
        ResultModel Delete(Guid id);
        ResultModel Seen(Guid id);
        ResultModel SeenAll(string userId);
        ResultModel Test(string title, string description, int count);
    }
    public class NotificationService : INotificationService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly INotificationHub _notificationHub;
        private readonly UserManager<User> _userManager;

        public NotificationService(IMapper mapper, AppDbContext dbContext, INotificationHub notificationHub, UserManager<User> userManager)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _notificationHub = notificationHub;
            _userManager = userManager;
        }

        public ResultModel Test(string title, string description, int count)
        {
            var result = new ResultModel();
            try
            {
                string adminId = "a1c081c8-eb8a-49c9-aef1-admin0000001";

                NotificationPushViewModel notiPushViewModel = new NotificationPushViewModel()
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Description = description
                };

                _notificationHub.NewNotification(notiPushViewModel, adminId);
                _notificationHub.NewNotificationCount(count, adminId);

                result.Data = notiPushViewModel;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Get(Guid? id)
        {
            var result = new ResultModel();
            try
            {
                var notifications = _dbContext.Notifications.Include(n => n.Booking).Where(s => id == null || s.Id == id).ToList();

                result.Data = _mapper.Map<List<Notification>, List<NotificationViewModel>>(notifications);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetUserNotification(string userId, string role)
        {
            var result = new ResultModel();
            try
            {
                var notifications = _dbContext.Notifications.Include(n => n.Booking)
                                                            .Include(n => n.Feedback)
                                                            .Where(n => n.UserId == userId)
                                                            .Where(n => n.IsDisable == false)
                                                            .OrderByDescending(n => n.DateCreated).ToList();

                object listNoti = new List<object>();
                if (role == ConstRole.EMPLOYEE)
                {
                    listNoti = _mapper.Map<List<Notification>, List<NotificationEmployeeViewModel>>(notifications);
                }
                else if (role == ConstRole.CUSTOMER)
                {
                    listNoti = _mapper.Map<List<Notification>, List<NotificationCustomerViewModel>>(notifications);
                }
                else if (role == ConstRole.ADMIN)
                {
                    listNoti = _mapper.Map<List<Notification>, List<NotificationAdminViewModel>>(notifications);
                }

                result.Data = listNoti;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Create(NotificationCreateModel model, bool isSentSignalR = true)
        {
            var result = new ResultModel();
            try
            {
                var notification = _mapper.Map<NotificationCreateModel, Notification>(model);

                notification.Title = GetNotiTitle(notification.Type);

                _dbContext.Add(notification);
                _dbContext.SaveChanges();

                result.Data = notification.Id;
                result.Succeed = true;

                if (isSentSignalR)
                {
                    var notiPush = _dbContext.Notifications.FirstOrDefault(n => n.Id == notification.Id);
                    NotificationPushViewModel notiPushViewModel = _mapper.Map<Notification, NotificationPushViewModel>(notiPush);

                    _notificationHub.NewNotification(notiPushViewModel, model.UserId);
                    _notificationHub.NewNotificationCount((int)CountUnSeen(model.UserId).Data, model.UserId);
                }
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> CreateForAdmin(NotificationCreateModel model, bool isSentSignalR = true)
        {
            var result = new ResultModel();
            try
            {
                var admins = (await _userManager.GetUsersInRoleAsync(ConstRole.ADMIN))
                                                .Where(e => e.IsDisable == false).ToList();

                foreach (var ad in admins)
                {
                    string userId = ad.Id;

                    var notification = _mapper.Map<NotificationCreateModel, Notification>(model);

                    notification.UserId = userId;
                    notification.Title = GetNotiTitle(notification.Type);

                    _dbContext.Add(notification);
                    _dbContext.SaveChanges();

                    if (isSentSignalR)
                    {
                        var notiPush = _dbContext.Notifications.FirstOrDefault(n => n.Id == notification.Id);

                        NotificationPushViewModel notiPushViewModel = _mapper.Map<Notification, NotificationPushViewModel>(notiPush);

                        await _notificationHub.NewNotification(notiPushViewModel, userId);
                        await _notificationHub.NewNotificationCount((int)CountUnSeen(userId).Data, userId);
                    }
                }

                result.Data = "Successfully";
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel CountUnSeen(string userId)
        {
            var result = new ResultModel();
            try
            {
                var notifications = _dbContext.Notifications.Where(n => n.UserId == userId && n.Seen == false).ToList();

                result.Data = notifications.Count();
                result.Succeed = false;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Seen(Guid id)
        {
            var result = new ResultModel();
            try
            {
                var noti = _dbContext.Notifications.Include(n => n.Booking)
                                                     .Where(n => n.Id == id)
                                                     .Where(n => n.IsDisable == false)
                                                     .Where(n => n.Seen == false).FirstOrDefault();

                if (noti == null)
                {
                    throw new Exception("Invalid Id");
                }

                noti.Seen = true;

                _dbContext.Update(noti);
                _dbContext.SaveChanges();

                result.Data = "Successfully";
                result.Succeed = true;

                _notificationHub.NewNotificationCount((int)CountUnSeen(noti.UserId).Data, noti.UserId);

            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel SeenAll(string userId)
        {
            var result = new ResultModel();
            try
            {
                var noties = _dbContext.Notifications.Include(n => n.Booking)
                                                     .Where(n => n.UserId == userId)
                                                     .Where(n => n.IsDisable == false)
                                                     .Where(n => n.Seen == false).ToList();

                //if (noti == null)
                //{
                //    throw new Exception("Invalid Id");
                //}
                foreach (var noti in noties)
                {
                    noti.Seen = true;

                    _dbContext.Update(noti);
                }

                _dbContext.SaveChanges();

                result.Data = "Successfully";
                result.Succeed = true;

                _notificationHub.NewNotificationCount((int)CountUnSeen(userId).Data, userId);

            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Update(Guid id, NotificationUpdateModel model)
        {
            var result = new ResultModel();
            try
            {
                var notification = _dbContext.Notifications.FirstOrDefault(s => s.Id == id);

                if (notification == null)
                {
                    throw new Exception("Invalid Id");
                }

                notification = _mapper.Map<NotificationUpdateModel, Notification>(model);
                notification.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(notification);
                _dbContext.SaveChanges();

                result.Data = notification.Id;
                result.Succeed = true;
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
                var notification = _dbContext.Notifications.FirstOrDefault(s => s.Id == id);

                if (notification == null)
                {
                    throw new Exception("Invalid Id");
                }

                notification.IsDisable = true;
                notification.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(notification);
                _dbContext.SaveChanges();

                result.Data = notification.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public string GetNotiTitle(string type)
        {
            if (type == ConstNotificationType.BOOKING)
            {
                return "Đặt lịch dọn vệ sinh";
            }
            else if (type == ConstNotificationType.FEEDBACK)
            {
                return "Đánh giá";
            }
            else if (type == ConstNotificationType.DEPOSIT)
            {
                return "Nạp tiền";
            }
            else if (type == ConstNotificationType.REQUEST)
            {
                return "Cấp phát dụng cụ";
            }
            else
            {
                return "Không có tiêu đề";
            }
        }
    }
}
