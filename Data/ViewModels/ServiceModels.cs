using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class ServiceAddModel
    {
        public double UnitPrice { get; set; }
        public bool CanInputQuantity { get; set; }
        public Guid ServiceGroupId { get; set; }
        public string Description { get; set; }
        public int EstiamtedMinutes { get; set; }
        public string Type { get; set; }
    }

    public class ServiceUpdateModel
    {
        public double UnitPrice { get; set; }
        public int EstiamtedMinutes { get; set; }
        public bool CanInputQuantity { get; set; }
        public Guid ServiceGroupId { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
    }

    public class ServiceViewModel
    {
        public Guid Id { get; set; }
        public double UnitPrice { get; set; }
        public int EstiamtedMinutes { get; set; }
        public bool CanInputQuantity { get; set; }
        public Guid ServiceGroupId { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }

    public class ServiceForRequestViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        //public List<ServiceCleaningToolViewModel> ServiceCleaningTools { get; set; }
        public List<CleaningToolRequestViewModel> CleaningTools { get; set; } = new List<CleaningToolRequestViewModel>();
    }

    //public class ServiceCleaningToolViewModel
    //{
    //    public CleaningToolRequestViewModel CleaningTool { get; set; }
    //}

}
