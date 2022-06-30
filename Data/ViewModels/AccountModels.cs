using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.ViewModels
{
    
    public class UserLoginViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class UserLoginSuccessViewModel
    {
        public string HasAvatar { get; set; }
        public string Role { get; set; }
        public string Fullname { get; set; }
        public object Token { get; set; }
    }
    
    public class UserAfterLoginViewModel
    {
        public string HasAvatar { get; set; }
        public string Role { get; set; }
        public string Fullname { get; set; }
        public object Token { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public bool Succeeded { get; set; }
    }

    public class EmployeeRegisterModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
        public string Password { get; set; }
        [Required]
        public string Fullname { get; set; }
        public string Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public IFormFile AvatarFile { get; set; }
    }
    
    public class CustomerRegisterModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
        public string Password { get; set; }
        [Required]
        public string Fullname { get; set; }
        public string PhoneNumber { get; set; }
        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class ActiveCodeModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
    }
    
    public class UsernameConfirmModel
    {
        public string Username { get; set; }
        public string Code { get; set; }
    }
    
    public class EmailConfirmModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class EmailValidModel
    {
        public string Message { get; set; }
        public bool EmailExisted { get; set; }
    }

    public class UserEditPasswordViewModel
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
    
    public class UserEditPasswordForgotViewModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string HasAvatar { get; set; }
        public bool EmailConfirmed { get; set; }
        public double Balance { get; set; }
        public bool IsDisable { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string Fullname { get; set; }
        public string Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string ProvinceId { get; set; }
        public BaseStringModel Province { get; set; }
        public string DistrictId { get; set; }
        public BaseStringModel District { get; set; }
        public string WardId { get; set; }
        public BaseStringModel Ward { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string EmployeeCode { get; set; }
        public int? EmployeeCredit { get; set; }
    }

    public class UserUpdateModel
    {
        public string Fullname { get; set; }
        public string Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class UserFullnameViewModel
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
    }

    public class EmployeeBookingDetailsModel
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
        public string HasAvatar { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string EmployeeCode { get; set; }
        public int? EmployeeCredit { get; set; }
        public double AvgRating { get; set; }
    }
    
    public class CustomerBookingDetailsModel
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
        public string HasAvatar { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class UserFullnameAvatarViewModel
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
        public string HasAvatar { get; set; }
    }
    
    public class UserBookingInWorkingAdViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Fullname { get; set; }
        public string HasAvatar { get; set; }
    }
    
    //public class UserBookingListAdminViewModel
    //{
    //    public string Id { get; set; }
    //    public string Fullname { get; set; }
    //    public string HasAvatar { get; set; }
    //}
}
