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
    public class ServiceBookingController : ControllerBase
    {
        private readonly IServiceBookingService _serviceBookingService;

        public ServiceBookingController(IServiceBookingService serviceBookingService)
        {
            _serviceBookingService = serviceBookingService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Get(Guid? bookingId, Guid? serviceId)
        {
            var result = _serviceBookingService.Get(bookingId, serviceId);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        //[HttpPost]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        //public IActionResult Create([FromBody] ServiceBookingCreateModel model)
        //{
        //    var result = _serviceBookingService.Create(model);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}

        //[HttpPut("{bookingId}/{serviceId}")]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        //public IActionResult Update(Guid bookingId, Guid serviceId, [FromBody] ServiceBookingUpdateModel model)
        //{
        //    var result = _serviceBookingService.Update(bookingId, serviceId, model);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}

        //[HttpDelete("{bookingId}/{serviceId}")]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        //public IActionResult Delete(Guid bookingId, Guid serviceId)
        //{
        //    var result = _serviceBookingService.Delete(bookingId, serviceId);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}
    }
}
