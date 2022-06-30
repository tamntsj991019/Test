using Data.ConstData;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeAndClean.Extensions;
using Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafeAndClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestCleaningToolController : ControllerBase
    {
        private readonly IRequestCleaningToolService _requestCleaningToolService;

        public RequestCleaningToolController(IRequestCleaningToolService requestCleaningToolService)
        {
            _requestCleaningToolService = requestCleaningToolService;
        }

        [HttpGet("Pending")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult GetPendingRequest(string cleaningToolName, int pageIndex = 1, int pageSize = 10)
        {
            var result = _requestCleaningToolService.GetRequestByStatus(cleaningToolName, ConstRequestStatus.PENDING, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("Approved")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult GetApprovedRequest(string cleaningToolName, int pageIndex = 1, int pageSize = 10)
        {
            var result = _requestCleaningToolService.GetRequestByStatus(cleaningToolName, ConstRequestStatus.APPROVED, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("AdminHistory")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult GetAdminHistory(string cleaningToolName, int pageIndex, int pageSize)
        {
            var result = _requestCleaningToolService.GetAdminHistory(cleaningToolName, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("EmployeeProcessing")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPLOYEE)]
        public IActionResult GetEmployeeProcessing()
        {
            var result = _requestCleaningToolService.GetEmployeeProcessing(User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("EmployeeHistory")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPLOYEE)]
        public IActionResult GetEmployeeHistory()
        {
            var result = _requestCleaningToolService.GetEmployeeHistory(User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPLOYEE)]
        public async Task<IActionResult> Create([FromBody] List<RequestCleaningToolCreateModel> listModel)
        {
            var result = await _requestCleaningToolService.Create(listModel, User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Cancelled/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPLOYEE)]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var result = await _requestCleaningToolService.Cancel(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Approved/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Approve(Guid id)
        {
            var result = _requestCleaningToolService.UpdateStatus(id, ConstRequestStatus.APPROVED);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Rejected/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Reject(Guid id, string reason)
        {
            var result = _requestCleaningToolService.UpdateStatus(id, ConstRequestStatus.REJECTED, reason);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Provided/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Provide(Guid id)
        {
            var result = _requestCleaningToolService.UpdateStatus(id, ConstRequestStatus.PROVIDED);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

    }
}
