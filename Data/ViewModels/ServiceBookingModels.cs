using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class ServiceBookingViewModel
    {
        public Guid BookingId { get; set; }
        public Guid ServiceId { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public int EstiamtedMinutes { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }

    public class ServiceBookingUpdateModel
    {
        public Guid ServiceId { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public int EstiamtedMinutes { get; set; }
        public bool IsDisable { get; set; }
    }

    public class ServiceBookingCreateModel
    {
        public Guid ServiceId { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public int EstiamtedMinutes { get; set; }
    }
}
