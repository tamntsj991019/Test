using Data.ConstData;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using System;
using System.Threading.Tasks;

namespace SafeAndClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceGroupController : ControllerBase
    {
        private readonly IServiceGroupService _serviceGroupService;

        public ServiceGroupController(IServiceGroupService serviceGroupService)
        {
            _serviceGroupService = serviceGroupService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Get(Guid? id, int pageIndex = 1, int pageSize = 10)
        {
            var result = _serviceGroupService.Get(id, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("SearchingByName/{name}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult SearchByName(string name, int pageIndex = 1, int pageSize = 10)
        {
            var result = _serviceGroupService.SearchByName(name, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("ServiceGroupEnable")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetEnable(Guid? id)
        {
            var result = _serviceGroupService.GetEnable(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("ServiceGroupNormalEnable")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetNormalWithServiceEnable(Guid? id)
        {
            var result = _serviceGroupService.GetNormalWithServiceEnable(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public async Task<IActionResult> Create([FromForm] ServiceGroupCreateModel model)
        {
            var result = await _serviceGroupService.Create(model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Update(Guid id, [FromBody] ServiceGroupUpdateModel model)
        {
            var result = _serviceGroupService.Update(id, model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete("Enable/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Enable(Guid id)
        {
            var result = _serviceGroupService.UpdateServiceGroupStatus(id, false);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete("Disable/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Disable(Guid id)
        {
            var result = _serviceGroupService.UpdateServiceGroupStatus(id, true);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("Image/{id}")]
        public IActionResult GetImage(Guid id)
        {
            try
            {
                var rs = _serviceGroupService.GetImage(id);
                return File(rs.Data, rs.FileType);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("Image/{id}")]
        [Consumes("multipart/form-data")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public async Task<IActionResult> UploadImage(Guid id, [FromForm] FileUploadModel model)
        {
            var result = await _serviceGroupService.AddImage(model.File, id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
    }
}
