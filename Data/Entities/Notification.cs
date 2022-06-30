using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; }
        public string Type { get; set; } // BOOKING - FEEDBACK
        public bool Seen { get; set; } = false;

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public Guid? BookingId { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        public Guid? FeedbackId { get; set; }
        [ForeignKey("FeedbackId")]
        public virtual Feedback Feedback { get; set; }

        public Guid? RequestCleaningToolId { get; set; }
        [ForeignKey("RequestCleaningToolId")]
        public virtual RequestCleaningTool RequestCleaningTool { get; set; }
    }
}
