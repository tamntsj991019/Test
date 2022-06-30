using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class Conversation //: BaseEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid BookingId { get; set; }
        [Key, ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        public bool IsDisable { get; set; } = false;
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime? DateUpdated { get; set; } = DateTime.UtcNow.AddHours(7);

        public virtual ICollection<Message> Messages { get; set; } = new HashSet<Message>();
    }
}
