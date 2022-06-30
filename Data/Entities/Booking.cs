using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class Booking : BaseEntity
    {
        public string EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual User Employee { get; set; }

        public string CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }

        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }

        public int EstimatedTime { get; set; } = 0;
        public double TotalPrice { get; set; } = 0;

        public bool UseCompanyTool { get; set; } = false;
        public double UseCompanyToolPrice { get; set; } = 0;

        public bool IsCustomerFeedback { get; set; } = false;
        public bool IsEmployeeFeedback { get; set; } = false;

        public string ProvinceId { get; set; }
        [ForeignKey("ProvinceId")]
        public virtual Location Province { get; set; }

        public string DistrictId { get; set; }
        [ForeignKey("DistrictId")]
        public virtual Location District { get; set; }

        public string WardId { get; set; }
        [ForeignKey("WardId")]
        public virtual Location Ward { get; set; }

        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public virtual ICollection<BookingLog> BookingLogs { get; set; } = new HashSet<BookingLog>();
        public virtual ICollection<ServiceBooking> ServiceBookings { get; set; } = new HashSet<ServiceBooking>();
    }
}
