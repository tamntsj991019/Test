using Data.ConstData;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeAndClean.Extensions;
using Services.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SafeAndClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingLogController : ControllerBase
    {
        private readonly IBookingLogService _bookingLogService;

        public BookingLogController(IBookingLogService bookingLogService)
        {
            _bookingLogService = bookingLogService;
        }
        #region GET
        [HttpGet("Default")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Get(Guid? id)
        {
            var result = _bookingLogService.Get(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        
        [HttpGet("{bookingId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Get(Guid bookingId)
        {
            var result = _bookingLogService.GetByBookingId(bookingId);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("BookingImages/{bookingId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetImagesByBookingId(Guid bookingId)
        {
            var result = _bookingLogService.GetImagesByBookingId(bookingId, User.GetRole());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("BookingImageReference/{fileId}")]
        public IActionResult GetBookingImageReferenceById(Guid fileId)
        {
            try
            {
                var rs = _bookingLogService.GetImageById(fileId, Path.Combine($"FileStored/BookingImageReferences"));
                return File(rs.Data, rs.FileType);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("BookingImageEvidence/{fileId}")]
        public IActionResult GetBookingImageEvidenceById(Guid fileId)
        {
            try
            {
                var rs = _bookingLogService.GetImageById(fileId, Path.Combine($"FileStored/BookingImageEvidences"));
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
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Create([FromBody] BookingLogCreateModel model)
        {
            var result = _bookingLogService.Create(model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion

        #region PUT
        [HttpPut("BookingImageReference/{bookingId}")]
        [Consumes("multipart/form-data")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> UploadBookingImageReference([FromForm] FileUploadListModel model, Guid bookingId)
        {
            var result = await _bookingLogService.AddImages(model.Files, bookingId, ConstBookingStatus.PROCESSING);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("BookingImageEvidence/{bookingId}")]
        [Consumes("multipart/form-data")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> UploadBookingImageEvidence([FromForm] FileUploadListModel model, Guid bookingId)
        {
            var result = await _bookingLogService.AddImages(model.Files, bookingId, ConstBookingStatus.CONFIRM_WAITING);
            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Update(Guid id, [FromBody] BookingLogUpdateModel model)
        {
            var result = _bookingLogService.Update(id, model);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        #endregion

        #region DELETE
        //[HttpDelete("Disable/{id}")]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        //public IActionResult Delete(Guid id)
        //{
        //    var result = _bookingLogService.Delete(id);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}
        #endregion
    }
}
