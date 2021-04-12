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
            List <string> filenames = new List<string>()
            {
                "Programa-2019-2020-I-srok",
                "Programa-2019-2020-II-srok",
                "Programa-2018-2019-I-srok",
                    "Programa-2018-2019-II-srok",
            };         

            PerformanceTestPMGHaskovo.test(filenames[0]);
            //PerformanceTest1.test();
        }
    }
}