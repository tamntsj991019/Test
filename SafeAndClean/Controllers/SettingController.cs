using Data.ConstData;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SafeAndClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingController : ControllerBase
    {
        private readonly ISettingService _settingService;

        public SettingController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Get(Guid? id)
        {
            var result = _settingService.Get(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("UsingCompanyTool")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetUseCompanyTool()
        {
            var result = _settingService.GetByKey(ConstSetting.USE_COMPANY_TOOL);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("CleaningAll")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetCleaningAll()
        {
            var result = _settingService.GetCleaningAll();

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Create([FromBody] SettingCreateModel model)
        {
            var result = _settingService.Create(model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        public IActionResult Update(Guid id, [FromBody] SettingUpdateModel model)
        {
            var result = _settingService.Update(id, model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        //[HttpDelete("{id}")]
        //[Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.ADMIN)]
        //public IActionResult Delete(Guid id)
        //{
        //    var result = _settingService.Delete(id);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}
    }
}
