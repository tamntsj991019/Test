using Data.ConstData;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeAndClean.Extensions;
using Services.Core;
using System;
using System.Collections.Generic;

namespace SafeAndClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Get(Guid? id)
        {
            var result = _scheduleService.Get(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        
        [HttpGet("Employee")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPLOYEE)]
        public IActionResult GetEnableByUserId(Guid? id)
        {
            var result = _scheduleService.GetEnableByUserId(id, User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPLOYEE)]
        public IActionResult Create([FromBody] List<ScheduleCreateModel> models)
        {
            var result = _scheduleService.Create(models, User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPLOYEE)]
        public IActionResult Update(Guid id, [FromBody] ScheduleUpdateModel model)
        {
            var result = _scheduleService.Update(id, model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete("Disable/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPLOYEE)]
        public IActionResult Disable(Guid id)
        {
            var result = _scheduleService.UpdateStatus(id, true, User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
    }
}
