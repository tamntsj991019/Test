using AutoMapper;
using Data.Entities;
using Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.MappingProfiles
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            #region USER MODEL
            CreateMap<EmployeeRegisterModel, User>()
                .ReverseMap();
            CreateMap<CustomerRegisterModel, User>()
                .ReverseMap();
            CreateMap<UserViewModel, User>()
                .ReverseMap();

            CreateMap<SuitableEmployeeModel, User>()
                .ReverseMap();
            CreateMap<UserFullnameViewModel, User>()
                .ReverseMap();
            CreateMap<EmployeeBookingDetailsModel, User>()
                .ReverseMap();
            CreateMap<CustomerBookingDetailsModel, User>()
                .ReverseMap();
            CreateMap<UserFullnameAvatarViewModel, User>()
                .ReverseMap();
            CreateMap<UserBookingInWorkingAdViewModel, User>()
                .ReverseMap();

            CreateMap<EmployeeRecommendedModel, User>()
                .ReverseMap();
            #endregion

            #region BOOKING MODEL
            CreateMap<BookingCreateModel, Booking>()
                .ReverseMap();
            CreateMap<BookingEmployeeAgainCreateModel, Booking>()
                .ReverseMap();
            CreateMap<BookingDetailsViewModel, Booking> ()
                .ReverseMap();

            CreateMap<BookingLogMainViewModel, BookingLog>()
                .ReverseMap();
            CreateMap<ServiceBookingDetailViewModel, ServiceBooking>()
                .ReverseMap();
            CreateMap<ServiceDetailViewModel, Service>()
                .ReverseMap();

            //Booking Admin
            CreateMap<BookingListInWorkingAdminViewModel, Booking>()
                .ReverseMap();
            CreateMap<BookingListAdminViewModel, Booking>()
                .ReverseMap();
            CreateMap<BookingDetailsAdminViewModel, Booking>()
                .ReverseMap();
            //Booking Employee
            CreateMap<BookingLogEmployeeByStatusViewModel, BookingLog>()
                .ReverseMap();
            CreateMap<BookingEmployeeByStatusViewModel, Booking>()
                .ReverseMap();
            //Booking Customer
            CreateMap<BookingLogCustomerByStatusViewModel, BookingLog>()
                .ReverseMap();
            CreateMap<BookingCustomerByStatusView, Booking>()
                .ReverseMap();

            // Booking Noti
            CreateMap<BookingNotiAdminViewModel, Booking>()
                .ReverseMap();
            CreateMap<BookingNotiCustomerViewModel, Booking>()
                .ReverseMap();
            CreateMap<BookingNotiEmployeeViewModel, Booking>()
                .ReverseMap();

            //Booking Transaction
            CreateMap<BookingTransactionViewModel, Booking>()
                .ReverseMap();

            //Booking Feedback
            CreateMap<BookingFeedbackViewModel, Booking>()
                .ReverseMap();

            //Booking Update service
            CreateMap<BookingAfterUpdateServiceModel, Booking>()
                .ReverseMap();
            #endregion

            #region BOOKING IMAGE MODEL
            CreateMap<BookingImageCreateModel, BookingImage>()
                .ReverseMap();
            CreateMap<BookingImageViewModel, BookingImage>()
                .ReverseMap();
            CreateMap<ImageIdViewModel, BookingImage>()
                .ReverseMap();
            #endregion

            #region BOOKING LOG MODEL
            CreateMap<BookingLogCreateModel, BookingLog>()
                .ReverseMap();
            CreateMap<BookingLogViewModel, BookingLog>()
                .ReverseMap();
            CreateMap<BookingLogWithImagesViewModel, BookingLog>()
                .ReverseMap();
            #endregion

            #region BOOKING STATUS MODEL
            CreateMap<BookingStatusCreateModel, BookingStatus>()
                .ReverseMap();
            CreateMap<BookingStatusViewModel, BookingStatus>()
                .ReverseMap();
            CreateMap<BaseStringModel, BookingStatus>()
                .ReverseMap();
            #endregion

            #region CLEANING TOOL MODEL
            CreateMap<CleaningToolCreateModel, CleaningTool>()
                .ReverseMap();
            CreateMap<CleaningToolViewModel, CleaningTool>()
                .ReverseMap();
            CreateMap<CleaningToolRequestViewModel, CleaningTool>()
                .ReverseMap();
            #endregion
            
            #region REQUEST CLEANING TOOL MODEL
            CreateMap<RequestCleaningToolCreateModel, RequestCleaningTool>()
                .ReverseMap();
            CreateMap<RequestCleaningToolWithUserViewModel, RequestCleaningTool>()
                .ReverseMap();
            CreateMap<RequestCleaningToolEmployeeViewModel, RequestCleaningTool>()
                .ReverseMap();
            CreateMap<RequestCleaningToolViewModel, RequestCleaningTool>()
                .ReverseMap();
            CreateMap<RequestNotiViewModel, RequestCleaningTool>()
                .ReverseMap();
            CreateMap<RequestNotiAdminViewModel, RequestCleaningTool>()
                .ReverseMap();
            #endregion
            
            #region REQUEST STATUS MODEL
            CreateMap<BaseStringModel, RequestStatus>()
                .ReverseMap();
            #endregion

            #region CONVERSATION MODEL
            CreateMap<ConversationViewModel, Conversation>()
                .ReverseMap();
            CreateMap<ConversationPushingViewModel, Conversation>()
                .ReverseMap();
            #endregion

            #region FEEDBACK MODEL
            CreateMap<FeedbackViewModel, Feedback>()
                .ReverseMap();
            CreateMap<FeedbackCreateModel, Feedback>()
                .ReverseMap();
            CreateMap<FeedbackWithUserBookingViewModel, Feedback>()
                .ReverseMap();
            CreateMap<FeedbackNotiViewModel, Feedback>()
                .ReverseMap();
            #endregion

            #region INTERVAL MODEL
            CreateMap<IntervalCreateModel, Interval>()
                .ReverseMap();
            CreateMap<IntervalViewModel, Interval>()
                .ReverseMap();
            CreateMap<IntervalTimeOnlyModel, Interval>()
                .ReverseMap();
            #endregion

            #region MESSAGE MODEL
            CreateMap<MessageViewModel, Message>()
                .ReverseMap();
            CreateMap<MessageCreateModel, Message>()
                .ReverseMap();
            #endregion

            #region NOTIFICATION MODEL
            CreateMap<NotificationViewModel, Notification>()
                .ReverseMap();
            CreateMap<NotificationCreateModel, Notification>()
                .ReverseMap();
            CreateMap<NotificationPushViewModel, Notification>()
                .ReverseMap();
            CreateMap<NotificationAdminViewModel, Notification>()
                .ReverseMap();
            CreateMap<NotificationCustomerViewModel, Notification>()
                .ReverseMap();
            CreateMap<NotificationEmployeeViewModel, Notification>()
                .ReverseMap();
            #endregion

            #region SCHEDULE MODEL
            CreateMap<ScheduleCreateModel, Schedule>()
                .ReverseMap();
            CreateMap<ScheduleViewModel, Schedule>()
                .ReverseMap();
            CreateMap<ScheduleWithIntervalViewModel, Schedule>()
                .ReverseMap();
            #endregion

            #region  SERVICE MODEL 
            CreateMap<ServiceViewModel, Service>()
               .ReverseMap();
            CreateMap<ServiceAddModel, Service>()
               .ReverseMap();
            CreateMap<ServiceForRequestViewModel, Service>()
               .ReverseMap();

            // ServiceCleaningTool
            //CreateMap<ServiceCleaningToolViewModel, ServiceCleaningTool>()
            //   .ReverseMap();
            #endregion

            #region SERVICE BOOKING MODEL
            CreateMap<ServiceBookingCreateModel, ServiceBooking>()
                .ReverseMap();
            CreateMap<ServiceBookingUpdateModel, ServiceBooking>()
                .ReverseMap();
            CreateMap<ServiceBookingViewModel, ServiceBooking>()
                .ReverseMap();
            #endregion

            #region SERVICE GROUP MODEL
            CreateMap<ServiceGroupCreateModel, ServiceGroup>()
                .ReverseMap();
            CreateMap<ServiceGroupViewModel, ServiceGroup>()
                .ReverseMap();

            CreateMap<ServiceGroupWithServiceViewModel, ServiceGroup>()
                .ReverseMap();
            #endregion

            #region TRANSACTION MODEL
            CreateMap<TransactionUserViewModel, Transaction>()
               .ReverseMap();
            CreateMap<TransactionBookingViewModel, Transaction>()
               .ReverseMap();
            CreateMap<TransactionCompanyViewModel, Transaction>()
               .ReverseMap();

            CreateMap<TransactionCreateModel, Transaction>()
               .ReverseMap();

            #endregion

            #region SETTING MODEL
            CreateMap<SettingCreateModel, Setting>()
                .ReverseMap();
            CreateMap<SettingViewModel, Setting>()
                .ReverseMap();
            CreateMap<SettingUserViewModel, Setting>()
                .ReverseMap();
            CreateMap<UsingCompanyToolViewModel, Setting>()
                .ReverseMap();
            #endregion

            #region SUBPACKET MODEL
            CreateMap<Location, BaseStringModel>()
               .ReverseMap();
            #endregion

            #region ADDRESS MODEL
            CreateMap<Booking, BaseAddressModel>()
               .ReverseMap();
            CreateMap<User, BaseAddressModel>()
               .ReverseMap();
            #endregion

        }
    }
}
