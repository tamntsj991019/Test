using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class ScheduleViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime DateWorking { get; set; }
        public string EmployeeId { get; set; }
        //public List<> Intervals { get; set; }
    }
    
    public class ScheduleWithIntervalViewModel
    {
        public Guid Id { get; set; }
        public DateTime DateWorking { get; set; }
        public List<IntervalTimeOnlyModel> Intervals { get; set; }
    }

    public class ScheduleCreateModel
    {
        public DateTime DateWorking { get; set; }
        public List<IntervalCreateModel> Intervals { get; set; }
    }

    public class ScheduleUpdateModel
    {
        public string Description { get; set; }
        public DateTime DateWorking { get; set; }
        public List<IntervalUpdateModel> Intervals { get; set; }
    }
}
