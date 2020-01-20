using AspectCore.DynamicProxy;
using Consul.WebApi.ServiceA.Common;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Consul.WebApi.ServiceA.AOP
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HystrixCommandAttribute : AbstractInterceptorAttribute
    {
        #region 属性
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
        #endregion


        private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        private IAsyncPolicy policy;

        public HystrixCommandAttribute()
        {

        }

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            //await next(context);

            //一个HystrixCommand中保持一个policy对象即可
            //其实主要是CircuitBreaker要求对于同一段代码要共享一个policy对象
            //根据反射原理，同一个方法的MethodInfo是同一个对象，但是对象上取出来的HystrixCommandAttribute
            //每次获取的都是不同的对象，因此以MethodInfo为Key保存到policies中，确保一个方法对应一个policy实例
            //policies.TryGetValue(context.ServiceMethod, out Policy policy);
            lock (this)//因为Invoke可能是并发调用，因此要确保policies赋值的线程安全
            {
                if (policy == null)
                {
                    policy = Policy
                   .Handle<ArgumentException>()
                   .FallbackAsync(async (ctx, t) =>
                   {
                       AspectContext aspectContext = (AspectContext)ctx["aspectContext"];
                       var fallBackMethod = context.ServiceMethod.DeclaringType.GetMethod(this.FallBackMethod);
                       Object fallBackResult = fallBackMethod.Invoke(context.Implementation, context.Parameters);

                       //不能如下这样，因为这是闭包相关，如果这样写第二次调用Invoke的时候context指向的
                       //还是第一次的对象，所以要通过Polly的上下文来传递AspectContext
                       //context.ReturnValue = fallBackResult;
                       aspectContext.ReturnValue = fallBackResult;

                       Console.WriteLine("我也是醉了");
                    },async (ex, t) =>
                    {
                        //Console.WriteLine("哈哈 我终于进来了");
                        //context.ReturnValue = "哈哈哈";

                        Console.WriteLine("我TM也是醉了");
                    });
                    // 设置 最大重试次数限制
                    if (MaxRetryTimes > 0)
                    {
                        policy = policy.WrapAsync(Policy.Handle<ArgumentException>()
                           .WaitAndRetryAsync(MaxRetryTimes,
                           i => TimeSpan.FromMilliseconds(RetryIntervalMilliseconds)));
                    }

                    // 启用熔断保护（CircuitBreaker）
                    if (IsEnableCircuitBreaker)
                    {
                        policy = policy.WrapAsync(Policy.Handle<ArgumentException>()
                            .CircuitBreakerAsync(ExceptionsAllowedBeforeBreaking,
                            TimeSpan.FromMilliseconds(MillisecondsOfBreak), (ex, ts) =>
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
                    if (TimeOutMilliseconds > 0)
                    {
                        policy = policy.WrapAsync(Policy.TimeoutAsync(() =>
                            TimeSpan.FromMilliseconds(TimeOutMilliseconds),
                            Polly.Timeout.TimeoutStrategy.Pessimistic));
                    }
                    //policy = policyFallBack.WrapAsync(policy);
                    //放入
                    //policies.TryAdd(context.ServiceMethod, policy);
                };
            }

            //把本地调用的AspectContext传递给Polly，主要给FallbackAsync中使用，避免闭包的坑
            Context pollyCtx = new Context();
            pollyCtx["aspectContext"] = context;

            //Install-Package Microsoft.Extensions.Caching.Memory
            if (CacheTTLMilliseconds > 0)
            {
                //用类名+方法名+参数的下划线连接起来作为缓存key
                var cacheKey = CustomCacheKey(context);
                //尝试去缓存中获取。如果找到了，则直接用缓存中的值做返回值
                if (_memoryCache.TryGetValue(cacheKey, out var cacheValue))
                {
                    context.ReturnValue = cacheValue;
                }
                else
                {
                    //如果缓存中没有，则执行实际被拦截的方法
                    await policy.ExecuteAsync(ctx => next(context), pollyCtx);
                    //存入缓存中
                    using (var cacheEntry = _memoryCache.CreateEntry(cacheKey))
                    {
                        cacheEntry.Value = context.ReturnValue;
                        cacheEntry.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMilliseconds(CacheTTLMilliseconds);
                    }
                }
            }
            else//如果没有启用缓存，就直接执行业务方法
            {
                await policy.ExecuteAsync(ctx => next(context), pollyCtx);
            }

        }

        /// <summary>
        /// 自定义缓存的key
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        protected string CustomCacheKey(AspectContext context)
        {
            var typeName = context.GetType().Name;
            var methodName = context.ImplementationMethod.Name;
            var methodArguments = context.ImplementationMethod.GetParameters();//获取参数列表，最多三个

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
