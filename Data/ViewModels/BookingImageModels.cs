using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class BookingImageViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public Guid BookingLogId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsCleaned { get; set; } = false;
        public bool IsConfirmed { get; set; } = false;
    }

    public class BookingImageCreateModel
    {
        public string Description { get; set; }
        public Guid BookingLogId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsCleaned { get; set; }
    }

    public class BookingImageUpdateModel
    {
        public string Description { get; set; }
        public Guid BookingLogId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsCleaned { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
