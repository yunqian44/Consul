using Consul.WebApi.ServiceA.AOP;
using Consul.WebApi.ServiceA.AttributeExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.WebApi.ServiceA.Services
{
    public class ProductService
    {
        /// <summary>
        /// 获取所有产品
        /// </summary>
        /// <param name="productType"></param>
        /// <returns></returns>
        [Hystrix(FallBackMethod = nameof(GetAllProductsFallBackAsync),
            IsEnableCircuitBreaker = true,
            ExceptionsAllowedBeforeBreaking = 2,
            MillisecondsOfBreak = 1000 * 5)]
        public virtual async Task<string> GetAllProductsAsync(string productType)
        {
            Console.WriteLine($"-->>Starting get product type : {productType}");
            string str = await Task.Run(()=> $"Get All Product Success");
            str.ToString();
            throw new ArgumentException();
            // to do : using HttpClient to call outer service to get product list

            return $"OK {str}";
        }

        public virtual async Task<string> GetAllProductsFallBackAsync(string productType)
        {
            Console.WriteLine($"-->>FallBack : Starting get product type : {productType}");

            string str = await Task.Run(() => $"Get All Product Fall");
            str.ToString();
            return $"OK for FallBack  {str}";
        }
    }
}
