using Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{

    public class BookingCheckingEmployeeFreeModel
    {
        public bool IsFree { get; set; } = false;
        public List<BookingLog> BLogs { get; set; }
    }

    public class BookingCheckEmployeeModel
    {
        public bool Check { get; set; } = false;
        public List<BookingLog> BLogs { get; set; }
    }
    
    public class BookingCheckEmployeeWithDistanceModel
    {
        public bool Check { get; set; } = false;
        public double Distance { get; set; } = 0;
        public List<BookingLog> BLogs { get; set; }
    }

    public class DatetimeNewBookingModel
    {
        public DateTime NewBookingDate { get; set; }
        public TimeSpan NewBookingBegin { get; set; }
        public TimeSpan NewBookingEnd { get; set; }
    }

    public class BaseAddressModel
    {
        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
    }

    public class EmployeeDistanceModel
    {
        public bool IsInRadius { get; set; }
        public double Distance { get; set; }
    }

    public class GoogleDistanceAndDuration
    {
        public double Distance { get; set; }
        public int Duration { get; set; }
    }

    public class GoogleDistanceAndDuration2Ways
    {
        public GoogleDistanceAndDuration FromAToB { get; set; }
        public GoogleDistanceAndDuration FromBToA { get; set; }
    }
}
