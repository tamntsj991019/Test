using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.Entities
{
    public class CleaningTool : BaseEntity
    {
        public int Quantity { get; set; } = 0;
        public byte[] Image { get; set; }

        public Guid HasImage => (Image != null) ? Id : Guid.Empty;

        public virtual ICollection<ServiceCleaningTool> ServiceCleaningTools { get; set; } = new HashSet<ServiceCleaningTool>();
    }

}
