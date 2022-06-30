using Data.ConstData;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeAndClean.Extensions;
using Services.Core;
using System;

namespace SafeAndClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Get(Guid? id)
        {
            var result = _notificationService.Get(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        
        [HttpGet("Test")]
        public IActionResult Test(string title, string description, int count)
        {
            var result = _notificationService.Test(title, description, count);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        
        [HttpGet("User")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetUserNotification()
        {
            var result = _notificationService.GetUserNotification(User.GetId(), User.GetRole());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Seen")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Seen(Guid id)
        {
            var result = _notificationService.Seen(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        
        [HttpPut("SeenAll")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult SeenAll()
        {
            var result = _notificationService.SeenAll(User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }


        //[HttpPost]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        //public IActionResult Create([FromBody] NotificationCreateModel model)
        //{
        //    var result = _notificationService.Create(model);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}

        //[HttpPut("{id}")]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        //public IActionResult Update(Guid id, [FromBody] NotificationUpdateModel model)
        //{
        //    var result = _notificationService.Update(id, model);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}

        //[HttpDelete("{id}")]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        //public IActionResult Delete(Guid id)
        //{
        //    var result = _notificationService.Delete(id);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}
    }
}
