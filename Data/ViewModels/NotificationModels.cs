using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class NotificationViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool Seen { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? FeedbackId { get; set; }
    }
    
    public class NotificationAdminViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Seen { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public Guid? BookingId { get; set; }
        public BookingNotiAdminViewModel Booking { get; set; }
        public Guid? RequestCleaningToolId { get; set; }
        public RequestNotiAdminViewModel RequestCleaningTool { get; set; }
    }
    
    public class NotificationCustomerViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Seen { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public Guid? BookingId { get; set; }
        public BookingNotiCustomerViewModel Booking { get; set; }
        public Guid? FeedbackId { get; set; }
        public FeedbackNotiViewModel Feedback { get; set; }
    }
    
    public class NotificationEmployeeViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Seen { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public Guid? BookingId { get; set; }
        public BookingNotiEmployeeViewModel Booking { get; set; }
        public Guid? FeedbackId { get; set; }
        public FeedbackNotiViewModel Feedback { get; set; }
        public Guid? RequestCleaningToolId { get; set; }
        public RequestNotiViewModel RequestCleaningTool { get; set; }
    }

    public class NotificationPushViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } 
        public string Description { get; set; }
        public string Type { get; set; }
        public Guid? BookingId { get; set; }
    }

    public class NotificationCreateModel
    {
        public string Description { get; set; }
        public string UserId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? FeedbackId { get; set; }
        public Guid? RequestCleaningToolId { get; set; }
        public string Type { get; set; }
    }

    public class NotificationUpdateModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? FeedbackId { get; set; }
        public Guid? RequestCleaningToolId { get; set; }
        public string Type { get; set; }
    }
}
