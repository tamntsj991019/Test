using Data.ConstData;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeAndClean.Extensions;
using Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafeAndClean.Controlleresult
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        #region GET

        #region Admin Booking
        [HttpGet("Default")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Get(Guid? id, int pageIndex = 1, int pageSize = 10)
        {
            var result = _bookingService.Get(id, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("BookingNew")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult GetNewBooking(Guid? id, int pageIndex = 1, int pageSize = 10)
        {
            var result = _bookingService.GetNewBooking(id, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("BookingFinished")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult GetFinishedBooking(Guid? id, int pageIndex = 1, int pageSize = 10)
        {
            var result = _bookingService.GetFinishedBooking(id, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("BookingInWorking")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult GetBookingInWorking(string empName, int pageIndex = 1, int pageSize = 10)
        {
            var result = _bookingService.GetBookingInWorking(empName, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("BookingDetailAdmin/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult GetBookingDetailAdmin(Guid id)
        {
            var result = _bookingService.GetBookingDetailAdmin(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("DetailWithRecomendEmployee/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public async Task<IActionResult> GetNewBookingWithRecomendEmpAsync(Guid id, double radius = 0)
        {
            var result = await _bookingService.GetNewBookingWithRecomendEmp(id, radius);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion

        [HttpGet("BookingEnable/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetBookingEnableById(Guid id)
        {
            var result = _bookingService.GetBookingEnableById(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        #region Employee Booking List
        //[HttpGet("CheckingExistedBookingProcessing")]
        //[Authorize(AuthenticationSchemes = "Bearer", Roles = ConstUserRoles.EMPLOYEE)]
        //public IActionResult IsExistedBookingProcessing()
        //{
        //    var result = _bookingService.IsExistedBookingProcessing(User.GetId());

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}

        [HttpGet("BookingEmployeeWaitingEnables")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPLOYEE)]
        public IActionResult GetBookingEmployeeWaitingEnables()
        {
            var result = _bookingService.GetBookingEmployeeWaitingEnables(User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("BookingEmployeeProcessingEnables")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPLOYEE)]
        public IActionResult GetBookingEmployeeProcessingEnables()
        {
            var result = _bookingService.GetBookingEmployeeProcessingEnables(User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("HistoryBookingEmployeeEnables")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPLOYEE)]
        public IActionResult GetHistoryBookingEmployeeEnables()
        {
            var result = _bookingService.GetHistoryBookingEmployeeEnables(User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion

        #region Customer Booking List
        [HttpGet("BookingCustomerWaitingEnables")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.CUSTOMER)]
        public IActionResult GetBookingCustomerWaitingEnables()
        {
            var result = _bookingService.GetBookingCustomerWaitingEnables(User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("BookingCustomerProcessingEnables")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.CUSTOMER)]
        public IActionResult GetBookingCustomerProcessingEnables()
        {
            var result = _bookingService.GetBookingCustomerProcessingEnables(User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("HistoryBookingCustomerEnables")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.CUSTOMER)]
        public IActionResult GetHistoryBookingCustomerEnables()
        {
            var result = _bookingService.GetHistoryBookingCustomerEnables(User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion

        [HttpGet("BookingRe-findEmployee/{bookingId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.CUSTOMER)]
        public async Task<IActionResult> ReFindSuitableEmployee(Guid bookingId)
        {
            var result = await _bookingService.ReFindSuitableEmployee(bookingId);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion

        #region POST
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.CUSTOMER)]
        public async Task<IActionResult> Create([FromBody] BookingCreateModel model, bool isRandomEmployee = true)
        {
            var result = await _bookingService.Create(model, User.GetId(), isRandomEmployee);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost("CheckingEmployeeFree")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.CUSTOMER)]
        public IActionResult CheckEmployeeSuitable(string employeeCode, [FromBody] BookingCreateModel model)
        {
            var result = _bookingService.CheckEmployeeSuitable(employeeCode, model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }



        //[HttpPost("Rebooking")]
        //[Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.CUSTOMER)]
        //public IActionResult Rebooking([FromBody] BookingEmployeeAgainCreateModel model)
        //{
        //    var result = _bookingService.RebookingEmployee(model, User.GetId());

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}
        #endregion

        #region PUT
        [HttpPut("AssigningEmployee/{bookingId}/{employeeId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public async Task<IActionResult> AssignWorkToEmployee(Guid bookingId, string employeeId)
        {
            var result = await _bookingService.AssignWorkToEmployeeAsync(bookingId, employeeId);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        
        [HttpPut("Services/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPCUS)]
        public async Task<IActionResult> CheckBeforeUpdateServiceDetails(Guid id, [FromBody] BookingUpdateServiceDetailsModel model)
        {
            var result = await _bookingService.CheckBeforeUpdateServiceDetails(id, model, User.GetRole());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Status/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPCUS)]
        public async Task<IActionResult> UpdateStatus(Guid id, string statusId, string bookingDescription)
        {
            var result = await _bookingService.UpdateBookingStatus(id, statusId.Trim(), bookingDescription);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion

        #region  DELETE
        //[HttpDelete("Disable/{id}")]
        //[Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.CUSTOMER)]
        //public IActionResult Delete(Guid id)
        //{
        //    var result = _bookingService.Delete(id);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}
        #endregion
    }
}
