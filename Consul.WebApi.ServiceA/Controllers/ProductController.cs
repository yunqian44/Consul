using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul.WebApi.ServiceA.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Consul.WebApi.ServiceA.Controllers
{
    [Route("api/Product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService productService;

        
        public ProductController(ProductService _productService)
        {
            productService = _productService;
        }

        [HttpGet("{id}",Name ="Get")]
        public async Task<string> Get(int id)
        {
            var product = await productService.GetAllProductsAsync("B");

            return product;
        }
    }
}