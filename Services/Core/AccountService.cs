using AutoMapper;
using Data.DbContext;
using Data.Entities;
using Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Services.Util;
using Data.ConstData;
using Hubs;

namespace Services.Core
{
    public interface IAccountService
    {
        Task<ResultModel> Login(UserLoginViewModel model);
        Task<ResultModel> RegisterEmployee(EmployeeRegisterModel model);
        Task<ResultModel> RegisterCustomer(CustomerRegisterModel model);

        Task<ResultModel> UpdatePassword(UserEditPasswordViewModel model, string userId);

        Task<ResultModel> UpdatePasswordForgot(UserEditPasswordForgotViewModel model);
        ResultModel ForgotPassword(string username); 
        ResultModel ConfirmForgotPasswordByCode(UsernameConfirmModel model);

        ResultModel CheckEmailToSendActiveCode(ActiveCodeModel model);
        ResultModel GetActiveCode(ActiveCodeModel model);
        ResultModel ConfirmEmail(EmailConfirmModel model);
        //Task<ResultModel> ChangePasswordByCode(string email, string password, string code);

        ResultModel Get(string id);
        ResultModel GetUserEnable(string id);
        Task<ResultModel> GetUserByRole(string userName, string roleId, int pageIndex, int pageSize);
        ResultModel GetBalance(string id);

        ResultModel UpdateDetail(string id, UserUpdateModel model);
        ResultModel UpdateUserStatus(string id, bool isDisable);
        ResultModel UpdateBalance(string id, double money);

        FileModel GetAvatar(string userId);
        Task<ResultModel> AddAvatar(IFormFile file, string userId);
    }

    public class AccountService : IAccountService
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ISendMailService _sendMailService;
        private readonly INotificationService _notificationService;
        private readonly INotificationHub _notificationHub;

        private readonly Utils _utils = new Utils();
        private readonly TimeSpan Expired_Time = new TimeSpan(0, 0, 5, 0);
        private const int CODE_LENGTH = 6;
        private readonly int MaxFileSize;

        public AccountService(SignInManager<User> signInManager, UserManager<User> userManager, IMapper mapper, AppDbContext dbContext, IConfiguration configuration, 
                              ISendMailService sendMailService, INotificationService notificationService, INotificationHub notificationHub)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _dbContext = dbContext;
            _sendMailService = sendMailService;
            _notificationService = notificationService;
            _notificationHub = notificationHub;

            _configuration = configuration;
            MaxFileSize = int.Parse(_configuration["MaxFileSizeAllowed"]);
        }

        public object GenerateJwtToken(User user, string role)
        {
            var claims = new List<Claim>
            {
                //new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, role),
                //new Claim(ClaimTypes.GivenName, fullname)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuerOptions:Issuer"],
                _configuration["JwtIssuerOptions:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ResultModel> Login(UserLoginViewModel model)
        {
            var result = new ResultModel();
            try
            {
                var signInResult = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                if (signInResult.Succeeded)
                {
                    var appUser = _userManager.Users.FirstOrDefault(u => u.UserName == model.UserName);
                    var isDisable = _dbContext.Users.FirstOrDefault(u => u.Id == appUser.Id).IsDisable;
                    if (!isDisable)
                    {
                        if (appUser.EmailConfirmed == true)
                        {
                            var rolesUser = await _userManager.GetRolesAsync(appUser);
                            var token = GenerateJwtToken(appUser, rolesUser[0]);
                            UserAfterLoginViewModel successModel = new UserAfterLoginViewModel()
                            {
                                HasAvatar = (appUser.Avatar != null) ? appUser.Id : null,
                                Fullname = appUser.Fullname,
                                Role = rolesUser[0],
                                Token = token,
                                Succeeded = true
                            };
                            result.Data = successModel;
                        }
                        else
                        {
                            UserAfterLoginViewModel failModel = new UserAfterLoginViewModel()
                            {
                                Id = appUser.Id,
                                Email = appUser.Email,
                                Succeeded = false
                            };
                            result.Data = failModel;
                        }
                        result.Succeed = true;
                    }
                    else
                    {
                        throw new Exception("Tài khoản của bạn đẫ bị khoá");
                    }
                }
                else
                {
                    throw new Exception("Tài khoản hoặc mật khẩu không chính xác");
                }
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> RegisterEmployee(EmployeeRegisterModel model)
        {
            var result = new ResultModel();
            try
            {
                var isExistUserName = _dbContext.Users.Any(u => u.UserName == model.UserName);
                if (isExistUserName)
                {
                    throw new Exception("UserName existed");
                }
                User user = _mapper.Map<EmployeeRegisterModel, User>(model);
                user.EmailConfirmed = true;

                var setting = _dbContext.Settings.FirstOrDefault(s => s.Key == ConstSetting.EMPLOYEE_CODE_LENGHT);
                int empCodeLenght = int.Parse(setting.Data);

                string empCode = "NULL";
                while (true)
                {
                    empCode = _utils.GenerateEmployeeCode(empCodeLenght);
                    bool isExistedEmpCode = _dbContext.Users.Any(u => u.EmployeeCode == empCode);
                    if (isExistedEmpCode == false)
                    {
                        user.EmployeeCode = empCode;
                        break;
                    }
                }


                var createUser = await _userManager.CreateAsync(user, model.Password);
                if (createUser.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, ConstRole.EMPLOYEE);
                    _dbContext.SaveChanges();

                    result.Data = user.Id;
                    result.Succeed = true;

                    if (model.AvatarFile != null)
                    {
                        await AddAvatar(model.AvatarFile, user.Id);
                    }

                    var cleaningTools = _dbContext.CleaningTools.Where(u => u.IsDisable == false).ToList();
                    foreach (var item in cleaningTools)
                    {
                        UserCleaningTool uct = new UserCleaningTool()
                        {
                            EmployeeId = user.Id,
                            CleaningToolId = item.Id
                        };
                        _dbContext.Add(uct);
                    }
                    _dbContext.SaveChanges();
                }
                else
                {
                    throw new Exception("Register failed");
                }

            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> RegisterCustomer(CustomerRegisterModel model)
        {
            var result = new ResultModel();
            try
            {
                var isExistUserName = _dbContext.Users.Any(u => u.UserName == model.UserName);
                if (isExistUserName)
                {
                    throw new Exception("UserName existed");
                }
                User user = _mapper.Map<CustomerRegisterModel, User>(model);
                var createUser = await _userManager.CreateAsync(user, model.Password);
                if (createUser.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, ConstRole.CUSTOMER);
                    _dbContext.SaveChanges();

                    result.Data = user.Id;
                    result.Succeed = true;

                }
                else
                {
                    throw new Exception("Register failed");
                }

            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> UpdatePassword(UserEditPasswordViewModel model, string userId)
        {
            var result = new ResultModel();
            try
            {
                User user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

                user.DateUpdated = DateTime.UtcNow.AddHours(7);
                
                var userEditPassword = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                if (userEditPassword.Succeeded)
                {
                    result.Data = user.Id;
                    result.Succeed = true;
                }
                else
                {
                    throw new Exception("Incorrect password");
                }
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        #region Forgot Password
        public async Task<ResultModel> UpdatePasswordForgot(UserEditPasswordForgotViewModel model)
        {
            var result = new ResultModel();
            try
            {
                var appUser = _userManager.Users.SingleOrDefault(r => r.UserName == model.Username);
                await _userManager.RemovePasswordAsync(appUser);
                await _userManager.AddPasswordAsync(appUser, model.NewPassword);

                result.Data = true;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel ForgotPassword(string username)
        {
            var result = new ResultModel();
            try
            {
                var user = _dbContext.Users.FirstOrDefault(a => a.UserName == username && a.IsDisable == false);

                if (user == null)
                {
                    throw new Exception("INvalid Id");
                }

                ActiveCodeModel acModel = new ActiveCodeModel()
                {
                    UserId = user.Id,
                    Email = user.Email
                };
                GetActiveCode(acModel);

                result.Data = "Đã gửi mã xác nhận đến Email mà bạn đã đăng ký.";
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel ConfirmForgotPasswordByCode(UsernameConfirmModel model)
        {
            var result = new ResultModel();
            try
            {
                var account = _dbContext.Users.FirstOrDefault(a => a.UserName == model.Username && a.IsDisable == false);
                if (account != null)
                {
                    DateTime today = DateTime.UtcNow.AddHours(7);
                    DateTime codeCreateTime = account.CodeCreateTime.Value + Expired_Time;
                    if (DateTime.Compare(codeCreateTime, today) >= 0)
                    {
                        if (account.ActiveCode == model.Code)
                        {
                            result.Data = ConstActiveCode.SUCCESS;
                        }
                        else
                        {
                            result.Data = ConstActiveCode.WRONG_CODE;
                        }
                    }
                    else
                    {
                        result.Data = ConstActiveCode.EXPIRED;
                    }
                    result.Succeed = true;

                }
                else
                {
                    throw new Exception("Invalid Account");
                }
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }
        #endregion

        #region Valid Account
        public ResultModel CheckEmailToSendActiveCode(ActiveCodeModel model)
        {
            var result = new ResultModel();
            try
            {
                var isUsedEmail = _dbContext.Users.Any(a => a.Email == model.Email && a.EmailConfirmed == true);

                EmailValidModel evModel = new EmailValidModel();
                if (isUsedEmail)
                {
                    evModel.Message = "Email này đã được sử dụng, vui lòng chọn email khác";
                    evModel.EmailExisted = true;
                }
                else
                {
                    GetActiveCode(model);

                    evModel.Message = "Đã gửi mã xác nhận, vui lòng kiểm tra Email.";
                    evModel.EmailExisted = false;
                }

                result.Data = evModel;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetActiveCode(ActiveCodeModel model)
        {
            var result = new ResultModel();
            try
            {
                string randCode = _utils.GenerateRandomCode(CODE_LENGTH);

                User account = _dbContext.Users.FirstOrDefault(a => a.Id == model.UserId);
                if (account != null)
                {
                    account.Email = model.Email;
                    account.DateUpdated = DateTime.UtcNow.AddHours(7);
                    account.ActiveCode = randCode;
                    account.CodeCreateTime = DateTime.UtcNow.AddHours(7);
                    _dbContext.Users.Update(account);
                    _dbContext.SaveChanges();

                    string subject = "Be Clean - Mã xác nhận Tài Khoản";

                    _sendMailService.SendHTMLMail(model.Email, subject, randCode);

                    result.Data = "OK";
                    result.Succeed = true;
                }
                else
                {
                    throw new Exception("Invalid Id");
                }

            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel ConfirmEmail(EmailConfirmModel model)
        {
            var result = new ResultModel();
            try
            {
                var account = _dbContext.Users.FirstOrDefault(a => a.Id == model.UserId && a.Email == model.Email && a.IsDisable == false);
                if (account != null)
                {
                    DateTime today = DateTime.UtcNow.AddHours(7);
                    DateTime codeCreateTime = account.CodeCreateTime.Value + Expired_Time;
                    if (DateTime.Compare(codeCreateTime, today) >= 0)
                    {
                        if (account.ActiveCode == model.Code)
                        {
                            account.EmailConfirmed = true;

                            _dbContext.Update(account);
                            _dbContext.SaveChanges();

                            result.Data = ConstActiveCode.SUCCESS;
                        }
                        else
                        {
                            result.Data = ConstActiveCode.WRONG_CODE;
                        }
                    }
                    else
                    {
                        result.Data = ConstActiveCode.EXPIRED;
                    }
                    result.Succeed = true;
                }
                else
                {
                    throw new Exception("Invalid Account");
                }
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        //public ResultModel ForgotPassword()
        //{

        //}
        //public async Task<ResultModel> ChangePasswordByCode(string email, string password, string code)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        if (string.IsNullOrEmpty(code))
        //        {
        //            throw new Exception("Invalid code");
        //        }
        //        User account = _dbContext.Users.FirstOrDefault(u => u.Email == email && u.ActiveCode == code);
        //        if (account != null)
        //        {
        //            DateTime codeDate = account.CodeCreateTime.Value;
        //            codeDate += Expired_Time;

        //            if (DateTime.Compare(codeDate, DateTime.UtcNow) >= 0)
        //            {
        //                var appUser = _userManager.Users.SingleOrDefault(r => r.Email == email);
        //                await _userManager.RemovePasswordAsync(appUser);
        //                await _userManager.AddPasswordAsync(appUser, password);

        //                account.ActiveCode = "";
        //                _dbContext.Update(account);
        //                _dbContext.SaveChanges();
        //            }
        //            result.Succeed = true;
        //            result.Data = "Success";
        //        }
        //        else
        //        {
        //            result.ErrorMessage = "Invalid Active Code";
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        //    }
        //    return result;
        //}

        #endregion

        public ResultModel Get(string id)
        {
            var result = new ResultModel();
            try
            {
                var users = _dbContext.Users.Where(u => id == null || u.Id == id).ToList();

                result.Data = _mapper.Map<List<User>, List<UserViewModel>>(users);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetUserEnable(string id)
        {
            var result = new ResultModel();
            try
            {
                var users = _dbContext.Users.Include(u => u.Province)
                                            .Include(u => u.District)
                                            .Include(u => u.Ward)
                                            .Where(u => id == null || (u.IsDisable == false && u.Id == id)).ToList();

                result.Data = _mapper.Map<List<User>, List<UserViewModel>>(users);
                result.Succeed = true;
                if(id != null)
                {
                    int countUnSeenNoti = (int)_notificationService.CountUnSeen(id).Data;
                    _notificationHub.NewNotificationCount(countUnSeenNoti, id);
                }
                
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public async Task<ResultModel> GetUserByRole(string userName, string roleId, int pageIndex, int pageSize)
        {
            var result = new ResultModel();
            try
            {
                var users = (await _userManager.GetUsersInRoleAsync(roleId)).ToList();

                List<UserViewModel> resultData = _mapper.Map<List<User>, List<UserViewModel>>(users).OrderByDescending(u => u.DateCreated).ToList();
                if(userName != null && userName != "")
                {
                    resultData = resultData.Where(u => u.UserName.ToUpper().Contains(userName.ToUpper())).ToList();
                }

                result.Data = new PagingModel
                {
                    Total = resultData.Count,
                    Data = resultData.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList()
                };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel GetBalance(string id)
        {
            var result = new ResultModel();
            try
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    throw new Exception("Invalid Id");
                }

                result.Data = user.Balance;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel UpdateDetail(string id, UserUpdateModel model)
        {
            var result = new ResultModel();
            try
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    throw new Exception("Invalid Id");
                }

                //user = _mapper.Map<UserUpdateModel, User>(model);
                user.Fullname = model.Fullname;
                user.Gender = model.Gender;
                user.Birthday = model.Birthday;
                user.ProvinceId = model.ProvinceId;
                user.DistrictId = model.DistrictId;
                user.WardId = model.WardId;
                user.Address = model.Address;
                user.AddressDetail = model.AddressDetail;
                user.PhoneNumber = model.PhoneNumber;
                user.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(user);
                _dbContext.SaveChanges();

                result.Data = user.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel UpdateUserStatus(string id, bool isDisable)
        {
            var result = new ResultModel();
            try
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);

                user.IsDisable = isDisable;
                user.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(user);
                _dbContext.SaveChanges();

                result.Data = user.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        public ResultModel UpdateBalance(string id, double money)
        {
            var result = new ResultModel();
            try
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    throw new Exception("Invalid Id");
                }

                user.Balance += money;
                user.DateUpdated = DateTime.UtcNow.AddHours(7);

                _dbContext.Update(user);
                _dbContext.SaveChanges();

                result.Data = user.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        #region Process Avatar
        public FileModel GetAvatar(string userId)
        {
            var result = new FileModel();
            var user = _dbContext.Users.FirstOrDefault(e => e.Id == userId);

            if (user == null)
            {
                throw new Exception("Invalid Id");
            }

            //result.Id = user.Id;
            result.Data = user.Avatar;
            result.FileType = "image/jpeg";

            return result;
        }

        public async Task<ResultModel> AddAvatar(IFormFile file, string userId)
        {
            var result = new ResultModel();
            try
            {
                await HandleFile(file, userId);

                await _dbContext.SaveChangesAsync();

                result.Data = userId;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
            return result;
        }

        private async Task HandleFile(IFormFile file, string userId)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            // Upload the file if less than x MB
            if (memoryStream.Length < MaxFileSize)
            {
                //string fileType = "image/jpeg";
                //if (file.ContentType != null && !string.IsNullOrEmpty(file.ContentType))
                //{
                //    fileType = file.ContentType;
                //}
                var user = _dbContext.Users.FirstOrDefault(_u => _u.Id == userId);
                user.Avatar = memoryStream.ToArray();
                _dbContext.Users.Update(user);
            }
            else
            {
                throw new Exception("File size too large. File name: " + file.FileName);
            }
        }
        #endregion
    }
}
