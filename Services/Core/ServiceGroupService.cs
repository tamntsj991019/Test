using AutoMapper;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Data.ConstData;
using Newtonsoft.Json;
using Services.Util;

namespace Services.Core
{
    public interface IServiceGroupService
    {
        ResultModel Get(Guid? id, int pageIndex, int pageSize); 
        ResultModel SearchByName(string name, int pageIndex, int pageSize);
        ResultModel GetEnable(Guid? id);
        ResultModel GetNormalWithServiceEnable(Guid? id);
        Task<ResultModel> Create(ServiceGroupCreateModel model);
        ResultModel Update(Guid id, ServiceGroupUpdateModel model);
        ResultModel UpdateServiceGroupStatus(Guid id, bool isDisable);

        FileModel GetImage(Guid serviceGroupId);
        Task<ResultModel> AddImage(IFormFile file, Guid serviceGroupId);
    }
    public class ServiceGroupService : IServiceGroupService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly int MaxFileSize;
        private readonly IConfiguration _configuration;

        private readonly Utils _utils = new Utils();

        public ServiceGroupService(IMapper mapper, AppDbContext dbContext, IConfiguration configuration)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _configuration = configuration;
            MaxFileSize = int.Parse(_configuration["MaxFileSizeAllowed"]);
        }

        public ResultModel Get(Guid? id, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var serviceGroups = _dbContext.ServiceGroups.Where(s => id == null || s.Id == id)
                                                            .OrderBy(s => s.DateCreated).ToList();

                result.Data = new PagingModel
                {
                    Total = _mapper.Map<List<ServiceGroup>, List<ServiceGroupViewModel>>(serviceGroups).Count,
                    Data = _mapper.Map<List<ServiceGroup>, List<ServiceGroupViewModel>>(serviceGroups).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel SearchByName(string name, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var serviceGroups = _dbContext.ServiceGroups.OrderBy(s => s.DateCreated).ToList();

                List<ServiceGroupViewModel> listSG = _mapper.Map<List<ServiceGroup>, List<ServiceGroupViewModel>>(serviceGroups);

                string nameInput = _utils.RepalceSignForVietnameseString(name).ToUpper();
                listSG = listSG.Where(s => _utils.RepalceSignForVietnameseString(s.Description).ToUpper().Contains(nameInput)).ToList();
                
                result.Data = new PagingModel
                {
                    Total = listSG.Count,
                    Data = listSG.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetEnable(Guid? id)
        {
            var result = new ResultModel();
            try
            {
                var serviceGroups = _dbContext.ServiceGroups.Where(s => id == null || (s.IsDisable == false && s.Id == id)).ToList();

                List<ServiceGroupViewModel> listServiceGroup = _mapper.Map<List<ServiceGroup>, List<ServiceGroupViewModel>>(serviceGroups);
                if (serviceGroups != null && serviceGroups.Count() > 0)
                {
                    for (int i = 0; i < serviceGroups.Count(); i++)
                    {
                        if (serviceGroups[i].Image != null)
                        {
                            listServiceGroup[i].HasImage = serviceGroups[i].Id;
                        }
                    }
                }

                result.Data = listServiceGroup;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetNormalWithServiceEnable(Guid? id)
        {
            var result = new ResultModel();
            try
            {
                var serviceGroups = _dbContext.ServiceGroups.Where(s => (id == null && (s.Type == ConstServiceGroupType.NORMAL || s.Type == ConstServiceGroupType.OVERALL))
                                                                     || (s.IsDisable == false && (s.Type == ConstServiceGroupType.NORMAL || s.Type == ConstServiceGroupType.OVERALL) && s.Id == id)).ToList();

                List<ServiceGroupWithServiceViewModel> listSG = _mapper.Map<List<ServiceGroup>, List<ServiceGroupWithServiceViewModel>>(serviceGroups);
                if (serviceGroups != null && serviceGroups.Count() > 0)
                {
                    foreach (var sg in listSG)
                    {
                        var services = _dbContext.Services.Where(s => s.ServiceGroupId == sg.Id && s.IsDisable == false).ToList();
                        if (serviceGroups != null && serviceGroups.Count() != 0)
                        {
                            sg.ListService = _mapper.Map<List<Service>, List<ServiceViewModel>>(services);
                        }
                    }
                }

                var bookingTimeFrameJson = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.BOOKING_TIME_FRAME).Data;

                var settingUseCompanyTool = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.USE_COMPANY_TOOL);
                UsingCompanyToolViewModel uctModel = _mapper.Map<Setting, UsingCompanyToolViewModel>(settingUseCompanyTool);
                uctModel.Price = double.Parse(settingUseCompanyTool.Data);

                var cleaningAllJson = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.CLEAN_ALL).Data;

                SettingSetupModel ssModel = new SettingSetupModel()
                {
                    UsingCompanyTool = uctModel,
                    BookingTimeFrame = JsonConvert.DeserializeObject<BookingTimeFrameModel>(bookingTimeFrameJson),
                    CleaningAlls = JsonConvert.DeserializeObject<List<CleaningAllModel>>(cleaningAllJson)
                };

                BookingSetupModel bsModel = new BookingSetupModel()
                {
                    SettingService = ssModel,
                    ListServiceGroup = listSG
                };
                result.Data = bsModel;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> Create(ServiceGroupCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var serviceGroup = _mapper.Map<ServiceGroupCreateModel, ServiceGroup>(model);

                _dbContext.Add(serviceGroup);
                _dbContext.SaveChanges();

                result.Data = serviceGroup.Id;
                result.Succeed = true;

                if (model.ImageFile != null)
                {
                    await AddImage(model.ImageFile, serviceGroup.Id);
                }
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Update(Guid id, ServiceGroupUpdateModel model)
        {
            var result = new ResultModel();
            try
            {
                var serviceGroup = _dbContext.ServiceGroups.FirstOrDefault(s => s.Id == id);

                if (serviceGroup == null)
                {
                    throw new Exception("Invalid Id");
                }

                serviceGroup.Description = model.Description;
                serviceGroup.Type = model.Type;
                serviceGroup.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(serviceGroup);
                _dbContext.SaveChanges();

                result.Data = serviceGroup.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel UpdateServiceGroupStatus(Guid id, bool isDisable)
        {
            var result = new ResultModel();
            try
            {
                var serviceGroup = _dbContext.ServiceGroups.FirstOrDefault(s => s.Id == id);

                if (serviceGroup == null)
                {
                    throw new Exception("Invalid Id");
                }

                serviceGroup.IsDisable = isDisable;
                serviceGroup.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(serviceGroup);
                _dbContext.SaveChanges();

                result.Data = serviceGroup.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        #region Image Process
        public FileModel GetImage(Guid serviceGroupId)
        {
            var result = new FileModel();
            var serviceGroup = _dbContext.ServiceGroups.FirstOrDefault(e => e.Id == serviceGroupId);

            if (serviceGroup == null)
            {
                throw new Exception("Invalid Id");
            }

            result.Data = serviceGroup.Image;
            result.FileType = "image/jpeg";

            return result;
        }

        public async Task<ResultModel> AddImage(IFormFile file, Guid serviceGroupId)
        {
            var result = new ResultModel();
            try
            {
                await HandleFile(file, serviceGroupId);

                await _dbContext.SaveChangesAsync();

                result.Data = serviceGroupId;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        private async Task HandleFile(IFormFile file, Guid serviceGroupId)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            // Upload the file if less than x MB
            if (memoryStream.Length < MaxFileSize)
            {
                //string fileType = "image/jpeg";
                //if (file.ContentType != null && !string.IsNullOrEmpty(file.ContentType))
                //{
                //    fileType = file.ContentType;
                //}
                var serviceGroup = _dbContext.ServiceGroups.FirstOrDefault(sg => sg.Id == serviceGroupId);
                serviceGroup.Image = memoryStream.ToArray();
                _dbContext.ServiceGroups.Update(serviceGroup);
            }
            else
            {
                throw new Exception("File size too large. File name: " + file.FileName);
            }
        }
        #endregion

    }
}
