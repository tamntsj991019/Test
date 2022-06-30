using Data.ConstData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Entities
{
    public class Service : BaseEntity
    {
        public double UnitPrice { get; set; } = 0;
        public int EstiamtedMinutes { get; set; } = 0;
        public bool CanInputQuantity { get; set; } = true;
        public string Type { get; set; } // Quantity - Area


        public Guid ServiceGroupId { get; set; }
        [ForeignKey("ServiceGroupId")]
        public virtual ServiceGroup ServiceGroup { get; set; }

        public virtual ICollection<ServiceCleaningTool> ServiceCleaningTools { get; set; } = new HashSet<ServiceCleaningTool>();
    }
}
