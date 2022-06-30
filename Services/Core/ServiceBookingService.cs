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
    public interface IServiceBookingService
    {
        ResultModel Get(Guid? bookingId, Guid? serviceId);
        //ResultModel Create(ServiceBookingCreateModel model);
        //ResultModel Update(Guid bookingId, Guid serviceId, ServiceBookingUpdateModel model);
        //ResultModel Delete(Guid bookingId, Guid serviceId);
    }
    public class ServiceBookingService : IServiceBookingService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;

        public ServiceBookingService(IMapper mapper, AppDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public ResultModel Get(Guid? bookingId, Guid? serviceId)
        {
            var result = new ResultModel();
            try
            {
                var ServiceBookings = _dbContext.ServiceBookings.Where(s => (bookingId == null && serviceId==null) || 
                                                                            (s.IsDisable == false && s.BookingId == bookingId && s.ServiceId == serviceId)).ToList();

                result.Data = _mapper.Map<List<ServiceBooking>, List<ServiceBookingViewModel>>(ServiceBookings);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        

        //public ResultModel Create(ServiceBookingCreateModel model)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        var ServiceBooking = _mapper.Map<ServiceBookingCreateModel, ServiceBooking>(model);

        //        _dbContext.Add(ServiceBooking);
        //        _dbContext.SaveChanges();

        //        result.Data = (ServiceBooking.BookingId.ToString()) + " + " + (ServiceBooking.ServiceId.ToString());
        //        result.Succeed = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        //    }
        //    return result;
        //}

        //public ResultModel Update(Guid bookingId, Guid serviceId, ServiceBookingUpdateModel model)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        var serviceBooking = _dbContext.ServiceBookings.FirstOrDefault(s => s.BookingId == bookingId && s.ServiceId == serviceId);

        //        if (serviceBooking == null)
        //        {
        //            throw new Exception("Invalid Id");
        //        }

        //        //serviceBooking = _mapper.Map<ServiceBookingUpdateModel, ServiceBooking>(model);
        //        serviceBooking.Quantity = model.Quantity;
        //        serviceBooking.DateUpdated = DateTime.UtcNow.AddHours(7);

        //        _dbContext.Update(serviceBooking);
        //        _dbContext.SaveChanges();

        //        result.Data = (serviceBooking.BookingId.ToString()) + " + " + (serviceBooking.ServiceId.ToString());
        //        result.Succeed = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        //    }
        //    return result;
        //}

        //public ResultModel Delete(Guid bookingId, Guid serviceId)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        var ServiceBooking = _dbContext.ServiceBookings.FirstOrDefault(s => s.BookingId == bookingId && s.ServiceId == serviceId);

        //        if (ServiceBooking == null)
        //        {
        //            throw new Exception("Invalid Id");
        //        }

        //        ServiceBooking.IsDisable = true;
        //        ServiceBooking.DateUpdated = DateTime.UtcNow.AddHours(7);

        //        _dbContext.Update(ServiceBooking);
        //        _dbContext.SaveChanges();

        //        result.Data = (ServiceBooking.BookingId.ToString()) + " + " + (ServiceBooking.ServiceId.ToString());
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
