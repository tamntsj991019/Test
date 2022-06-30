using Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class SuitableEmployeeModel
    {
        public string Id { get; set; }
        public int EmployeeCredit { get; set; }
        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Distance { get; set; }
    }

    public class EmployeeSuitableWithBookingLogs
    {
        public SuitableEmployeeModel Employee { get; set; }
        public List<BookingLog> bLogs { get; set; } = new List<BookingLog>();
    }

    public class EmployeeWithDistanceBooking
    {
        public string EmployeeId { get; set; }
        public int BookingDistanceMeters { get; set; }
    }
    public class EmployeeWithBookingLogs
    {
        public string EmployeeId { get; set; }
        public double Distance { get; set; }
        public List<BookingLog> bLogs { get; set; } = new List<BookingLog>();
    }
}
