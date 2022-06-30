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
    public class SubPackageController : ControllerBase
    {
        private readonly ISubPackService _subPackService;

        public SubPackageController(ISubPackService subPackService)
        {
            _subPackService = subPackService;
        }

        [HttpGet("Province")]
        public IActionResult GetProvince()
        {
            var result = _subPackService.GetProvince();
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("WardAndDistric/{parentId}")]
        public IActionResult GetSupplierCertification(string parentId)
        {
            var result = _subPackService.GetWardAndDistric(parentId);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        //[Authorize(AuthenticationSchemes = "Bearer")]
    }


}
