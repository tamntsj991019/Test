using Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    #region Booking Details
    public class BookingLogMainViewModel
    {
        public Guid Id { get; set; }
        public string BookingStatusId { get; set; }
        public virtual BaseStringModel BookingStatus { get; set; }
        public Guid BookingId { get; set; }
        public BookingDetailsViewModel Booking { get; set; }
    }

    public class BookingDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public int EstimatedTime { get; set; }
        public double? TotalPrice { get; set; }

        public string EmployeeId { get; set; }
        public EmployeeBookingDetailsModel Employee { get; set; }
        public string CustomerId { get; set; }
        public CustomerBookingDetailsModel Customer { get; set; }

        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }

        public string ProvinceId { get; set; }
        public BaseStringModel Province { get; set; }
        public string DistrictId { get; set; }
        public BaseStringModel District { get; set; }
        public string WardId { get; set; }
        public BaseStringModel Ward { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public bool UseCompanyTool { get; set; }
        public double UseCompanyToolPrice { get; set; }

        public bool IsCustomerFeedback { get; set; }
        public bool IsEmployeeFeedback { get; set; }
        public bool IsCleaningAll { get; set; }
        public List<ServiceBookingDetailViewModel> ServiceBookings { get; set; }
    }
    
    public class ServiceBookingDetailViewModel
    {
        public Guid ServiceId { get; set; }
        public ServiceDetailViewModel Service { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public int EstiamtedMinutes { get; set; }
        public bool IsDisable { get; set; }
    }

    public class ServiceDetailViewModel
    {
        public string Description { get; set; }
        public bool CanInputQuantity { get; set; }
        public string Type { get; set; }
    }
    #endregion

    #region Admin Booking
    public class BookingListInWorkingAdminViewModel
    {
        public Guid Id { get; set; }
        public string BookingStatusId { get; set; }
        public virtual BaseStringModel BookingStatus { get; set; }
        public string EmployeeId { get; set; }
        public UserBookingInWorkingAdViewModel Employee { get; set; }
        public string CustomerId { get; set; }
        public UserBookingInWorkingAdViewModel Customer { get; set; }
        public DateTime? DateBegin { get; set; }
        public BaseStringModel Province { get; set; }
        public string DistrictId { get; set; }
        public BaseStringModel District { get; set; }
        public string WardId { get; set; }
        public BaseStringModel Ward { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    
    public class BookingListAdminViewModel
    {
        public Guid Id { get; set; }
        public string BookingStatusId { get; set; }
        public virtual BaseStringModel BookingStatus { get; set; }
        public string EmployeeId { get; set; }
        public UserFullnameViewModel Employee { get; set; }
        public string CustomerId { get; set; }
        public UserFullnameViewModel Customer { get; set; }
        public DateTime? DateBegin { get; set; }
        public BaseStringModel Province { get; set; }
        public string DistrictId { get; set; }
        public BaseStringModel District { get; set; }
        public string WardId { get; set; }
        public BaseStringModel Ward { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public DateTime? DateCreated { get; set; }
    }

    public class BookingDetailsAdminViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public int EstimatedTime { get; set; }
        public double? TotalPrice { get; set; }

        public string BookingStatusId { get; set; }
        public virtual BaseStringModel BookingStatus { get; set; }

        public string EmployeeId { get; set; }
        public EmployeeBookingDetailsModel Employee { get; set; }
        public string CustomerId { get; set; }
        public CustomerBookingDetailsModel Customer { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }

        public string ProvinceId { get; set; }
        public BaseStringModel Province { get; set; }
        public string DistrictId { get; set; }
        public BaseStringModel District { get; set; }
        public string WardId { get; set; }
        public BaseStringModel Ward { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public bool UseCompanyTool { get; set; }
        public double UseCompanyToolPrice { get; set; }

        public bool IsCustomerFeedback { get; set; }
        public bool IsEmployeeFeedback { get; set; }
        public List<ServiceBookingDetailViewModel> ServiceBookings { get; set; }
    }
    
    public class EmployeeRecommendedModel
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
        public string HasAvatar { get; set; }
        public string EmployeeCode { get; set; }
        public double AvgRating { get; set; }
        public double Distance { get; set; }
        public int NumberOfBooking { get; set; }
        public int EmployeeCredit { get; set; }
    }

    public class BookingWithRecomendEmpModel
    {
        public double Distance { get; set; }
        public BookingDetailsAdminViewModel BookingDetail { get; set; }
        public List<EmployeeRecommendedModel> ListEmp { get; set; }
    }
    
    public class BookingAssigningEmployeeModel
    {
        public bool IsSuceed { get; set; } = false;
        public string  Message { get; set; }
    }
    #endregion

    #region Employee Booking
    public class BookingLogEmployeeByStatusViewModel
    {
        public Guid Id { get; set; }
        public string BookingStatusId { get; set; }
        public virtual BaseStringModel BookingStatus { get; set; }
        public Guid BookingId { get; set; }
        public BookingEmployeeByStatusViewModel Booking { get; set; }
    }

    public class BookingEmployeeByStatusViewModel
    {
        public Guid Id { get; set; }
        public string CustomerId { get; set; }
        public UserFullnameAvatarViewModel Customer { get; set; }
        public double? TotalPrice { get; set; }
        public string ProvinceId { get; set; }
        public BaseStringModel Province { get; set; }
        public string DistrictId { get; set; }
        public BaseStringModel District { get; set; }
        public string WardId { get; set; }
        public BaseStringModel Ward { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public bool IsEmployeeFeedback { get; set; }
    }
    #endregion

    #region Customer Booking
    public class BookingLogCustomerByStatusViewModel
    {
        public Guid Id { get; set; }
        public string BookingStatusId { get; set; }
        public virtual BaseStringModel BookingStatus { get; set; }
        public Guid BookingId { get; set; }
        public BookingCustomerByStatusView Booking { get; set; }
    }

    public class BookingCustomerByStatusView
    {
        public Guid Id { get; set; }
        public string EmployeeId { get; set; }
        public UserFullnameAvatarViewModel Employee { get; set; }
        public double? TotalPrice { get; set; }
        public string ProvinceId { get; set; }
        public BaseStringModel Province { get; set; }
        public string DistrictId { get; set; }
        public BaseStringModel District { get; set; }
        public string WardId { get; set; }
        public BaseStringModel Ward { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public bool IsCustomerFeedback { get; set; }
    }
    #endregion

    #region Create

    public class BookingCreateModel
    {
        public string EmployeeId { get; set; }
        public string Description { get; set; }

        public int EstimatedTime { get; set; }
        public double TotalPrice { get; set; }

        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DateTime DateBegin { get; set; }

        public bool UseCompanyTool { get; set; }
        public double UseCompanyToolPrice { get; set; }

        public List<ServiceBookingCreateModel> ServiceBookings { get; set; }
    }

    public class BookingEmployeeAgainCreateModel
    {
        public string EmployeeId { get; set; }
        public string Description { get; set; }

        public int EstimatedTime { get; set; }
        public double TotalPrice { get; set; }

        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DateTime DateBegin { get; set; }

        public bool UseCompanyTool { get; set; }
        public double UseCompanyToolPrice { get; set; }

        public List<ServiceBookingCreateModel> ServiceBookings { get; set; }
    }

    #endregion

    public class BookingUpdateServiceDetailsModel
    {
        public int EstimatedTime { get; set; }
        public double TotalPrice { get; set; }
        public bool UseCompanyTool { get; set; }
        public double UseCompanyToolPrice { get; set; }
        public List<ServiceBookingUpdateModel> ServiceBookings { get; set; }
    }

    #region Noti

    public class BookingNotiAdminViewModel
    {
        public Guid Id { get; set; }
        public string CustomerId { get; set; }
        public UserFullnameAvatarViewModel Customer { get; set; }
    }
    
    public class BookingNotiCustomerViewModel
    {
        public Guid Id { get; set; }
        public string EmployeeId { get; set; }
        public UserFullnameAvatarViewModel Employee { get; set; }
    }

    public class BookingNotiEmployeeViewModel
    {
        public Guid Id { get; set; }
        public string CustomerId { get; set; }
        public UserFullnameAvatarViewModel Customer { get; set; }
    }

    #endregion

    public class BookingAfterCreatingModel
    {
        public Guid BookingId { get; set; }
        public string BookingStatusId { get; set; }
    }

    public class BookingTransactionViewModel
    {
        public Guid Id { get; set; }
        public double? TotalPrice { get; set; }
        public string EmployeeId { get; set; }
        public UserFullnameViewModel Employee { get; set; }
        public string CustomerId { get; set; }
        public UserFullnameViewModel Customer { get; set; }
    }
    
    public class BookingFeedbackViewModel
    {
        public Guid Id { get; set; }
        public double? TotalPrice { get; set; }
        public DateTime? DateBegin { get; set; }
    }

    public class BookingAfterUpdateServiceModel
    {
        public Guid BookingId { get; set; }
        public bool IsUpdateSuccess { get; set; }
        public string Message { get; set; }
    }

    public class BookingAfterCheckingEmployeeFree
    {
        public bool IsEmployeeSuitable { get; set; }
        public string EmployeeStatus { get; set; }
        public string EmployeeId { get; set; }
        public EmployeeBookingDetailsModel Employee { get; set; }
    }

    //public class BookingUpdateLocationModel
    //{
    //    public string Description { get; set; }
    //    public DateTime DateBegin { get; set; }
    //    public string ProvinceId { get; set; }
    //    public string DistrictId { get; set; }
    //    public string WardId { get; set; }
    //    public string Address { get; set; }
    //    public string AddressDetail { get; set; }
    //    public double Latitude { get; set; }
    //    public double Longitude { get; set; }
    //}

    //#region Booking Feedback
    //public class BookingFeedbackCustomerViewModel
    //{
    //    public Guid Id { get; set; }
    //    public string EmployeeId { get; set; }
    //    public UserFullnameAvatarViewModel Employee { get; set; }
    //}
    //public class BookingFeedbackEmployeeViewModel
    //{
    //    public Guid Id { get; set; }
    //    public string CustomerId { get; set; }
    //    public UserFullnameAvatarViewModel Customer { get; set; }
    //}
    //#endregion
}
