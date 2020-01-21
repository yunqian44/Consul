using Polly;
using System;

namespace Consul.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            //Case1();

            #region Test001 compare two object
            //var s = new { aa = "张三", age = "历史" };
            //var b = new { aa = "张三", age = "历史" };

            //Console.WriteLine($"s和b相等吗?----{s.Equals(b)}");
            //Console.WriteLine("Hello World!"); 
            #endregion

            DateTimeRange timeRange = new DateTimeRange(DateTime.Now.AddHours(-1),DateTime.Now);

            double hours = timeRange;

            double hours2 = (DateTimeRange)timeRange;

            Console.ReadKey();
        }

        public static int SumNum()
        {
            int i = int.Parse("111");
            //throw new Exception("dd");
            return i;
        }

        public class DateTimeRange
        {
            public DateTime StartTime { get; set; }

            public DateTime EndTime { get; set; }

            public DateTimeRange(DateTime startTime, DateTime endTime)
            {
                StartTime = startTime;
                EndTime = endTime;  
            }

            //operator 后面跟需要转换的类型
            public static implicit operator double(DateTimeRange timeRange)
            {
                return (timeRange.EndTime - timeRange.StartTime).TotalHours;
            }

        }

        public static void Case1()
        {
            ISyncPolicy policy = Policy.Handle<ArgumentException>()
                .Fallback(() =>
                {
                    Console.WriteLine("Error occured");
                });

            policy.Execute(() =>
            {
                //try
                //{
                    Console.WriteLine("Job Start");
                    throw new ArgumentException("sdada");
                    var ss = SumNum();


                    Console.WriteLine($"Result {ss}");
                //}
                //catch (AggregateException ex)
                //{
                //    Console.WriteLine("我是错误的方法");
                //}
                //finally
                //{
                //    Console.WriteLine("Job End");
                //}
                //throw new ArgumentException("Hello Polly!");
            });
        }
    }
}
