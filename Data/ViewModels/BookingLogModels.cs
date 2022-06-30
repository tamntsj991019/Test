using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class BookingLogViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public Guid BookingId { get; set; }
        public string BookingStatusId { get; set; }
    }

    public class BookingLogCreateModel
    {
        public string Description { get; set; }
        public Guid BookingId { get; set; }
        public string BookingNote { get; set; }
        public string BookingStatusId { get; set; }
    }

    public class BookingLogUpdateModel
    {
        public string Description { get; set; }
        public Guid BookingId { get; set; }
        public string BookingStatusId { get; set; }
    }

    public class ImagesCustomerViewModel
    {
        public bool ShowRecleanBtn { get; set; }
        public List<Guid> ReferenceImages { get; set; } 
        public List<Guid> EvidenceImages { get; set; } 
    }
    
    public class ImagesUserViewModel
    {
        public List<Guid> ReferenceImages { get; set; } 
        public List<Guid> EvidenceImages { get; set; } 
    }

    public class BookingLogWithImagesViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public Guid BookingId { get; set; }
        public string BookingStatusId { get; set; }
        public List<ImageIdViewModel> BookingImages { get; set; }
    }

    public class ImageIdViewModel
    {
        public Guid Id { get; set; }
    }
}
