using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SchoolManager.Generation_utils;
using SchoolManager.School_Models;
namespace SchoolManager
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            IEnumerable<int> kur = new HashSet<int>() { 1, 2, 2, 3 };
            Console.WriteLine(kur.Count(x => true));

            kur = kur.ToList();
            Console.WriteLine(kur.Count(x => true));

            kur = kur.Where(x => true);
            Console.WriteLine(kur.Count(x => true));

            kur = new List<int>() { 1, 4};
            Console.WriteLine(kur.Count(x => true));
            */

            PerformanceTestPMGHaskovo.test("Programa-2019-2020-I-srok");
            //PerformanceTest1.test();
        }
    }
}