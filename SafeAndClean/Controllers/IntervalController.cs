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
    public class IntervalController : ControllerBase
    {
        private readonly IIntervalService _IntervalService;

        public IntervalController(IIntervalService IntervalService)
        {
            _IntervalService = IntervalService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Get(Guid? id)
        {
            var result = _IntervalService.Get(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost()]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Create(IntervalCreateModel model)
        {
            var result = _IntervalService.Create(model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Update(Guid id, [FromBody] IntervalUpdateModel model)
        {
            var result = _IntervalService.Update(id, model, User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete("Disable/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Disable(Guid id)
        {
            var result = _IntervalService.UpdateStatus(id, true, User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
    }
}
