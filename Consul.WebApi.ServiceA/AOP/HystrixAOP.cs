using Castle.DynamicProxy;
using Consul.WebApi.ServiceA.AttributeExtension;
using Consul.WebApi.ServiceA.Common;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Fallback;
using Polly.NoOp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Consul.WebApi.ServiceA.AOP
{
    /// <summary>
    /// 拦截器
    /// </summary>
    public class HystrixAOP : IInterceptor
    {
        private readonly IMemoryCache _memoryCache;

        public HystrixAOP(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        private ISyncPolicy policy;


        //Intercept方法是拦截的关键所在，也是IInterceptor接口中的唯一定义
        public void Intercept(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            //对当前方法的特性验证

            //需要验证
            if (method.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(HystrixAttribute)) is HystrixAttribute qHystrixAttribute)
            {
                // Polly CircuitBreaker要求对于同一段代码要共享一个policy对象
                lock (this) // 线程安全考虑
                {
                    if (policy == null)
                    {
                        policy = Policy
                          .Handle<ArgumentException>()
                          .Fallback(async (ctx, t) =>
                       {

                            //var fallBackMethod = .ServiceMethod.DeclaringType.GetMethod(qHystrixAttribute.FallBackMethod);
                            //var fallBackResult = fallBackMethod.Invoke(context.Implementation, context.Parameters);
                           // Console.WriteLine("哈哈 我终于进来了");
                           //invocation.ReturnValue = Task.Run(() => "哈哈哈");
                           //return;
                       }, async (ex, t) => {
                           Console.WriteLine("哈哈 我终于进来了");
                           invocation.ReturnValue = Task.Run(() => "哈哈哈");
                           return;
                       });
                    }


                    // 设置 最大重试次数限制
                    if (qHystrixAttribute.MaxRetryTimes > 0)
                    {
                        policy = policy.Wrap(Policy.Handle<ArgumentException>()
                           .WaitAndRetry(qHystrixAttribute.MaxRetryTimes,
                           i => TimeSpan.FromMilliseconds(qHystrixAttribute.RetryIntervalMilliseconds)));
                    }

                    // 启用熔断保护（CircuitBreaker）
                    if (qHystrixAttribute.IsEnableCircuitBreaker)
                    {
                        policy = policy.Wrap(Policy.Handle<ArgumentException>()
                            .CircuitBreaker(qHystrixAttribute.ExceptionsAllowedBeforeBreaking,
                            TimeSpan.FromMilliseconds(qHystrixAttribute.MillisecondsOfBreak), (ex, ts) =>
                            {
                            // assuem to do logging
                            Console.WriteLine($"Service API OnBreak -- ts = {ts.Seconds}s, ex.message = {ex.Message}");
                            }, () =>
                            {
                            // assume to do logging
                            Console.WriteLine($"Service API OnReset");
                            }));
                    }

                    // 设置超时时间
                    if (qHystrixAttribute.TimeOutMilliseconds > 0)
                    {
                        policy = policy.Wrap(Policy.Timeout(() =>
                            TimeSpan.FromMilliseconds(qHystrixAttribute.TimeOutMilliseconds),
                            Polly.Timeout.TimeoutStrategy.Pessimistic));
                    }
                }


                // 设置缓存时间
                if (qHystrixAttribute.CacheTTLMilliseconds > 0)
                {
                    var cacheKey = CustomCacheKey(invocation);

                    if (_memoryCache.TryGetValue(cacheKey, out var cacheValue))
                    {
                        // 如果缓存中有，直接用缓存的值
                        invocation.ReturnValue = cacheValue;
                        return;
                    }
                    else
                    {
                        // 如果缓存中没有，则执行实际被拦截的方法
                        policy.Execute(() =>
                        {
                            throw new ArgumentException();
                            invocation.Proceed();
                        });

                        // 执行完被拦截方法后存入缓存中以便后面快速复用
                        using (var cacheEntry = _memoryCache.CreateEntry(cacheKey))
                        {
                            cacheEntry.Value = invocation.ReturnValue;
                            cacheEntry.AbsoluteExpiration = DateTime.Now
                                + TimeSpan.FromMilliseconds(qHystrixAttribute.CacheTTLMilliseconds); // 设置缓存过期时间
                        }
                    }
                }
                else
                {
                    // 如果没有启用缓存，则直接执行业务方法
                    policy.Execute(() =>
                    {
                        //测试熔断机制
                        //throw new ArgumentException();
                        invocation.Proceed();
                    });
                }
            }
            else
            {
                // 将策略应用到 invocation.Proceed 方法上
                policy.Execute(() =>
                {
                    //测试熔断机制
                    //throw new ArgumentException();
                    invocation.Proceed();
                });
            }
        }

        /// <summary>
        /// 自定义缓存的key
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        protected string CustomCacheKey(IInvocation invocation)
        {
            var typeName = invocation.TargetType.Name;
            var methodName = invocation.Method.Name;
            var methodArguments = invocation.Arguments.Select(GetArgumentValue).Take(3).ToList();//获取参数列表，最多三个

            string key = $"{typeName}:{methodName}:";
            foreach (var param in methodArguments)
            {
                key = $"{key}{param}:";
            }

            return key.TrimEnd(':');
        }

        /// <summary>
        /// object 转 string
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected static string GetArgumentValue(object arg)
        {
            if (arg is DateTime || arg is DateTime?)
                return ((DateTime)arg).ToString("yyyyMMddHHmmss");

            if (arg is string || arg is ValueType || arg is Nullable)
                return arg.ToString();

            if (arg != null)
            {
                if (arg.GetType().IsClass)
                {
                    return MD5Helper.MD5Encrypt16(Newtonsoft.Json.JsonConvert.SerializeObject(arg));
                }
            }
            return string.Empty;
        }
    }
}
