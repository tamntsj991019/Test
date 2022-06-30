using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class BookingStatusViewModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }

    public class BookingStatusCreateModel
    {
        public string Description { get; set; }
    }

    public class BookingStatusUpdateModel
    {
        public string Description { get; set; }
    }
}
