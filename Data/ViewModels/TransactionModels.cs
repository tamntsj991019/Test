using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class TransactionUserViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public double Deposit { get; set; }
        public string UserId { get; set; }
        public UserFullnameAvatarViewModel User { get; set; }
        public Guid? BookingId { get; set; }
    }

    public class TransactionBookingViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid? BookingId { get; set; }
        public BookingTransactionViewModel Booking { get; set; }
    }

    public class TransactionCompanyViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public double Deposit { get; set; }
        public string UserId { get; set; }
        public UserFullnameAvatarViewModel User { get; set; }
        public Guid? BookingId { get; set; }
        public BookingTransactionViewModel Booking { get; set; }
    }

    public class TransactionCreateModel
    {
        public string Description { get; set; }
        public string UserId { get; set; }
        public double? Deposit { get; set; }
        public Guid? BookingId { get; set; }
        public string Type { get; set; }
    }
    
    public class TransactionUserHistoryModel
    {
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public string Money { get; set; }
        public bool IsBooking { get; set; }
    }
    
}
