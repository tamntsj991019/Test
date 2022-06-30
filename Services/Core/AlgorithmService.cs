using AutoMapper;
using Data.ConstData;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Directions.Request;
using GoogleApi.Entities.Maps.Directions.Response;
using Services.Util;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Http;
using Hangfire;
using Hubs;
using MoreLinq;
using Microsoft.EntityFrameworkCore;

namespace Services.Core
{
    public interface IAlgorithmService
    {
        int GetEstimatedMovingTime(DateTime dateBegin);
        Task<string> GetSuitableEmployee(Booking model);
        BookingCheckEmployeeModel CheckEmployeeFree(string empId, DatetimeNewBookingModel datetimeModel, Booking newBooking);
        Task<List<EmployeeWithBookingLogs>> GetEmployeeSuitable(Booking model, double radius);
    }

    public class AlgorithmService : IAlgorithmService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IGoogleService _googleService;
        private readonly IConfiguration _configuration;
        private readonly string _googleApiKey;

        private readonly Utils _utils = new Utils();

        public AlgorithmService(IMapper mapper, AppDbContext dbContext, UserManager<User> userManager, IGoogleService googleService,
                                IConfiguration configuration)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _userManager = userManager;
            _googleService = googleService;
            _configuration = configuration;
            _googleApiKey = _configuration["GoogleApiKey"];
        }

        #region Finding Utils
        // Lấy tên của Tp, Quận, Phường bằng Id
        public string GetLocationNameById(string locationId)
        {
            return _dbContext.Locations.FirstOrDefault(l => l.Id == locationId).Description;
        }

        public string GetFullAddress(BaseAddressModel model)
        {
            return model.Address + ", " + GetLocationNameById(model.WardId) + ", " +
                                          GetLocationNameById(model.DistrictId) + ", " +
                                          GetLocationNameById(model.ProvinceId);
        }

        // Ước tính thời gian di chuyển
        public int GetEstimatedMovingTime(DateTime dateBegin)
        {
            var settingJson = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.TRAFFIC_JAM_TIME).Data;
            List<TrafficJamTimeModel> listTrafficJamTime = JsonConvert.DeserializeObject<List<TrafficJamTimeModel>>(settingJson);

            bool isTrafficJamTime = false;
            TimeSpan bookingTime = dateBegin.TimeOfDay;
            foreach (var time in listTrafficJamTime)
            {
                TimeSpan timeFrom = _utils.ConverStringToTimeSpan(time.TimeFrom);
                TimeSpan timeTo = _utils.ConverStringToTimeSpan(time.TimeTo);
                if (TimeSpan.Compare(bookingTime, timeFrom) >= 0 && TimeSpan.Compare(bookingTime, timeTo) <= 0)
                {
                    isTrafficJamTime = true;
                    break;
                }
            }

            string estimatedMovingTime = "";
            if (isTrafficJamTime)
            {
                estimatedMovingTime = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.ESTIMATE_TRAFFIC_JAM_TIME).Data;
            }
            else
            {
                estimatedMovingTime = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.ESTIMATE_NORMAL_TIME).Data;
            }

            return int.Parse(estimatedMovingTime);
        }

        public TimeSpan GetEstimateTimeEnd(Booking booking)
        {
            return booking.DateBegin.Value.TimeOfDay
                          .Add(new TimeSpan(0, booking.EstimatedTime + GetEstimatedMovingTime(booking.DateBegin.Value), 0));
        }

        // kiem tra nhaan vien tu choi booking
        public bool IsEmployeeRejected(string cusId, string empId)
        {
            // Loại nếu như có reject booking của khách hàng đó rồi
            var setting = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.BAN_REJECT);
            int banHours = int.Parse(setting.Data);


            var bookingRejected = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                                .Where(bl => bl.Booking.CustomerId == cusId)
                                                                .Where(bl => bl.Booking.EmployeeId == empId)
                                                                .Where(bl => bl.BookingStatusId == ConstBookingStatus.REJECTED)
                                                                .OrderByDescending(bl => bl.Booking.DateCreated)
                                                                .ToList().FirstOrDefault();

            if (bookingRejected == null)
            {
                return false;
            }
            else
            {
                TimeSpan banHoursTS = new TimeSpan(banHours, 0, 0);
                DateTime today = DateTime.UtcNow.AddHours(7);
                bool isEnough24h = (today.Date - banHoursTS) > bookingRejected.DateCreated.Value.Date;
                if (isEnough24h)
                {
                    return false;
                }
            }
            return true;
        }

        // kiem tra khoang cach giua 2 Booking
        public GoogleDistanceAndDuration2Ways GetDistanceAndDurationBetween2Bookings(Booking bookingA, Booking bookingB)
        {
            //return _utils.CheckPointsNearByHaversine(bookingA.Latitude, bookingA.Longitude, bookingB.Latitude, bookingB.Longitude, radius);

            string add1 = GetFullAddress(_mapper.Map<Booking, BaseAddressModel>(bookingA));
            string add2 = GetFullAddress(_mapper.Map<Booking, BaseAddressModel>(bookingB));

            DirectionsRequest request = new DirectionsRequest
            {
                Key = _googleApiKey
            };

            return new GoogleDistanceAndDuration2Ways()
            {
                FromAToB = (GoogleDistanceAndDuration)_googleService.GetDistanceAndDurationByAddress(add1, add2, request).Data,
                FromBToA = (GoogleDistanceAndDuration)_googleService.GetDistanceAndDurationByAddress(add2, add1, request).Data
            };
        }

        // kiem tra khoang cach giua Booking vaf nhan vien
        public GoogleDistanceAndDuration GetDistanceAndDurationBetweenBookingAndEmp(User emp, Booking booking)
        {
            string addEmp = GetFullAddress(_mapper.Map<User, BaseAddressModel>(emp));
            string addBooking = GetFullAddress(_mapper.Map<Booking, BaseAddressModel>(booking));

            DirectionsRequest request = new DirectionsRequest
            {
                Key = _googleApiKey
            };

            return (GoogleDistanceAndDuration)_googleService.GetDistanceAndDurationByAddress(addEmp, addBooking, request).Data;
        }

        // lấy các khung giờ làm việc của emp trong 1 ngày
        public List<IntervalTimeOnlyModel> GetEmployeesIntervals(string empId, DateTime dateBegin)
        {
            var result = new List<IntervalTimeOnlyModel>();
            try
            {
                var intervals = _dbContext.Intervals.Include(i => i.Schedule).ThenInclude(s => s.Employee)
                                                    .Where(i => i.Schedule.EmployeeId == empId)
                                                    .Where(i => i.Schedule.DateWorking.Date == dateBegin.Date)
                                                    .Where(i => i.Schedule.IsDisable == false)
                                                    .Where(i => i.IsDisable == false).ToList();

                if (intervals != null && intervals.Count() > 0)
                {
                    result = _mapper.Map<List<Interval>, List<IntervalTimeOnlyModel>>(intervals);
                }
            }
            catch (Exception e)
            {
                //result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
                Console.WriteLine(e.InnerException != null ? e.InnerException.Message : e.Message);
            }
            return result;
        }

        public BookingCheckEmployeeModel CheckEmployeeFree(string empId, DatetimeNewBookingModel datetimeModel, Booking newBooking)
        {
            BookingCheckEmployeeModel result = new BookingCheckEmployeeModel();

            // tìm xem có emp nào có đã có booking vào ngày làm việc dateBegin không
            var bLogs = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                              .Where(bl => bl.Booking.EmployeeId == empId)
                                              .Where(bl => bl.BookingId != newBooking.Id)
                                              // ngày làm booking mới trùng với booking đã có trong cùng 1 ngày làm
                                              .Where(bl => bl.Booking.DateBegin.Value.Date == datetimeModel.NewBookingDate)
                                              .Where(bl => bl.Booking.IsDisable == false && bl.IsDisable == false)
                                              .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                              .Where(bl => bl.BookingStatusId == ConstBookingStatus.WAITING).ToList();

            result.BLogs = bLogs;

            // lay thong tin ngay lam viec cua emp
            var empSche = _dbContext.Schedules.Include(s => s.Employee)
                                              .Include(s => s.Intervals)
                                              .Where(s => s.EmployeeId == empId)
                                              .Where(s => s.DateWorking.Date == datetimeModel.NewBookingDate)
                                              .Where(s => s.IsDisable == false).FirstOrDefault();

            if (empSche == null)
            {
                return result;
            }

            empSche.Intervals = empSche.Intervals.Where(i => i.IsDisable == false)
                                                 .OrderBy(i => i.TimeFrom)
                                                 .ToList();

            if (bLogs.Count() == 0)
            {
                result.Check = true;
            }
            else if (bLogs.Count() == 1)
            {   // Nếu có 1 booking trong khoảng thời gian làm việc thì check khoảng thời gian trước và sau booking có phù hợp không
                var booking = bLogs.FirstOrDefault().Booking;

                TimeSpan bookingTimeBegin = booking.DateBegin.Value.TimeOfDay;
                TimeSpan bookingTimeEnd = GetEstimateTimeEnd(booking);

                foreach (var itv in empSche.Intervals)
                {
                    TimeSpan timeFrom = _utils.ConverStringToTimeSpan(itv.TimeFrom);
                    TimeSpan timeTo = _utils.ConverStringToTimeSpan(itv.TimeTo);
                    if ((TimeSpan.Compare(timeFrom, datetimeModel.NewBookingBegin) <= 0 && TimeSpan.Compare(datetimeModel.NewBookingEnd, bookingTimeBegin) <= 0) ||
                        (TimeSpan.Compare(bookingTimeEnd, datetimeModel.NewBookingBegin) <= 0 && TimeSpan.Compare(datetimeModel.NewBookingEnd, timeTo) <= 0))
                    {
                        result.Check = true;
                        break;
                    }
                }
            }
            else if (bLogs.Count() > 1)
            {
                // Lay ra khung gio cua emp phu hop voi booking moi 
                Interval suitableInterval = new Interval();
                foreach (var itv in empSche.Intervals)
                {
                    TimeSpan from = _utils.ConverStringToTimeSpan(itv.TimeFrom);
                    TimeSpan to = _utils.ConverStringToTimeSpan(itv.TimeTo);
                    if ((TimeSpan.Compare(from, datetimeModel.NewBookingBegin) <= 0 && TimeSpan.Compare(datetimeModel.NewBookingEnd, to) <= 0))
                    {
                        suitableInterval = itv;
                        break;
                    }
                }

                TimeSpan timeFrom = _utils.ConverStringToTimeSpan(suitableInterval.TimeFrom);
                TimeSpan timeTo = _utils.ConverStringToTimeSpan(suitableInterval.TimeTo);

                // lay ra cac booking da co trong khung gio o tren
                List<Booking> listBooking = new List<Booking>();
                foreach (var bLog in bLogs)
                {
                    var booking = bLog.Booking;
                    TimeSpan bookingBegin = booking.DateBegin.Value.TimeOfDay;
                    TimeSpan bookingEnd = GetEstimateTimeEnd(booking);

                    if ((TimeSpan.Compare(timeFrom, bookingBegin) <= 0 && TimeSpan.Compare(bookingEnd, timeTo) <= 0))
                    {
                        listBooking.Add(booking);
                    }
                }
                listBooking = listBooking.OrderBy(b => b.DateBegin).ToList();

                // check tung khoang trong trong khung gio giua cac booing
                // From - Booking 1 - Booking 2 - ... - To
                for (int i = 0; i < listBooking.Count(); i++)
                {
                    var booking = listBooking[i];
                    TimeSpan bookingBegin = booking.DateBegin.Value.TimeOfDay;
                    TimeSpan bookingEnd = GetEstimateTimeEnd(booking);

                    if (i == 0)
                    {
                        if ((TimeSpan.Compare(timeFrom, datetimeModel.NewBookingBegin) <= 0 && TimeSpan.Compare(datetimeModel.NewBookingEnd, bookingBegin) < 0))
                        {
                            result.Check = true;
                            break;
                        }
                    }
                    else
                    {
                        if (i == listBooking.Count() - 1)
                        {
                            if ((TimeSpan.Compare(bookingEnd, datetimeModel.NewBookingBegin) < 0 && TimeSpan.Compare(datetimeModel.NewBookingEnd, timeTo) <= 0))
                            {
                                result.Check = true;
                                break;
                            }
                        }
                        else
                        {
                            TimeSpan bookingPrevEnd = GetEstimateTimeEnd(listBooking[i - 1]);
                            if ((TimeSpan.Compare(bookingPrevEnd, datetimeModel.NewBookingBegin) < 0 && TimeSpan.Compare(datetimeModel.NewBookingEnd, bookingBegin) < 0))
                            {
                                result.Check = true;
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public BookingCheckEmployeeWithDistanceModel CheckEmployeeSuitable(string empId, DatetimeNewBookingModel datetimeModel, Booking newBooking, double radius)
        {
            BookingCheckEmployeeWithDistanceModel result = new BookingCheckEmployeeWithDistanceModel();

            var emp = _dbContext.Users.FirstOrDefault(u => u.Id == empId);
            if (emp == null)
            {
                return result;
            }

            GoogleDistanceAndDuration bookingAndEmp = GetDistanceAndDurationBetweenBookingAndEmp(emp, newBooking);
            if (bookingAndEmp.Distance > radius)
            {
                return result;
            }

            result.Distance = bookingAndEmp.Distance;

            // tìm xem có emp nào có đã có booking vào ngày làm việc dateBegin không
            var bLogs = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                              .Where(bl => bl.Booking.EmployeeId == empId)
                                              .Where(bl => bl.BookingId != newBooking.Id)
                                              // ngày làm booking mới trùng với booking đã có trong cùng 1 ngày làm
                                              .Where(bl => bl.Booking.DateBegin.Value.Date == datetimeModel.NewBookingDate)
                                              .Where(bl => bl.Booking.IsDisable == false && bl.IsDisable == false)
                                              .OrderByDescending(bl => bl.DateCreated).DistinctBy(bl => bl.BookingId)
                                              .Where(bl => bl.BookingStatusId == ConstBookingStatus.WAITING).ToList();

            result.BLogs = bLogs;

            // lay thong tin ngay lam viec cua emp
            var empSche = _dbContext.Schedules.Include(s => s.Employee)
                                              .Include(s => s.Intervals)
                                              .Where(s => s.EmployeeId == empId)
                                              .Where(s => s.DateWorking.Date == datetimeModel.NewBookingDate)
                                              .Where(s => s.IsDisable == false).FirstOrDefault();

            if (empSche == null)
            {
                return result;
            }

            empSche.Intervals = empSche.Intervals.Where(i => i.IsDisable == false)
                                                 .OrderBy(i => i.TimeFrom)
                                                 .ToList();

            if (bLogs.Count() == 0)
            {
                result.Check = true;
            }
            else if (bLogs.Count() == 1)
            {   // Nếu có 1 booking trong khoảng thời gian làm việc thì check khoảng thời gian trước và sau booking có phù hợp không
                var booking = bLogs.FirstOrDefault().Booking;

                // chi khi co khoang thoi gian phu hop thi moi kiem tra quang duong < X km
                GoogleDistanceAndDuration2Ways disDu2Bookings = GetDistanceAndDurationBetween2Bookings(booking, newBooking);

                TimeSpan bookingTimeBegin = booking.DateBegin.Value.TimeOfDay;
                TimeSpan bookingTimeEnd = GetEstimateTimeEnd(booking);

                foreach (var itv in empSche.Intervals)
                {
                    TimeSpan timeFrom = _utils.ConverStringToTimeSpan(itv.TimeFrom);
                    TimeSpan timeTo = _utils.ConverStringToTimeSpan(itv.TimeTo);
                    if ((TimeSpan.Compare(timeFrom, datetimeModel.NewBookingBegin) <= 0 &&
                        TimeSpan.Compare(datetimeModel.NewBookingEnd.Add(new TimeSpan(0, 0, disDu2Bookings.FromBToA.Duration)), bookingTimeBegin) <= 0))
                    {
                        if (disDu2Bookings.FromBToA.Distance <= radius)
                        {
                            result.Check = true;
                            break;
                        }
                    }
                    else if (TimeSpan.Compare(bookingTimeEnd.Add(new TimeSpan(0, 0, disDu2Bookings.FromAToB.Duration)), datetimeModel.NewBookingBegin) <= 0 &&
                            TimeSpan.Compare(datetimeModel.NewBookingEnd, timeTo) <= 0)
                    {
                        if (disDu2Bookings.FromAToB.Distance <= radius)
                        {
                            result.Check = true;
                            break;
                        }
                    }
                }
            }
            else if (bLogs.Count() > 1)
            {
                // Lay ra khung gio cua emp phu hop voi booking moi 
                Interval suitableInterval = new Interval();
                foreach (var itv in empSche.Intervals)
                {
                    TimeSpan from = _utils.ConverStringToTimeSpan(itv.TimeFrom);
                    TimeSpan to = _utils.ConverStringToTimeSpan(itv.TimeTo);
                    if ((TimeSpan.Compare(from, datetimeModel.NewBookingBegin) <= 0 && TimeSpan.Compare(datetimeModel.NewBookingEnd, to) <= 0))
                    {
                        suitableInterval = itv;
                        break;
                    }
                }

                TimeSpan timeFrom = _utils.ConverStringToTimeSpan(suitableInterval.TimeFrom);
                TimeSpan timeTo = _utils.ConverStringToTimeSpan(suitableInterval.TimeTo);

                // lay ra cac booking da co trong khung gio o tren
                List<Booking> listBooking = new List<Booking>();
                foreach (var bLog in bLogs)
                {
                    var booking = bLog.Booking;
                    TimeSpan bookingBegin = booking.DateBegin.Value.TimeOfDay;
                    TimeSpan bookingEnd = GetEstimateTimeEnd(booking);

                    if ((TimeSpan.Compare(timeFrom, bookingBegin) <= 0 && TimeSpan.Compare(bookingEnd, timeTo) <= 0))
                    {
                        listBooking.Add(booking);
                    }
                }
                listBooking = listBooking.OrderBy(b => b.DateBegin).ToList();

                // check tung khoang trong trong khung gio giua cac booing
                // From - Booking 1 - Booking 2 - ... - To
                GoogleDistanceAndDuration2Ways disDu2BookingsPrev = new GoogleDistanceAndDuration2Ways();
                for (int i = 0; i < listBooking.Count(); i++)
                {
                    var booking = listBooking[i];
                    TimeSpan bookingBegin = booking.DateBegin.Value.TimeOfDay;
                    TimeSpan bookingEnd = GetEstimateTimeEnd(booking);

                    // chi khi co khoang thoi gian phu hop thi moi kiem tra quang duong < X km
                    GoogleDistanceAndDuration2Ways disDu2Bookings = GetDistanceAndDurationBetween2Bookings(newBooking, booking);
                    if (i == 0)
                    {
                        if ((TimeSpan.Compare(timeFrom, datetimeModel.NewBookingBegin) <= 0 &&
                             TimeSpan.Compare(datetimeModel.NewBookingEnd.Add(new TimeSpan(0, 0, disDu2Bookings.FromAToB.Duration)), bookingBegin) < 0))
                        {
                            // chi khi co khoang thoi gian phu hop thi moi kiem tra quang duong < X km
                            if (disDu2Bookings.FromAToB.Distance <= radius)
                            {
                                result.Check = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (i == listBooking.Count() - 1)
                        {
                            if ((TimeSpan.Compare(bookingEnd.Add(new TimeSpan(0, 0, disDu2Bookings.FromBToA.Duration)), datetimeModel.NewBookingBegin) < 0 &&
                                TimeSpan.Compare(datetimeModel.NewBookingEnd, timeTo) <= 0))
                            {
                                if (disDu2Bookings.FromBToA.Distance <= radius)
                                {
                                    result.Check = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            TimeSpan bookingPrevEnd = GetEstimateTimeEnd(listBooking[i - 1]).Add(new TimeSpan(0,0,disDu2BookingsPrev.FromBToA.Duration));
                            if ((TimeSpan.Compare(bookingPrevEnd, datetimeModel.NewBookingBegin) < 0 &&
                                TimeSpan.Compare(datetimeModel.NewBookingEnd.Add(new TimeSpan(0, 0, disDu2Bookings.FromAToB.Duration)), bookingBegin) < 0))
                            {
                                //GoogleDistanceAndDuration disDu2BookingsPrev = GetDistanceAndDurationBetween2Bookings(listBooking[i - 1], newBooking);
                                if (disDu2Bookings.FromAToB.Distance <= radius && disDu2BookingsPrev.FromBToA.Distance <= radius)
                                {
                                    result.Check = true;
                                    break;
                                }
                            }
                        }
                    }
                    disDu2BookingsPrev = disDu2Bookings;
                }
            }
            return result;
        }
        #endregion

        #region FIND NEW
        // Tìm các nhân viên có thời gian làm việc phù hợp với dateBegin của booking
        // theo danh sách nhân viên đã tìm ra ở GetSuitableEmployeeByDistance(Booking model)
        public async Task<List<User>> GetEmployeeHasSuitableWorkingTime(Booking model, DateTime dateBegin)
        {
            var emps = (await _userManager.GetUsersInRoleAsync(ConstRole.EMPLOYEE))
                                          .Where(e => e.IsDisable == false).ToList();

            if (emps == null || emps.Count() == 0)
            {
                return null;
            }

            int estimatedMovingTime = GetEstimatedMovingTime(dateBegin);
            // lọc emp có thời gian làm việc phù hợp
            List<User> listEmpResult = new List<User>();
            foreach (var emp in emps)
            {
                var intervals = _dbContext.Intervals.Include(i => i.Schedule).Where(i => i.Schedule.EmployeeId == emp.Id)
                                                                             .Where(i => i.Schedule.DateWorking.Date == dateBegin.Date)
                                                                             .Where(i => i.Schedule.IsDisable == false)
                                                                             .Where(i => i.IsDisable == false).ToList();

                var interval = intervals.Where(i => TimeSpan.Compare(_utils.ConverStringToTimeSpan(i.TimeFrom), dateBegin.TimeOfDay) <= 0)
                                        .Where(i => TimeSpan.Compare(_utils.ConverStringToTimeSpan(i.TimeTo), dateBegin.TimeOfDay.Add(new TimeSpan(0, model.EstimatedTime + estimatedMovingTime, 0))) >= 0)
                                        .FirstOrDefault();

                // Vi can Emp co thoi gian phu hop nen khi query co data thi moi add
                if (interval != null)
                {
                    listEmpResult.Add(emp);
                }
            }
            return listEmpResult;
        }

        public async Task<List<EmployeeWithBookingLogs>> GetEmployeeSuitable(Booking model, double radius)
        {
            List<EmployeeWithBookingLogs> result = new List<EmployeeWithBookingLogs>();

            List<User> listEmpWithSuitableWorkingTime = await GetEmployeeHasSuitableWorkingTime(model, model.DateBegin.Value);
            if (listEmpWithSuitableWorkingTime == null || listEmpWithSuitableWorkingTime.Count() == 0)
            {
                return result;
            }

            DatetimeNewBookingModel datetimeModel = new DatetimeNewBookingModel()
            {
                NewBookingDate = model.DateBegin.Value.Date,
                NewBookingBegin = model.DateBegin.Value.TimeOfDay,
                NewBookingEnd = GetEstimateTimeEnd(model)
            };

            foreach (var emp in listEmpWithSuitableWorkingTime)
            {
                BookingCheckEmployeeWithDistanceModel checkingEmpSuitable = CheckEmployeeSuitable(emp.Id, datetimeModel, model, radius);

                if (checkingEmpSuitable.Check == true)
                {
                    bool isReject = IsEmployeeRejected(model.CustomerId, emp.Id);
                    if (isReject == false)
                    {
                        result.Add(new EmployeeWithBookingLogs()
                        {
                            EmployeeId = emp.Id,
                            Distance = checkingEmpSuitable.Distance,
                            bLogs = checkingEmpSuitable.BLogs
                        });
                    }
                }
            }

            return result.OrderBy(e => e.bLogs.Count()).ToList();
        }
        #endregion

        #region Find OLD
        // Tìm các nhân viên trong khoảng cách quy định
        public async Task<List<SuitableEmployeeModel>> GetSuitableEmployeeByDistance(Booking model)
        {
            var setting = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.MAX_DISTANCE);
            double radius = double.Parse(setting.Data);

            var emps = (await _userManager.GetUsersInRoleAsync(ConstRole.EMPLOYEE))
                                          .Where(e => e.IsDisable == false)
                                          .Where(e => _utils.CheckPointsNearByHaversine(e.Latitude.Value, e.Longitude.Value, model.Latitude, model.Longitude, radius) == true)
                                          .ToList();

            if (emps == null || emps.Count() == 0)
            {
                return null;
            }

            ConcurrentQueue<SuitableEmployeeModel> listEmp = new ConcurrentQueue<SuitableEmployeeModel>();
            Parallel.ForEach(Partitioner.Create(emps), emp =>
            {
                SuitableEmployeeModel seModel = _mapper.Map<User, SuitableEmployeeModel>(emp);
                seModel.Distance = _utils.CalculatePointsNearByHaversine(seModel.Latitude, seModel.Longitude, model.Latitude, model.Longitude);
                listEmp.Enqueue(seModel);
            });

            return listEmp.OrderBy(l => l.Distance).ToList();
        }

        // Tìm các nhân viên có thời gian làm việc phù hợp với dateBegin của booking
        // theo danh sách nhân viên đã tìm ra ở GetSuitableEmployeeByDistance(Booking model)
        public async Task<List<SuitableEmployeeModel>> GetSuitableEmployeeByWorkingTime(Booking model, DateTime dateBegin)
        {
            List<SuitableEmployeeModel> listEmpWithSuitableDistance = await GetSuitableEmployeeByDistance(model);
            if (listEmpWithSuitableDistance == null || listEmpWithSuitableDistance.Count() == 0)
            {
                return null;
            }

            int estimatedMovingTime = GetEstimatedMovingTime(dateBegin);
            // lọc emp có thời gian làm việc phù hợp
            List<SuitableEmployeeModel> listEmpResult = new List<SuitableEmployeeModel>();
            foreach (var emp in listEmpWithSuitableDistance)
            {
                var intervals = _dbContext.Intervals.Include(i => i.Schedule).Where(i => i.Schedule.EmployeeId == emp.Id)
                                                                             .Where(i => i.Schedule.DateWorking.Date == dateBegin.Date)
                                                                             .Where(i => i.Schedule.IsDisable == false)
                                                                             .Where(i => i.IsDisable == false).ToList();

                var interval = intervals.Where(i => TimeSpan.Compare(_utils.ConverStringToTimeSpan(i.TimeFrom), dateBegin.TimeOfDay) <= 0)
                                        .Where(i => TimeSpan.Compare(_utils.ConverStringToTimeSpan(i.TimeTo), dateBegin.TimeOfDay.Add(new TimeSpan(0, model.EstimatedTime + estimatedMovingTime, 0))) >= 0)
                                        .FirstOrDefault();

                // Vi can Emp co thoi gian phu hop nen khi query co data thi moi add
                if (interval != null)
                {
                    listEmpResult.Add(emp);
                }
            }
            return listEmpResult;
        }

        public async Task<List<SuitableEmployeeModel>> GetSuitableEmployeeFree(Booking model)
        {
            List<SuitableEmployeeModel> listEmpWithSuitableWorkingTime = await GetSuitableEmployeeByWorkingTime(model, model.DateBegin.Value);
            if (listEmpWithSuitableWorkingTime == null || listEmpWithSuitableWorkingTime.Count() == 0)
            {
                return null;
            }

            DatetimeNewBookingModel datetimeModel = new DatetimeNewBookingModel()
            {
                NewBookingDate = model.DateBegin.Value.Date,
                NewBookingBegin = model.DateBegin.Value.TimeOfDay,
                NewBookingEnd = GetEstimateTimeEnd(model)
            };

            List<EmployeeSuitableWithBookingLogs> empBLogs = new List<EmployeeSuitableWithBookingLogs>();
            foreach (var emp in listEmpWithSuitableWorkingTime)
            {
                BookingCheckEmployeeModel checkingEmpFree = CheckEmployeeFree(emp.Id, datetimeModel, model);

                if (checkingEmpFree.Check)
                {
                    empBLogs.Add(new EmployeeSuitableWithBookingLogs()
                    {
                        Employee = emp,
                        bLogs = checkingEmpFree.BLogs
                    });
                }
            }

            empBLogs = empBLogs.OrderBy(e => e.bLogs.Count()).ToList();

            // kiem tra xem gio lam viec co phu hop voi thoi gian lam viec da co booking roi khong
            List<SuitableEmployeeModel> listEmpFree = new List<SuitableEmployeeModel>();
            foreach (var ebl in empBLogs)
            {
                listEmpFree.Add(ebl.Employee);
            }

            return listEmpFree;
        }

        public async Task<string> GetSuitableEmployee(Booking model)
        {
            List<EmployeeWithDistanceBooking> emps = new List<EmployeeWithDistanceBooking>();
            try
            {
                List<SuitableEmployeeModel> listEmpFree = await GetSuitableEmployeeFree(model);
                if (listEmpFree == null || listEmpFree.Count == 0)
                {
                    return null;
                }

                // Loại nếu như có reject booking của khách hàng đó rồi
                var setting = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.BAN_REJECT);
                int banHours = int.Parse(setting.Data);

                List<SuitableEmployeeModel> listEmpSuitable = new List<SuitableEmployeeModel>();
                foreach (var emp in listEmpFree)
                {
                    var bookingRejected = _dbContext.BookingLogs.Include(bl => bl.Booking)
                                                                .Where(bl => bl.Booking.CustomerId == model.CustomerId)
                                                                .Where(bl => bl.Booking.EmployeeId == emp.Id)
                                                                .Where(bl => bl.BookingStatusId == ConstBookingStatus.REJECTED)
                                                                .OrderByDescending(bl => bl.Booking.DateCreated)
                                                                .ToList().FirstOrDefault();

                    // add emp neu emp do khong reject booking do
                    if (bookingRejected == null)
                    {
                        listEmpSuitable.Add(emp);
                    }
                    else
                    {
                        TimeSpan banHoursTS = new TimeSpan(banHours, 0, 0);
                        DateTime today = DateTime.UtcNow.AddHours(7);
                        bool isEnough24h = (today.Date - banHoursTS) > bookingRejected.DateCreated.Value.Date;
                        if (isEnough24h)
                        {
                            listEmpSuitable.Add(emp);
                        }
                    }
                }

                if (listEmpSuitable.Count < 1)
                {
                    return null;
                }

                // uu tien theo credit cao nhat
                listEmpSuitable = listEmpSuitable.OrderByDescending(e => e.EmployeeCredit).ToList();

                int listSize = listEmpSuitable.Count;
                if (listSize > 5)
                {
                    listSize = 5;
                }

                DirectionsRequest request = new DirectionsRequest
                {
                    Key = _googleApiKey
                };


                for (int i = 0; i < listSize; i++)
                {
                    emps.Add(new EmployeeWithDistanceBooking()
                    {
                        EmployeeId = listEmpSuitable[i].Id,
                        BookingDistanceMeters = (int)_googleService.GetDistanceInMeters(listEmpSuitable[i], model, request).Data
                    });
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return emps.OrderBy(e => e.BookingDistanceMeters).FirstOrDefault().EmployeeId;
        }
        #endregion

        //public async Task<List<SuitableEmployeeModel>> GetSuitableEmployeeFree1(Booking model)
        //{
        //    List<SuitableEmployeeModel> listEmpWithSuitableWorkingTime = await GetSuitableEmployeeByWorkingTime(model, model.DateBegin.Value);
        //    if (listEmpWithSuitableWorkingTime == null || listEmpWithSuitableWorkingTime.Count() == 0)
        //    {
        //        return null;
        //    }

        //    DateTime dateBegin = model.DateBegin.Value;

        //    List<SuitableEmployeeModel> listEmpFree = new List<SuitableEmployeeModel>();
        //    foreach (var emp in listEmpWithSuitableWorkingTime)
        //    {
        //        var bLogs = _dbContext.BookingLogs.Include(bl => bl.Booking)
        //                                          .Where(bl => bl.Booking.EmployeeId == emp.Id)
        //                                          .Where(bl => bl.Booking.DateBegin.Value.Date == dateBegin.Date)
        //                                          .Where(bl => bl.BookingStatusId == ConstBookingStatus.WAITING).ToList();


        //        int estimatedMovingTime = GetEstimatedMovingTime(dateBegin);
        //        var bLog = bLogs.Where(b => TimeSpan.Compare(dateBegin.TimeOfDay, b.Booking.DateBegin.Value.TimeOfDay.Add(new TimeSpan(0, b.Booking.EstimatedTime + estimatedMovingTime, 0))) <= 0)
        //                              .Where(b => TimeSpan.Compare(dateBegin.TimeOfDay, b.Booking.DateBegin.Value.TimeOfDay) >= 0)
        //                              .FirstOrDefault();

        //        // Vi can Emp co thoi gian ranh nen khi query thoi gian lam viec khong trung voi thoi gian cua booking khac thi moi add
        //        if (bLog == null)
        //        {
        //            listEmpFree.Add(emp);
        //        }
        //    }

        //    return listEmpFree;
        //}

        // theo danh sách nhân viên đã tìm ra ở GetSuitableEmployeeByWorkingTime(Booking model, DateTime dateBegin)
    }
}