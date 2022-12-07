using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TESTBase:" + TestBase.val);
            Console.WriteLine("TEST1:" + Test1.val);
            Console.WriteLine("TEST2:" + Test2.val);
            Test1.val = true;
            Console.WriteLine("TESTBase:" + TestBase.val);
            Console.WriteLine("TEST1:" + Test1.val);
            Console.WriteLine("TEST2:" + Test2.val);
            Console.ReadLine();
        }
    }

    public abstract class TestBase
    {
        public static bool val { get; set; } = false;

    }

    public class Test1 : TestBase
    {
        public Test1() { }
    }

    public class Test2 : TestBase
    {
        public Test2() { }
    }
}
