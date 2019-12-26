using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.WebApi.ServiceA.AttributeExtension
{
    /// <summary>
    /// 这个Attribute就是使用时候的验证，把它添加到要缓存数据的方法中，即可完成缓存的操作。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class HystrixAttribute: Attribute
    {
        /// <summary>
        /// 最多重试几次：如果为0，则不重试
        /// </summary>
        public int MaxRetryTimes { get; set; }

        /// <summary>
        /// 重试间隔（单位：毫秒）：默认100ms
        /// </summary>
        public int RetryIntervalMilliseconds { get; set; } = 100;

        /// <summary>
        /// 是否启用熔断
        /// </summary>
        public bool IsEnableCircuitBreaker { get; set; } = false;

        /// <summary>
        /// 熔断前出现允许错误几次
        /// </summary>
        public int ExceptionsAllowedBeforeBreaking { get; set; } = 3;

        /// <summary>
        /// 熔断时间（单位：毫秒）：默认1000ms
        /// </summary>
        public int MillisecondsOfBreak { get; set; } = 1000;

        /// <summary>
        /// 执行超过多少毫秒则认为超时（0表示不检测超时）
        /// </summary>
        public int TimeOutMilliseconds { get; set; } = 0;

        /// <summary>
        /// 缓存时间（存活期，单位：毫秒）：默认为0，表示不缓存
        /// Key：类名+方法名+所有参数ToString
        /// </summary>
        public int CacheTTLMilliseconds { get; set; } = 0;

        /// <summary>
        /// 降级的方法名称
        /// </summary>
        public string FallBackMethod { get; set; }
    }
}
