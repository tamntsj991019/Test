using AutoMapper;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Services.Core
{
    public interface IBookingImageService
    {
        ResultModel Get(Guid? id);
        ResultModel Create(BookingImageCreateModel model);
        ResultModel Update(Guid id, BookingImageUpdateModel model);
        ResultModel Delete(Guid id);
    }
    public class BookingImageService : IBookingImageService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;

        public BookingImageService(IMapper mapper, AppDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public ResultModel Get(Guid? id)
        {
            var result = new ResultModel();
            try
            {
                var bookingImages = _dbContext.BookingImages.Where(s => id == null || (s.IsDisable == false && s.Id == id)).ToList();

                result.Data = _mapper.Map<List<BookingImage>, List<BookingImageViewModel>>(bookingImages);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Create(BookingImageCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var bookingImage = _mapper.Map<BookingImageCreateModel, BookingImage>(model);

                _dbContext.Add(bookingImage);
                _dbContext.SaveChanges();

                result.Data = bookingImage.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Update(Guid id, BookingImageUpdateModel model)
        {
            var result = new ResultModel();
            try
            {
                var bookingImage = _dbContext.BookingImages.FirstOrDefault(s => s.Id == id);

                if (bookingImage == null)
                {
                    throw new Exception("Invalid Id");
                }

                bookingImage = _mapper.Map<BookingImageUpdateModel, BookingImage>(model);
                bookingImage.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(bookingImage);
                _dbContext.SaveChanges();

                result.Data = bookingImage.Id;
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
                var bookingImage = _dbContext.BookingImages.FirstOrDefault(s => s.Id == id);

                if (bookingImage == null)
                {
                    throw new Exception("Invalid Id");
                }

                bookingImage.IsDisable = true;
                bookingImage.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(bookingImage);
                _dbContext.SaveChanges();

                result.Data = bookingImage.Id;
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
