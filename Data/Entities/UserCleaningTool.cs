 using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class UserCleaningTool
    {
        public string EmployeeId { get; set; }
        [Key, ForeignKey("EmployeeId")]
        public virtual User Employee { get; set; }

        public Guid CleaningToolId { get; set; }
        [Key, ForeignKey("CleaningToolId")]
        public virtual CleaningTool CleaningTool { get; set; }

        public int Quantity { get; set; } = 0;

        public bool IsDisable { get; set; } = false;
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime? DateUpdated { get; set; } = DateTime.UtcNow.AddHours(7);
    }
}
