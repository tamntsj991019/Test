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
    public class BookingImageController : ControllerBase
    {
        private readonly IBookingImageService _bookingImageService;

        public BookingImageController(IBookingImageService bookingImageService)
        {
            _bookingImageService = bookingImageService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Get(Guid? id)
        {
            var result = _bookingImageService.Get(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Create([FromBody] BookingImageCreateModel model)
        {
            var result = _bookingImageService.Create(model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Update(Guid id, [FromBody] BookingImageUpdateModel model)
        {
            var result = _bookingImageService.Update(id, model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Delete(Guid id)
        {
            var result = _bookingImageService.Delete(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
    }
}
