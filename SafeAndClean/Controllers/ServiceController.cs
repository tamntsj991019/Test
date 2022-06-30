using Data.ConstData;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using System;

namespace SafeAndClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServiceController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Get(Guid? id, int pageIndex = 1, int pageSize = 10)
        {
            var rs = _serviceService.Get(id, pageIndex, pageSize);

            if (rs.Succeed) return Ok(rs.Data);
            return BadRequest(rs.ErrorMessage);
        }

        [HttpGet("ServiceGroupId/{sgId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetByServiceGroupId(string name, Guid sgId, int pageIndex = 1, int pageSize = 10)
        {
            var rs = _serviceService.GetByServiceGroupId(name, sgId, pageIndex, pageSize);

            if (rs.Succeed) return Ok(rs.Data);
            return BadRequest(rs.ErrorMessage);
        }
        
        [HttpGet("ServiceCleaningTool")]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetServiceWithCleaningTool()
        {
            var rs = _serviceService.GetServiceWithCleaningTool();

            if (rs.Succeed) return Ok(rs.Data);
            return BadRequest(rs.ErrorMessage);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Add([FromBody] ServiceAddModel model)
        {
            var rs = _serviceService.Add(model);

            if (rs.Succeed) return Ok(rs.Data);
            return BadRequest(rs.ErrorMessage);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Update(Guid id, [FromBody] ServiceUpdateModel model)
        {
            var rs = _serviceService.Update(id, model);

            if (rs.Succeed) return Ok(rs.Data);
            return BadRequest(rs.ErrorMessage);
        }

        [HttpDelete("Enable/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Enable(Guid id)
        {
            var rs = _serviceService.UpdateServiceStatus(id, false);

            if (rs.Succeed) return Ok(rs.Data);
            return BadRequest(rs.ErrorMessage);
        }

        [HttpDelete("Disable/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Disable(Guid id)
        {
            var rs = _serviceService.UpdateServiceStatus(id, true);

            if (rs.Succeed) return Ok(rs.Data);
            return BadRequest(rs.ErrorMessage);
        }
    }
}
