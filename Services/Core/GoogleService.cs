using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using System;
using System.Linq;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Directions.Request;
using GoogleApi.Entities.Maps.Directions.Response;
using MoreLinq;

namespace Services.Core
{
    public interface IGoogleService
    {
        ResultModel GetDistanceInMeters(SuitableEmployeeModel employee, Booking model, DirectionsRequest request);
        ResultModel GetDistanceAndDurationByAddress(string add1, string add2, DirectionsRequest request);
    }

    public class GoogleService : IGoogleService
    {
        private readonly AppDbContext _dbContext;

        public GoogleService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Lấy tên của Tp, Quận, Phường bằng Id
        public string GetLocationNameById(string locationId)
        {
            return _dbContext.Locations.FirstOrDefault(l => l.Id == locationId).Description;
        }

        public ResultModel GetDistanceInMeters(SuitableEmployeeModel employee, Booking model, DirectionsRequest request)
        {
            var result = new ResultModel();
            try
            {
                string addOriginString = employee.Address + ", " +
                                                  GetLocationNameById(employee.WardId) + ", " +
                                                  GetLocationNameById(employee.DistrictId) + ", " +
                                                  GetLocationNameById(employee.ProvinceId);
                Address addOrigin = new Address(addOriginString);
                request.Origin = new LocationEx(addOrigin);

                string addDestinationString = model.Address + ", " +
                                                    GetLocationNameById(model.WardId) + ", " +
                                                    GetLocationNameById(model.DistrictId) + ", " +
                                                    GetLocationNameById(model.ProvinceId);
                Address addDestination = new Address(addDestinationString);
                request.Destination = new LocationEx(addDestination);

                var response = GoogleApi.GoogleMaps.Directions.Query(request);

                result.Data = response.Routes.First().Legs.First().Distance.Value; // meters
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetDistanceAndDurationByAddress(string add1, string add2, DirectionsRequest request)
        {
            var result = new ResultModel();
            try
            {
                Address addOrigin = new Address(add1);
                request.Origin = new LocationEx(addOrigin);

                Address addDestination = new Address(add2);
                request.Destination = new LocationEx(addDestination);

                var response = GoogleApi.GoogleMaps.Directions.Query(request);

                double distanceInKm = (double)response.Routes.First().Legs.First().Distance.Value / 1000;
                int duration = response.Routes.First().Legs.First().Duration.Value;

                result.Data = new GoogleDistanceAndDuration()
                {
                    Distance = Math.Round(distanceInKm, 2),
                    Duration = duration
                };
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