using Data.ConstData;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SafeAndClean.Extensions;
using System.Security.Claims;

namespace SafeAndClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService FeedbackService)
        {
            _feedbackService = FeedbackService;
        }

        //[HttpGet]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        //public IActionResult Get(Guid? id)
        //{
        //    var result = _feedbackService.Get(id);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}

        [HttpGet("Customer")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.CUSTOMER)]
        public IActionResult GetFeedbacksForCustomer()
        {
            var result = _feedbackService.GetFeedbacksForCustomer(User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        
        [HttpGet("Employee")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = ConstRole.EMPLOYEE)]
        public IActionResult GetFeedbacksForEmployee()
        {
            var result = _feedbackService.GetFeedbacksForEmployee(User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Create([FromBody] FeedbackCreateModel model)
        {
            var result = _feedbackService.Create(model, User.GetId(), User.GetRole());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        //[HttpPut("{id}")]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        //public IActionResult Update(Guid id, [FromBody] FeedbackUpdateModel model)
        //{
        //    var result = _feedbackService.Update(id, model);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}

        //[HttpDelete("{id}")]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        //public IActionResult Delete(Guid id)
        //{
        //    var result = _feedbackService.Delete(id);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}
    }
}
