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
    public class HystrixAOP : IInterceptor
    {
        private readonly IMemoryCache _memoryCache;

        public HystrixAOP(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }


        //Intercept方法是拦截的关键所在，也是IInterceptor接口中的唯一定义
        public void Intercept(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            //对当前方法的特性验证
            //如果需要验证
            if (method.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(HystrixAttribute)) is HystrixAttribute qHystrixAttribute)
            {

                // Polly CircuitBreaker要求对于同一段代码要共享一个policy对象
                lock (this) // 线程安全考虑
                {

                    Policy.Handle<Exception>().FallbackAsync(async (ctx, t) =>
                   {
                       invocation.ReturnValue = "我失败了";
                   }, async (ex, t) => { });


                    // 设置 最大重试次数限制
                    if (qHystrixAttribute.MaxRetryTimes > 0)
                    {
                        Policy.WrapAsync(Policy.Handle<Exception>()
                           .WaitAndRetryAsync(qHystrixAttribute.MaxRetryTimes,
                           i => TimeSpan.FromMilliseconds(qHystrixAttribute.RetryIntervalMilliseconds)));
                    }

                    // 启用熔断保护（CircuitBreaker）
                    if (qHystrixAttribute.IsEnableCircuitBreaker)
                    {
                        Policy.WrapAsync(Policy.Handle<Exception>()
                            .CircuitBreakerAsync(qHystrixAttribute.ExceptionsAllowedBeforeBreaking,
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
                        Policy.WrapAsync(Policy.TimeoutAsync(() =>
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
                        invocation.Proceed();

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
                    invocation.Proceed();
                }
            }
            else
            {
                invocation.Proceed();//直接执行被拦截方法
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


        //public override async Task Invoke(AspectContext context, AspectDelegate next)
        //{
        //    //一个HystrixCommand中保持一个policy对象即可
        //    //其实主要是CircuitBreaker要求对于同一段代码要共享一个policy对象
        //    //根据反射原理，同一个方法的MethodInfo是同一个对象，但是对象上取出来的HystrixCommandAttribute
        //    //每次获取的都是不同的对象，因此以MethodInfo为Key保存到policies中，确保一个方法对应一个policy实例
        //    policies.TryGetValue(context.ServiceMethod, out AsyncPolicy policy);
        //    lock (policies)//因为Invoke可能是并发调用，因此要确保policies赋值的线程安全
        //    {
        //        if (policy == null)
        //        {
        //            policy = Policy.NoOpAsync();//创建一个空的Policy
        //            if (IsEnableCircuitBreaker)
        //            {
        //                policy = policy.WrapAsync(Policy.Handle<Exception>()
        //                    .CircuitBreakerAsync(ExceptionsAllowedBeforeBreaking, TimeSpan.FromMilliseconds(MillisecondsOfBreak),(ex, ts) => 
        //                    {
        //                        Console.WriteLine($"Service API OnBreak -- ts = {ts.Seconds}s, ex.message = {ex.Message}");
        //                    },()=> {
        //                        Console.WriteLine($"Service API OnRest");
        //                    }));
        //            }
        //            if (TimeOutMilliseconds > 0)
        //            {
        //                policy = policy.WrapAsync(Policy.TimeoutAsync(() => TimeSpan.FromMilliseconds(TimeOutMilliseconds), Polly.Timeout.TimeoutStrategy.Pessimistic));
        //            }
        //            if (MaxRetryTimes > 0)
        //            {
        //                policy = policy.WrapAsync(Policy.Handle<Exception>().WaitAndRetryAsync(MaxRetryTimes, i => TimeSpan.FromMilliseconds(RetryIntervalMilliseconds)));
        //            }
        //            AsyncFallbackPolicy policyFallBack = Policy
        //            .Handle<Exception>()
        //            .FallbackAsync(async (ctx, t) =>
        //            {
        //                AspectContext aspectContext = (AspectContext)ctx["aspectContext"];
        //                var fallBackMethod = context.ServiceMethod.DeclaringType.GetMethod(this.FallBackMethod);
        //                Object fallBackResult = fallBackMethod.Invoke(context.Implementation, context.Parameters);
        //                //不能如下这样，因为这是闭包相关，如果这样写第二次调用Invoke的时候context指向的
        //                //还是第一次的对象，所以要通过Polly的上下文来传递AspectContext
        //                //context.ReturnValue = fallBackResult;
        //                aspectContext.ReturnValue = fallBackResult;
        //            }, async (ex, t) => { });

        //            policy = policyFallBack.WrapAsync(policy);
        //            //放入
        //            policies.TryAdd(context.ServiceMethod, policy);
        //        }
        //    }

        //    //把本地调用的AspectContext传递给Polly，主要给FallbackAsync中使用，避免闭包的坑
        //    Context pollyCtx = new Context();
        //    pollyCtx["aspectContext"] = context;

        //    //Install-Package Microsoft.Extensions.Caching.Memory
        //    if (CacheTTLMilliseconds > 0)
        //    {
        //        //用类名+方法名+参数的下划线连接起来作为缓存key
        //        string cacheKey = "HystrixMethodCacheManager_Key_" + context.ServiceMethod.DeclaringType
        //                                                           + "." + context.ServiceMethod + string.Join("_", context.Parameters);
        //        //尝试去缓存中获取。如果找到了，则直接用缓存中的值做返回值
        //        if (memoryCache.TryGetValue(cacheKey, out var cacheValue))
        //        {
        //            context.ReturnValue = cacheValue;
        //        }
        //        else
        //        {
        //            //如果缓存中没有，则执行实际被拦截的方法
        //            await policy.ExecuteAsync(ctx => next(context), pollyCtx);
        //            //存入缓存中
        //            using (var cacheEntry = memoryCache.CreateEntry(cacheKey))
        //            {
        //                cacheEntry.Value = context.ReturnValue;
        //                cacheEntry.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMilliseconds(CacheTTLMilliseconds);
        //            }
        //        }
        //    }
        //    else//如果没有启用缓存，就直接执行业务方法
        //    {
        //        await policy.ExecuteAsync(ctx => next(context), pollyCtx);
        //    }
        //}
    }
}
