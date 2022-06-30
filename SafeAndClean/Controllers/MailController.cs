using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafeAndClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly ISendMailService _sendMailService;

        public MailController(ISendMailService sendMailService)
        {
            _sendMailService = sendMailService;
        }

        [HttpGet("Backgroud")]
        public IActionResult GetBacgroundImage()
        {
            try
            {
                var rs = _sendMailService.GetImageByName("mail_backgroud");
                return File(rs.Data, rs.FileType);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("TEST")]
        public void TEST()
        {
            try
            {
                _sendMailService.SendHTMLMail("shinniri991019@gmail.com", "Test send mail with HTML", "ABC123");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
