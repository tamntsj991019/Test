using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class BookingImage : BaseEntity
    {
        public Guid BookingLogId { get; set; }
        [ForeignKey("BookingLogId")]
        public virtual BookingLog BookingLog { get; set; }

        public string FileExtension { get; set; }
        public string FileType { get; set; } = "image/jpeg";
    }
}
