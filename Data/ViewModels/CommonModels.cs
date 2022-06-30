using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class SettingSetupModel
    {
        public UsingCompanyToolViewModel UsingCompanyTool { get; set; }
        public BookingTimeFrameModel BookingTimeFrame { get; set; }
        public List<CleaningAllModel> CleaningAlls { get; set; }
    }

    public class BookingSetupModel
    {
        public SettingSetupModel SettingService { get; set; }
        public List<ServiceGroupWithServiceViewModel> ListServiceGroup { get; set; }
    }

}
