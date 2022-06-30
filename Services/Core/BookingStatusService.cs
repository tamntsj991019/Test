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
    public interface IBookingStatusService
    {
        ResultModel Get(string id);
        ResultModel Create(BookingStatusCreateModel model);
        ResultModel Update(string id, BookingStatusUpdateModel model);
        ResultModel Delete(string id);
    }
    public class BookingStatusService : IBookingStatusService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;

        public BookingStatusService(IMapper mapper, AppDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public ResultModel Get(string id)
        {
            var result = new ResultModel();
            try
            {
                var bookingStatuses = _dbContext.BookingStatuses.Where(s => id == null || (s.IsDisable == false && s.Id == id)).ToList();

                result.Data = _mapper.Map<List<BookingStatus>, List<BookingStatusViewModel>>(bookingStatuses);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Create(BookingStatusCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var bookingStatus = _mapper.Map<BookingStatusCreateModel, BookingStatus>(model);

                _dbContext.Add(bookingStatus);
                _dbContext.SaveChanges();

                result.Data = bookingStatus.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Update(string id, BookingStatusUpdateModel model)
        {
            var result = new ResultModel();
            try
            {
                var bookingStatus = _dbContext.BookingStatuses.FirstOrDefault(s => s.Id == id);

                if (bookingStatus == null)
                {
                    throw new Exception("Invalid Id");
                }

                bookingStatus = _mapper.Map<BookingStatusUpdateModel, BookingStatus>(model);
                bookingStatus.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(bookingStatus);
                _dbContext.SaveChanges();

                result.Data = bookingStatus.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel Delete(string id)
        {
            var result = new ResultModel();
            try
            {
                var bookingStatus = _dbContext.BookingStatuses.FirstOrDefault(s => s.Id == id);

                if (bookingStatus == null)
                {
                    throw new Exception("Invalid Id");
                }

                bookingStatus.IsDisable = true;
                bookingStatus.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(bookingStatus);
                _dbContext.SaveChanges();

                result.Data = bookingStatus.Id;
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
