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
        #region Test001 Method 
        /// <summary>
        /// 获取所有产品
        /// </summary>
        /// <param name="productType"></param>
        /// <returns></returns>
        [Hystrix(FallBackMethod = nameof(GetAllProductsFallBackAsync),
            IsEnableCircuitBreaker = true,
            ExceptionsAllowedBeforeBreaking = 3,
            MillisecondsOfBreak = 1000 * 5)]
        public virtual async Task<string> GetAllProductsAsync(string productType)
        {
            Console.WriteLine($"-->>Starting get product type : {productType}");
            string str = await Task.Run(() => $"Get All Product Success");
            str.ToString();
            //throw new ArgumentException();
            // to do : using HttpClient to call outer service to get product list

            return $"OK {str}";
        }
        #endregion


        /// <summary>
        /// 获取所有产品
        /// </summary>
        /// <param name="productType"></param>
        /// <returns></returns>
        [HystrixCommand(IsEnableCircuitBreaker = true,
            ExceptionsAllowedBeforeBreaking = 3,
            MillisecondsOfBreak = 1000 * 5, FallBackMethod=nameof(GetAllProductsFallBackAsync))]
        public virtual async Task<string> GetAllProductsAsync(string productType,int productNum)
        {
            
            Console.WriteLine($"-->>Starting get product type : {productType}");
            string str = await Task.Run(() => $"Get All Product Success");
            str.ToString();
            throw new ArgumentException();
            // to do : using HttpClient to call outer service to get product list


            return $"OK {str}";
        }

        public virtual async Task<string> GetAllProductsFallBackAsync(string productType, int productNum)
        {
            Console.WriteLine($"-->>FallBack : Starting get product type : {productType}");

            string str = await Task.Run(() => $"Get All Product Fall");
            str.ToString();
            return $"OK for FallBack  {str}";
        }
    }
}
