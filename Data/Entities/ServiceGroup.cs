using Data.ConstData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.Entities
{
    public class ServiceGroup : BaseEntity
    {
        public byte[] Image { get; set; }
        public string Type { get; set; } = ConstServiceGroupType.NORMAL; // NORMAL, OPTIONAL, OVERALL

        public Guid? HasImage => (Image != null) ? Id : Guid.Empty;

        public virtual ICollection<Service> Services { get; set; } = new HashSet<Service>();
    }
}
