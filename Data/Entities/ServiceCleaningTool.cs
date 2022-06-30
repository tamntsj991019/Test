using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class ServiceCleaningTool
    {
        public Guid ServiceId { get; set; }
        [Key, ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }

        public Guid CleaningToolId { get; set; }
        [Key, ForeignKey("CleaningToolId")]
        public virtual CleaningTool CleaningTool { get; set; }

        public bool IsDisable { get; set; } = false;
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime? DateUpdated { get; set; } = DateTime.UtcNow.AddHours(7);
    }
}
