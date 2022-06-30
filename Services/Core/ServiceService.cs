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
    public interface IServiceService
    {
        ResultModel Get(Guid? id, int pageIndex, int pageSize);
        ResultModel GetByServiceGroupId(string name, Guid sgId, int pageIndex, int pageSize);
        ResultModel GetServiceWithCleaningTool();
        ResultModel Add(ServiceAddModel model);
        ResultModel Update(Guid id, ServiceUpdateModel model);
        ResultModel UpdateServiceStatus(Guid id, bool isDisable);
    }
    public class ServiceService : IServiceService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        private readonly Utils _utils = new Utils();

        public ServiceService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public ResultModel Get(Guid? id, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var services = _dbContext.Services.Where(s => id == null || s.Id == id).ToList();

                result.Data = new PagingModel
                {
                    Total = _mapper.Map<List<Service>, List<ServiceViewModel>>(services).Count,
                    Data = _mapper.Map<List<Service>, List<ServiceViewModel>>(services).Skip((pageIndex - 1) * pageSize).Take(pageSize).OrderBy(_d => _d.DateCreated).ToList()
                };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetByServiceGroupId(string name, Guid sgId, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var services = _dbContext.Services.Where(s => s.ServiceGroupId == sgId).OrderBy(s => s.DateCreated).ToList();

                List<ServiceViewModel> listService = _mapper.Map<List<Service>, List<ServiceViewModel>>(services);
                if (name != null && name != "")
                {
                    string nameInput = _utils.RepalceSignForVietnameseString(name).ToUpper();
                    listService = listService.Where(s => _utils.RepalceSignForVietnameseString(s.Description)
                                                               .ToUpper().Contains(nameInput)).ToList();
                }

                result.Data = new PagingModel
                {
                    Total = listService.Count,
                    Data = listService.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetServiceWithCleaningTool()
        {
            var result = new ResultModel();
            try
            {
                var services = _dbContext.Services.Include(s => s.ServiceCleaningTools)
                                                  .Where(s => s.ServiceGroup.IsDisable == false)
                                                  .Where(s => s.ServiceCleaningTools.Count > 0)
                                                  .Where(s => s.IsDisable == false).ToList();

                //_mapper.Map<List<Service>, List<ServiceForRequestViewModel>>(services);
                List<ServiceForRequestViewModel> listResult = new List<ServiceForRequestViewModel>();
                foreach (var ser in services)
                {
                    ServiceForRequestViewModel serModel = _mapper.Map<Service, ServiceForRequestViewModel>(ser);

                    var serTools = ser.ServiceCleaningTools.Where(s => s.IsDisable == false).Where(s => s.CleaningTool.IsDisable == false).ToList();
                    if (serTools.Count > 0)
                    {
                        foreach (var serTool in serTools)
                        {
                            var tool = _mapper.Map<CleaningTool, CleaningToolRequestViewModel>(serTool.CleaningTool);
                            serModel.CleaningTools.Add(tool);
                        }
                    }

                    listResult.Add(serModel);
                }

                // get cleaning tool khong co service
                var cleaningTools = _dbContext.CleaningTools.Include(s => s.ServiceCleaningTools)
                                                  .Where(s => s.ServiceCleaningTools.Count == 0)
                                                  .Where(s => s.IsDisable == false).ToList();

                ServiceForRequestViewModel sfrvModel = new ServiceForRequestViewModel()
                {
                    Description = "Khác",
                    CleaningTools = _mapper.Map<List<CleaningTool>, List<CleaningToolRequestViewModel>>(cleaningTools)
                };
                listResult.Add(sfrvModel);

                result.Data = listResult;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }


        public ResultModel Add(ServiceAddModel model)
        {
            var result = new ResultModel();
            try
            {
                var service = _mapper.Map<ServiceAddModel, Service>(model);

                _dbContext.Add(service);
                _dbContext.SaveChanges();

                result.Data = service.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Update(Guid id, ServiceUpdateModel model)
        {
            var result = new ResultModel();
            try
            {
                var service = _dbContext.Services.FirstOrDefault(s => s.Id == id);

                if (service == null)
                {
                    throw new Exception("Invalid Id");
                }

                service.UnitPrice = model.UnitPrice;
                service.ServiceGroupId = model.ServiceGroupId;
                service.Description = model.Description;
                service.CanInputQuantity = model.CanInputQuantity;
                service.Type = model.Type;
                service.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(service);
                _dbContext.SaveChanges();

                result.Data = service.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel UpdateServiceStatus(Guid id, bool isDisable)
        {
            var result = new ResultModel();
            try
            {
                var service = _dbContext.Services.FirstOrDefault(s => s.Id == id);

                if (service == null)
                {
                    throw new Exception("Invalid Id");
                }

                service.IsDisable = isDisable;
                service.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(service);
                _dbContext.SaveChanges();

                result.Data = service.Id;
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
