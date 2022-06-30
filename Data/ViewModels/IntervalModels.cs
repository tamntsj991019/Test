using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
  
    public class IntervalViewModel
    {
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public Guid? ScheduleId { get; set; }
        public Guid Id { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; } = false;
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime? DateUpdated { get; set; } = DateTime.UtcNow.AddHours(7);
    }

    public class IntervalCreateModel
    {
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public Guid? ScheduleId { get; set; }
    }

    public class IntervalUpdateModel
    {
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }

    }
    
    public class IntervalTimeOnlyModel
    {
        public Guid Id { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }

    }
}
