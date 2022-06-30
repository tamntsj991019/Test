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
    public class CleaningToolController : ControllerBase
    {
        private readonly ICleaningToolService _cleaningToolService;

        public CleaningToolController(ICleaningToolService cleaningToolService)
        {
            _cleaningToolService = cleaningToolService;
        }

        #region GET
        [HttpGet("Default")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Get(Guid? id, int pageIndex = 1, int pageSize = 10)
        {
            var result = _cleaningToolService.Get(id, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("SearchingByName/{name}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult SearchByName(string name, int pageIndex = 1, int pageSize = 10)
        {
            var result = _cleaningToolService.SearchByName(name, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("CleaningToolEnable")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetEnable(Guid? id)
        {
            var result = _cleaningToolService.GetEnable(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("Image/{id}")]
        public IActionResult GetImage(Guid id)
        {
            try
            {
                var rs = _cleaningToolService.GetImage(id);
                return File(rs.Data, rs.FileType);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region POST
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public async Task<IActionResult> Create([FromForm] CleaningToolCreateModel model)
        {
            var result = await _cleaningToolService.Create(model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion

        #region PUT
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Update(Guid id, [FromBody] CleaningToolUpdateModel model)
        {
            var result = _cleaningToolService.Update(id, model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Adding/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Add(Guid id, int quantity)
        {
            var result = _cleaningToolService.AddMore(id, quantity);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Image/{id}")]
        [Consumes("multipart/form-data")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public async Task<IActionResult> UploadImage(Guid id, [FromForm] FileUploadModel model)
        {
            var result = await _cleaningToolService.AddImage(model.File, id);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Enable/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Enable(Guid id)
        {
            var result = _cleaningToolService.UpdateCleaningToolStatus(id, false);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion

        #region DELETE
        [HttpDelete("Disable/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Disable(Guid id)
        {
            var result = _cleaningToolService.UpdateCleaningToolStatus(id, true);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion
    }
}
