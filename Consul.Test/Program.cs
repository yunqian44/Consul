using Polly;
using System;

namespace Consul.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            //Case1();

            var s = new { aa = "张三", age = "历史" };
            var b = new { aa = "张三", age = "历史" };

            Console.WriteLine($"s和b相等吗?----{s.Equals(b)}");
            Console.WriteLine("Hello World!");

            Console.ReadKey();
        }

        public static int SumNum()
        {
            int i = int.Parse("111");
            //throw new Exception("dd");
            return i;
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
