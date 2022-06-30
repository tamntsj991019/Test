using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeAndClean.Extensions;
using Services.Core;

namespace SafeAndClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet("{bookingId}")]
        public IActionResult GetMessage(Guid bookingId, int? pageIndex = 1, int? pageSize = 10)
        {
            var rs = _messageService.GetMessage(bookingId, pageIndex.Value, pageSize.Value, User.GetId());
            if (rs.Succeed) return Ok(rs.Data);
            return BadRequest(rs.ErrorMessage);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendMessage([FromForm] MessageCreateModel model)
        {
            var rs = await _messageService.SendMessage(model, User.GetId());
            if (rs.Succeed) return Ok(rs.Data);
            return BadRequest(rs.ErrorMessage);
        }

        [HttpGet("File/{messageId}")]
        public IActionResult GetMessageFile(Guid messageId)
        {
            try
            {
                var rs = _messageService.GetMessageFile(messageId);
                return File(rs.Data, rs.FileType, rs.FileName);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("Seen/{conversationId}")]
        public IActionResult SeenMessage(Guid conversationId)
        {
            var rs = _messageService.SeenMessage(conversationId, User.GetId());
            if (rs.Succeed) return Ok(rs.Data);
            return BadRequest(rs.ErrorMessage);
        }
    }
}
