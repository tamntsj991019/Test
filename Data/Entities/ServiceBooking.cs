using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class ServiceBooking
    {
        public Guid BookingId { get; set; }
        [Key, ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        public Guid ServiceId { get; set; }
        [Key, ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }

        public int Quantity { get; set; } = 0;
        public double UnitPrice { get; set; } = 0;
        public int EstiamtedMinutes { get; set; } = 0;
        public bool IsDisable { get; set; } = false;
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime? DateUpdated { get; set; } = DateTime.UtcNow.AddHours(7);
    }
}
