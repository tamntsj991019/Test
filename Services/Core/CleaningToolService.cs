using AutoMapper;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Services.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Core
{
    public interface ICleaningToolService
    {
        ResultModel Get(Guid? id, int pageIndex, int pageSize);
        ResultModel SearchByName(string name, int pageIndex, int pageSize);
        ResultModel GetEnable(Guid? id);
        Task<ResultModel> Create(CleaningToolCreateModel model);
        ResultModel Update(Guid id, CleaningToolUpdateModel model);
        ResultModel AddMore(Guid id, int quantity);
        ResultModel UpdateCleaningToolStatus(Guid id, bool isDisable);

        FileModel GetImage(Guid cleaningToolId);
        Task<ResultModel> AddImage(IFormFile file, Guid cleaningToolId);
    }
    public class CleaningToolService : ICleaningToolService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly int MaxFileSize;
        private readonly IConfiguration _configuration;

        private readonly Utils _utils = new Utils();

        public CleaningToolService(IMapper mapper, AppDbContext dbContext, IConfiguration configuration)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _configuration = configuration;
            MaxFileSize = int.Parse(_configuration["MaxFileSizeAllowed"]); ;
        }

        public ResultModel Get(Guid? id, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var cleaningTools = _dbContext.CleaningTools.Where(cl => id == null || cl.Id == id).ToList();

                List<CleaningToolViewModel> listCleaningTool = _mapper.Map<List<CleaningTool>, List<CleaningToolViewModel>>(cleaningTools);

                result.Data = new PagingModel
                {
                    Total = listCleaningTool.Count,
                    Data = listCleaningTool.Skip((pageIndex - 1) * pageSize).Take(pageSize).OrderBy(_d => _d.DateCreated).ToList()
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
                var cleaningTools = _dbContext.CleaningTools.OrderBy(cl => cl.DateCreated).ToList();

                List<CleaningToolViewModel> listCleaningTool = _mapper.Map<List<CleaningTool>, List<CleaningToolViewModel>>(cleaningTools);

                string nameInput = _utils.RepalceSignForVietnameseString(name).ToUpper();
                listCleaningTool = listCleaningTool.Where(cl => _utils.RepalceSignForVietnameseString(cl.Description)
                                                                               .ToUpper().Contains(nameInput)).ToList();

                result.Data = new PagingModel
                {
                    Total = listCleaningTool.Count,
                    Data = listCleaningTool.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
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
                var cleaningTools = _dbContext.CleaningTools.Where(cl => id == null || (cl.IsDisable == false && cl.Id == id)).ToList();

                List<CleaningToolRequestViewModel> listCleaningTool = _mapper.Map<List<CleaningTool>, List<CleaningToolRequestViewModel>>(cleaningTools);
                for (int i = 0; i < cleaningTools.Count; i++)
                {
                    if (cleaningTools[i].Image != null)
                    {
                        listCleaningTool[i].HasImage = listCleaningTool[i].Id;
                    }
                }

                result.Data = listCleaningTool;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> Create(CleaningToolCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var cleaningTool = _mapper.Map<CleaningToolCreateModel, CleaningTool>(model);

                _dbContext.Add(cleaningTool);
                _dbContext.SaveChanges();

                result.Data = cleaningTool.Id;
                result.Succeed = true;

                if (model.ImageFile != null)
                {
                    await AddImage(model.ImageFile, cleaningTool.Id);
                }
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Update(Guid id, CleaningToolUpdateModel model)
        {
            var result = new ResultModel();
            try
            {
                var cleaningTool = _dbContext.CleaningTools.FirstOrDefault(s => s.Id == id);

                if (cleaningTool == null)
                {
                    throw new Exception("Invalid Id");
                }

                cleaningTool.Description = model.Description;
                cleaningTool.Quantity = model.Quantity;
                cleaningTool.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(cleaningTool);
                _dbContext.SaveChanges();

                result.Data = cleaningTool.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel AddMore(Guid id, int quantity)
        {
            var result = new ResultModel();
            try
            {
                var cleaningTool = _dbContext.CleaningTools.FirstOrDefault(s => s.Id == id);

                if (cleaningTool == null)
                {
                    throw new Exception("Invalid Id");
                }

                cleaningTool.Quantity += quantity;
                cleaningTool.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(cleaningTool);
                _dbContext.SaveChanges();

                result.Data = cleaningTool.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel UpdateCleaningToolStatus(Guid id, bool isDisable)
        {
            var result = new ResultModel();
            try
            {
                var cleaningTool = _dbContext.CleaningTools.FirstOrDefault(s => s.Id == id);

                if (cleaningTool == null)
                {
                    throw new Exception("Invalid Id");
                }

                cleaningTool.IsDisable = isDisable;
                cleaningTool.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(cleaningTool);
                _dbContext.SaveChanges();

                result.Data = cleaningTool.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        #region Image Process
        public FileModel GetImage(Guid cleaningToolId)
        {
            var result = new FileModel();
            var cleaningTool = _dbContext.CleaningTools.FirstOrDefault(e => e.Id == cleaningToolId);

            if (cleaningTool == null)
            {
                throw new Exception("Invalid Id");
            }

            //result.Id = user.Id;
            result.Data = cleaningTool.Image;
            result.FileType = "image/jpeg";

            return result;
        }

        public async Task<ResultModel> AddImage(IFormFile file, Guid cleaningToolId)
        {
            var result = new ResultModel();
            try
            {
                await HandleFile(file, cleaningToolId);

                await _dbContext.SaveChangesAsync();

                result.Data = cleaningToolId;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        private async Task HandleFile(IFormFile file, Guid cleaningToolId)
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
                var cleaningTool = _dbContext.CleaningTools.FirstOrDefault(cl => cl.Id == cleaningToolId);
                cleaningTool.Image = memoryStream.ToArray();
                _dbContext.CleaningTools.Update(cleaningTool);
            }
            else
            {
                throw new Exception("File size too large. File name: " + file.FileName);
            }
        }
        #endregion

        //public ResultModel AllocateCleaningTool(Guid cleaningToolId, string userId)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        var userCleaningTool = _dbContext.UserCleaningTools.Any(uct => uct.CleaningToolId == cleaningToolId && uct.EmployeeId == userId);


        //    }
        //    catch (Exception e)
        //    {
        //        result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        //    }
        //    return result;
        //}


    }
}
