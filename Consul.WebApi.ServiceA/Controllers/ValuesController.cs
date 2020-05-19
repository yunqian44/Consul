using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Consul.WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static int _count = 0;

        [HttpGet]
        public string Test()
        {
            _count++;
            Console.WriteLine($"Get...{_count}");
            //if (_count <= 3)
            //{
            //    System.Threading.Thread.Sleep(5000);
            //}
            return "请求 ServiceA-1 成功";
        }
    }
}