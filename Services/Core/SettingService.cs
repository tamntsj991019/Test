using AutoMapper;
using Data.ConstData;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Services.Core
{
    public interface ISettingService
    {
        ResultModel Get(Guid? id);
        ResultModel GetByKey(string key);
        ResultModel GetCleaningAll();
        ResultModel GetTrafficJamTime();
        ResultModel Create(SettingCreateModel model);
        ResultModel Update(Guid id, SettingUpdateModel model);
        ResultModel Delete(Guid id);
    }
    public class SettingService : ISettingService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;

        public SettingService(IMapper mapper, AppDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public ResultModel Get(Guid? id)
        {
            var result = new ResultModel();
            try
            {
                var settings = _dbContext.Settings.Where(s => id == null || s.Id == id).ToList();

                result.Data = _mapper.Map<List<Setting>, List<SettingViewModel>>(settings);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetByKey(string key)
        {
            var result = new ResultModel();
            try
            {
                var settings = _dbContext.Settings.FirstOrDefault(s => s.Key == key);

                result.Data = _mapper.Map<Setting, SettingUserViewModel>(settings);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetCleaningAll()
        {
            var result = new ResultModel();
            try
            {
                var settingJson = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.CLEAN_ALL).Data;
                

                result.Data = JsonConvert.DeserializeObject<List<CleaningAllModel>>(settingJson); 
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }
        
        public ResultModel GetTrafficJamTime()
        {
            var result = new ResultModel();
            try
            {
                var settingJson = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.TRAFFIC_JAM_TIME).Data;
                

                result.Data = JsonConvert.DeserializeObject<List<TrafficJamTimeModel>>(settingJson); ;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Create(SettingCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var setting = _mapper.Map<SettingCreateModel, Setting>(model);

                _dbContext.Add(setting);
                _dbContext.SaveChanges();

                result.Data = setting.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Update(Guid id, SettingUpdateModel model)
        {
            var result = new ResultModel();
            try
            {
                var setting = _dbContext.Settings.FirstOrDefault(s => s.Id == id);

                if (setting == null)
                {
                    throw new Exception("Invalid Id");
                }

                setting.Description = model.Description;
                setting.Data = model.Data;
                setting.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(setting);
                _dbContext.SaveChanges();

                result.Data = setting.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        //public ResultModel UpdateJson(Guid id, SettingUpdateJsonModel model)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        var setting = _dbContext.Settings.FirstOrDefault(s => s.Id == id);

        //        if (setting == null)
        //        {
        //            throw new Exception("Invalid Id");
        //        }

        //        setting.Description = JsonConvert.SerializeObject(model.ListTrafficJamTime);
        //        setting.Key = model.Key;
        //        setting.DateUpdated = DateTime.UtcNow.AddHours(7);

        //        _dbContext.Update(setting);
        //        _dbContext.SaveChanges();

        //        result.Data = setting.Id;
        //        result.Success = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        //    }
        //    return result;
        //}



        public ResultModel Delete(Guid id)
        {
            var result = new ResultModel();
            try
            {
                var setting = _dbContext.Settings.FirstOrDefault(s => s.Id == id);

                if (setting == null)
                {
                    throw new Exception("Invalid Id");
                }

                setting.IsDisable = true;
                setting.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(setting);
                _dbContext.SaveChanges();

                result.Data = setting.Id;
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
