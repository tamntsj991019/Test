using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SafeAndClean.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ATestsController : ControllerBase
    {
        public ATestsController()
        {
        }

        //[Authorize(Roles = "ABC")]
        [HttpGet()]
        public IActionResult Get()
        {
            var ipHostEntry = Dns.GetHostEntry(Request.HttpContext.GetServerVariable("REMOTE_ADDR"));
            var ipAddresses = ipHostEntry.AddressList;
            var lstDeviceIP = new List<string>();
            foreach (var ip in ipAddresses)
            {
                lstDeviceIP.Add(ip.ToString());
            }
            //var str = JsonConvert.SerializeObject(ipHostEntry);
            //string deviceName = ipHostEntry.HostName;
            //var lstIp = ipHostEntry.A
            return Ok(new TestModel()
            {
                DeviceName = ipHostEntry.ToString(),
                ListDeviceIP = lstDeviceIP
            });
        }

        public class TestModel
        {
            public string DeviceName { get; set; }
            public List<string> ListDeviceIP { get; set; }
        }
    }
}
