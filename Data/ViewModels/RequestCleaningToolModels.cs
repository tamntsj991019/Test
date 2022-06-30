using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class RequestCleaningToolViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string EmployeeId { get; set; }
        public Guid CleaningToolId { get; set; }
        public string RequestStatusId { get; set; }
    }
    
    public class RequestCleaningToolWithUserViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public bool IsDisable { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string EmployeeId { get; set; }
        public UserFullnameAvatarViewModel Employee { get; set; }
        public Guid CleaningToolId { get; set; }
        public CleaningToolRequestViewModel CleaningTool { get; set; }
        public string RequestStatusId { get; set; }
        public  BaseStringModel RequestStatus { get; set; }
    }
    
    public class RequestCleaningToolEmployeeViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public Guid CleaningToolId { get; set; }
        public CleaningToolRequestViewModel CleaningTool { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string RequestStatusId { get; set; }
        public  BaseStringModel RequestStatus { get; set; }
    }

    public class RequestCleaningToolCreateModel
    {
        //public string Description { get; set; }
        public Guid CleaningToolId { get; set; }
    }

    public class RequestNotiViewModel
    {
        public Guid Id { get; set; }
        public Guid CleaningToolId { get; set; }
        public CleaningToolRequestViewModel CleaningTool { get; set; }
    }
    
    public class RequestNotiAdminViewModel
    {
        public Guid Id { get; set; }
        public Guid CleaningToolId { get; set; }
        public CleaningToolRequestViewModel CleaningTool { get; set; }
        public string EmployeeId { get; set; }
        public UserFullnameAvatarViewModel Employee { get; set; }
    }

    public class RequestUpdateStatusReturnModel
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public bool IsUpdateSuccess { get; set; }
    }
}
