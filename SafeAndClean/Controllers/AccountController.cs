using Data.ConstData;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SafeAndClean.Extensions;
using System.Security.Claims;

namespace SafeAndClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        #region GET
        [HttpGet("Default")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Get(string id)
        {
            var result = _accountService.Get(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("UserDetails")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetUserDetail(string id)
        {
            if (id == null || id == "")
            {
                id = User.GetId();
            }
            var result = _accountService.GetUserEnable(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("Customers")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public async Task<IActionResult> GetCustomers(string userName, int pageIndex = 1, int pageSize = 10)
        {
            var result = await _accountService.GetUserByRole(userName, ConstRole.CUSTOMER, pageIndex, pageSize);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("Employees")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public async Task<IActionResult> GetEmployees(string userName, int pageIndex = 1, int pageSize = 10)
        {
            var result = await _accountService.GetUserByRole(userName, ConstRole.EMPLOYEE, pageIndex, pageSize);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("Balance")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetBalance()
        {
            var result = _accountService.GetBalance(User.GetId());
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("Avatar/{id}")]
        public IActionResult GetAvatar(string id)
        {
            try
            {
                var rs = _accountService.GetAvatar(id);
                return File(rs.Data, rs.FileType);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region POST
        [HttpPost("Auth")]
        public async Task<IActionResult> Login([FromBody] UserLoginViewModel model)
        {
            var result = await _accountService.Login(model);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost("Customer")]
        public async Task<IActionResult> CustomerRegister([FromBody] CustomerRegisterModel model)
        {
            var result = await _accountService.RegisterCustomer(model);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost("Employee")]
        [Consumes("multipart/form-data")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public async Task<IActionResult> EmployeeRegister([FromForm] EmployeeRegisterModel model)
        {
            var result = await _accountService.RegisterEmployee(model);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost("ActiveCode")]
        public IActionResult GetActiveCode([FromBody] ActiveCodeModel model)
        {
            var result = _accountService.CheckEmailToSendActiveCode(model);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost("EmailConfirm")]
        public IActionResult ConfirmEmail([FromBody] EmailConfirmModel model)
        {
            var result = _accountService.ConfirmEmail(model);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost("ActiveCodeForgot")]
        public IActionResult GetActiveCodeForgot(string username)
        {
            var result = _accountService.ForgotPassword(username);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost("PaswordForgotConfirm")]
        public IActionResult ConfirmForgotPasswordByCode(UsernameConfirmModel model)
        {
            var result = _accountService.ConfirmForgotPasswordByCode(model);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion

        #region PUT
        [HttpPut("Detail")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult UpdateDetail(UserUpdateModel model)
        {
            var result = _accountService.UpdateDetail(User.GetId(), model);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Password")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> UpdatePassword(UserEditPasswordViewModel model)
        {
            var result = await _accountService.UpdatePassword(model, User.GetId());
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("PasswordForgot")]
        public async Task<IActionResult> UpdatePasswordForgot(UserEditPasswordForgotViewModel model)
        {
            var result = await _accountService.UpdatePasswordForgot(model);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Avatar")]
        [Consumes("multipart/form-data")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> UploadAvatar(string id, [FromForm] FileUploadModel model)
        {
            if (id == null || id == "") id = User.GetId();
            var result = await _accountService.AddAvatar(model.File, id);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Enable/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Enable(string id)
        {
            var result = _accountService.UpdateUserStatus(id, false);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion

        #region DELETE
        [HttpDelete("Disable/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Disable(string id)
        {
            var result = _accountService.UpdateUserStatus(id, true);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion
    }
}
