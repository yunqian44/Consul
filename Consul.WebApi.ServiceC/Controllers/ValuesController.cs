using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Consul.WebApi.ServiceC.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        [Route("TestServiceC")]
        public async Task<string> TestServiceC()
        {
            var str = await Task.Run(() => "Success:my name is testservicec");
            return str;
        }
    }
}