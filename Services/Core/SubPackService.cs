using AutoMapper;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Services.Core
{
    public interface ISubPackService
    {
        ResultModel GetWardAndDistric(string parentId);
        ResultModel GetProvince();

    }
    public class SubPackService : ISubPackService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public SubPackService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public ResultModel GetProvince()
        {
            var result = new ResultModel();
            try
            {
                var provinces = _dbContext.Locations.Where(_l => _l.ParentId == null).OrderBy(_l => _l.Description).ToList();

                try
                {
                    var hn = provinces.FirstOrDefault(_p => _p.Description.Equals("Thành phố Hà Nội"));
                    var hcm = provinces.FirstOrDefault(_p => _p.Description.Equals("Thành phố Hồ Chí Minh"));

                    provinces.Remove(hn);
                    provinces.Remove(hcm);

                    provinces.Insert(0, hcm);
                    provinces.Insert(0, hn);
                }
                catch (Exception) { }

                result.Data = _mapper.Map<List<Location>, List<BaseStringModel>>(provinces);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }
        public ResultModel GetWardAndDistric(string parentId)
        {
            var result = new ResultModel();
            try
            {
                var provinces = _dbContext.Locations.Where(_l => _l.ParentId == parentId).OrderBy(_l => _l.Description).ToList();

                result.Data = _mapper.Map<List<Location>, List<BaseStringModel>>(provinces);
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
