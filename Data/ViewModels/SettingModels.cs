using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class SettingViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
        public string Data { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }

    public class SettingUserViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Data { get; set; }
    }
    
    public class UsingCompanyToolViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
    }

    public class SettingCreateModel
    {
        public string Description { get; set; }
        public string Key { get; set; }
        public string Data { get; set; }
    }

    public class SettingUpdateModel
    {
        public string Description { get; set; }
        public string Data { get; set; }
    }

    public class TrafficJamTimeModel
    {
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
    }

    public class CleaningAllModel
    {
        public double AreaFrom { get; set; }
        public double AreaTo { get; set; }
        public double Price { get; set; }
        public int EstimateTime { get; set; }
    }

    public class BookingTimeFrameModel
    {
        public int MinHour { get; set; }
        public int MaxHour { get; set; }
    }

    public class StarCalculateModel
    {
        public int RatingPoint { get; set; }
        public int AbovePoint { get; set; }
        public int UnderPoint { get; set; }
    }

}
