using AutoMapper;
using Data.ConstData;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using Services.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Core
{
    public interface IRequestCleaningToolService
    {
        ResultModel GetRequestByStatus(string cleaningToolName, string statusId, int pageIndex, int pageSize);
        ResultModel GetAdminHistory(string cleaningToolName, int pageIndex, int pageSize);
        ResultModel GetEmployeeProcessing(string userId);
        ResultModel GetEmployeeHistory(string userId);
        Task<ResultModel> Create(List<RequestCleaningToolCreateModel> listModel, string userId);
        ResultModel UpdateStatus(Guid id, string statusId, string reasonReject = null);
        Task<ResultModel> Cancel(Guid id);
    }
    public class RequestCleaningToolService : IRequestCleaningToolService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly INotificationService _notificationService;

        private readonly Utils _utils = new Utils();

        public RequestCleaningToolService(IMapper mapper, AppDbContext dbContext, INotificationService notificationService)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        public ResultModel GetRequestByStatus(string cleaningToolName, string statusId, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var requestCleaningTools = _dbContext.RequestCleaningTools.Include(r => r.RequestStatus)
                                                                          .Include(r => r.CleaningTool)
                                                                          .Where(r => r.RequestStatusId == statusId)
                                                                          .OrderBy(r => r.DateCreated).ToList();

                List<RequestCleaningToolWithUserViewModel> listRequest = _mapper.Map<List<RequestCleaningTool>, List<RequestCleaningToolWithUserViewModel>>(requestCleaningTools);

                if (cleaningToolName != null && cleaningToolName != "")
                {
                    string nameInput = _utils.RepalceSignForVietnameseString(cleaningToolName).ToUpper();
                    listRequest = listRequest.Where(r => _utils.RepalceSignForVietnameseString(r.CleaningTool.Description)
                                                               .ToUpper().Contains(nameInput)).ToList();
                }

                result.Data = new PagingModel
                {
                    Total = listRequest.Count,
                    Data = listRequest.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetAdminHistory(string cleaningToolName, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var requestCleaningTools = _dbContext.RequestCleaningTools.Include(r => r.RequestStatus)
                                                                          .Include(r => r.CleaningTool)
                                                                          .Where(r => r.RequestStatusId == ConstRequestStatus.PROVIDED ||
                                                                                      r.RequestStatusId == ConstRequestStatus.CANCELLED ||
                                                                                      r.RequestStatusId == ConstRequestStatus.REJECTED)
                                                                          .OrderBy(r => r.DateCreated).ToList();

                List<RequestCleaningToolWithUserViewModel> listRequest = _mapper.Map<List<RequestCleaningTool>, List<RequestCleaningToolWithUserViewModel>>(requestCleaningTools);
                if (cleaningToolName != null && cleaningToolName != "")
                {
                    string nameInput = _utils.RepalceSignForVietnameseString(cleaningToolName).ToUpper();
                    listRequest = listRequest.Where(r => _utils.RepalceSignForVietnameseString(r.CleaningTool.Description)
                                                               .ToUpper().Contains(nameInput)).ToList();
                }

                result.Data = new PagingModel
                {
                    Total = listRequest.Count,
                    Data = listRequest.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetEmployeeProcessing(string userId)
        {
            var result = new ResultModel();
            try
            {
                var requestCleaningTools = _dbContext.RequestCleaningTools.Include(r => r.RequestStatus)
                                                                          .Include(r => r.CleaningTool)
                                                                          .Where(r => r.EmployeeId == userId && r.IsDisable == false)
                                                                          .Where(r => r.RequestStatusId == ConstRequestStatus.PENDING ||
                                                                                      r.RequestStatusId == ConstRequestStatus.APPROVED).ToList();

                result.Data = _mapper.Map<List<RequestCleaningTool>, List<RequestCleaningToolEmployeeViewModel>>(requestCleaningTools);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetEmployeeHistory(string userId)
        {
            var result = new ResultModel();
            try
            {
                var requestCleaningTools = _dbContext.RequestCleaningTools.Include(r => r.RequestStatus)
                                                                          .Include(r => r.CleaningTool)
                                                                          .Where(r => r.EmployeeId == userId && r.IsDisable == false)
                                                                          .Where(r => r.RequestStatusId == ConstRequestStatus.PROVIDED ||
                                                                                      r.RequestStatusId == ConstRequestStatus.CANCELLED ||
                                                                                      r.RequestStatusId == ConstRequestStatus.REJECTED)
                                                                          .ToList();

                result.Data = _mapper.Map<List<RequestCleaningTool>, List<RequestCleaningToolEmployeeViewModel>>(requestCleaningTools);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> Create(List<RequestCleaningToolCreateModel> listModel, string userId)
        {
            var result = new ResultModel();
            try
            {
                //listModel = listModel.DistinctBy(l => l.CleaningToolId).ToList();
                foreach (var model in listModel)
                {
                    var requestCleaningTool = _mapper.Map<RequestCleaningToolCreateModel, RequestCleaningTool>(model);
                    requestCleaningTool.EmployeeId = userId;

                    _dbContext.Add(requestCleaningTool);
                    _dbContext.SaveChanges();

                    // thong bao admin co yeu cau moi
                    NotificationCreateModel notiModel = new NotificationCreateModel()
                    {
                        UserId = userId,
                        RequestCleaningToolId = requestCleaningTool.Id,
                        Type = ConstNotificationType.REQUEST,
                        Description = "Có yêu cầu dụng cụ từ nhân viên"
                    };
                    await _notificationService.CreateForAdmin(notiModel);
                }

                result.Data = "Request Successfully";
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel UpdateStatus(Guid id, string statusId, string reasonReject = null)
        {
            var result = new ResultModel();
            try
            {
                var requestCleaningTool = _dbContext.RequestCleaningTools.FirstOrDefault(r => r.Id == id);

                if (requestCleaningTool == null)
                {
                    throw new Exception("Invalid Id");
                }
                var quantityCleaningToolStr = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.DISTRIBUTE_CLEANING_TOOL && s.IsDisable == false).Data;
                int quantityCleaningTool = int.Parse(quantityCleaningToolStr);

                var cleaningTool = _dbContext.CleaningTools.FirstOrDefault(c => c.Id == requestCleaningTool.CleaningToolId);

                bool isEnoughCleaningTool = false;
                bool isReturnModel = false;
                if (statusId == ConstRequestStatus.APPROVED)
                {
                    isReturnModel = true;
                    if (quantityCleaningTool <= cleaningTool.Quantity)
                    {
                        isEnoughCleaningTool = true;
                    }
                }
                else
                {
                    isEnoughCleaningTool = true;
                }

                if (isEnoughCleaningTool)
                {
                    requestCleaningTool.RequestStatusId = statusId;
                    requestCleaningTool.Description = reasonReject;
                    requestCleaningTool.DateUpdated = DateTime.UtcNow.AddHours(7);

                    _dbContext.Update(requestCleaningTool);
                    _dbContext.SaveChanges();

                    if (statusId == ConstRequestStatus.PROVIDED)
                    {
                        var userCleaningTool = _dbContext.UserCleaningTools.Where(u => u.CleaningToolId == requestCleaningTool.CleaningToolId)
                                                                           .Where(u => u.EmployeeId == requestCleaningTool.EmployeeId)
                                                                           .FirstOrDefault();

                        if (userCleaningTool != null)
                        {
                            userCleaningTool.Quantity += quantityCleaningTool;
                            userCleaningTool.DateUpdated = DateTime.UtcNow.AddHours(7);

                            _dbContext.Update(userCleaningTool);
                        }
                        else
                        {
                            UserCleaningTool uct = new UserCleaningTool()
                            {
                                CleaningToolId = requestCleaningTool.CleaningToolId,
                                EmployeeId = requestCleaningTool.EmployeeId,
                                Quantity = quantityCleaningTool
                            };

                            _dbContext.Add(quantityCleaningTool);
                        }

                        cleaningTool.Quantity -= quantityCleaningTool;
                        cleaningTool.DateUpdated = DateTime.UtcNow.AddHours(7);

                        _dbContext.SaveChanges();
                    }

                    if (statusId == ConstRequestStatus.APPROVED ||
                       statusId == ConstRequestStatus.PROVIDED ||
                       statusId == ConstRequestStatus.REJECTED)
                    {
                        string notiDescription;
                        string toolName = _dbContext.CleaningTools.FirstOrDefault(cl => cl.Id == requestCleaningTool.CleaningToolId).Description;
                        if (statusId == ConstRequestStatus.APPROVED)
                        {
                            notiDescription = "Yêu cầu cấp phát dụng cụ của bạn đã được chấp nhận";
                        }
                        else if (statusId == ConstRequestStatus.PROVIDED)
                        {
                            notiDescription = "Bạn đã được cấp " + toolName;
                        }
                        else
                        {
                            notiDescription = "Từ chối cấp phát " + toolName + ".\nLý do: " + reasonReject;
                        }

                        // thong bao thay doi trang thai yeu cau
                        NotificationCreateModel notiModel = new NotificationCreateModel()
                        {
                            RequestCleaningToolId = requestCleaningTool.Id,
                            UserId = requestCleaningTool.EmployeeId,
                            Type = ConstNotificationType.REQUEST,
                            Description = notiDescription
                        };
                        _notificationService.Create(notiModel);
                    }
                }

                if (isReturnModel)
                {
                    result.Data = new RequestUpdateStatusReturnModel()
                    {
                        Id = requestCleaningTool.Id,
                        IsUpdateSuccess = isEnoughCleaningTool,
                        Message = isEnoughCleaningTool ? "Thành công" : "Không đủ dụng cụ vệ sinh"
                    };
                }
                else
                {
                    result.Data = requestCleaningTool.Id;
                }

                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> Cancel(Guid id)
        {
            var result = new ResultModel();
            try
            {
                var requestCleaningTool = _dbContext.RequestCleaningTools.FirstOrDefault(r => r.Id == id);

                if (requestCleaningTool == null)
                {
                    throw new Exception("Invalid Id");
                }

                requestCleaningTool.RequestStatusId = ConstRequestStatus.CANCELLED;
                requestCleaningTool.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(requestCleaningTool);
                _dbContext.SaveChanges();

                // thong bao admin co yeu cau moi
                NotificationCreateModel notiModel = new NotificationCreateModel()
                {
                    RequestCleaningToolId = requestCleaningTool.Id,
                    Type = ConstNotificationType.REQUEST,
                    Description = "Nhân viên đã huỷ yêu cầu"
                };
                await _notificationService.CreateForAdmin(notiModel);

                result.Data = requestCleaningTool.Id;
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
