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
    public class BookingStatusController : ControllerBase
    {
        private readonly IBookingStatusService _bookingStatusService;

        public BookingStatusController(IBookingStatusService bookingStatusService)
        {
            _bookingStatusService = bookingStatusService;
        }

        [HttpGet]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Get(string id)
        {
            var result = _bookingStatusService.Get(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost]
        public IActionResult Create([FromBody] BookingStatusCreateModel model)
        {
            var result = _bookingStatusService.Create(model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] BookingStatusUpdateModel model)
        {
            var result = _bookingStatusService.Update(id, model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var result = _bookingStatusService.Delete(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
    }
}
