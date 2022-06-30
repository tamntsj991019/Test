using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class BookingLog : BaseEntity
    {
        public string BookingNote { get; set; }

        public Guid BookingId { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        public string BookingStatusId { get; set; }
        [ForeignKey("BookingStatusId")]
        public virtual BookingStatus BookingStatus { get; set; }

        public virtual ICollection<BookingImage> BookingImages { get; set; } = new HashSet<BookingImage>();
    }
}
