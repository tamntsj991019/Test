using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class FeedbackViewModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid BookingId { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

    }
    
    public class FeedbackWithUserBookingViewModel
    {
        public string UserId { get; set; }
        public UserFullnameAvatarViewModel User { get; set; }
        public Guid BookingId { get; set; }
        public BookingFeedbackViewModel Booking { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }

    }

    public class FeedbackNotiViewModel
    {
        public string UserId { get; set; }
        public UserFullnameAvatarViewModel User { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
    }

    public class FeedbackCreateModel
    {
        public Guid BookingId { get; set; }
        public double Rating { get; set; }
        public string Description { get; set; }
    }

    public class FeedbackUpdateModel
    {
        public int Rating { get; set; }
        public string Description { get; set; }
    }
}
