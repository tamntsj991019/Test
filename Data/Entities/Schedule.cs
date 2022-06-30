using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class Schedule : BaseEntity
    {
        public DateTime DateWorking { get; set; }

        public string EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual User Employee { get; set; }

        public virtual ICollection<Interval> Intervals { get; set; } = new HashSet<Interval>();
    }
}
