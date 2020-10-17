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
        private static Random rng = new Random();
        private static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        static void Main(string[] args)
        {
            PerformanceTest1.init();

            List<Group> groups = new List<Group>();
            groups.Add(PerformanceTest1._12a());
            groups.Add(PerformanceTest1._12b());
            groups.Add(PerformanceTest1._12v());
            groups.Add(PerformanceTest1._12g());
            groups.Add(PerformanceTest1._11а());
            groups.Add(PerformanceTest1._11b());

            //Shuffle(groups);
            //ScheduleGenerator sg = new ScheduleGenerator(groups, PerformanceTest1.teachers, PerformanceTest1.subjects);
            ScheduleGenerator2 sg = new ScheduleGenerator2(groups, PerformanceTest1.teachers, PerformanceTest1.subjects);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            string[,,] schedule = sg.generate();
            //sg.printSchedule(schedule);

            sw.Stop();
            Console.WriteLine($"Ellapsed total time = {sw.ElapsedMilliseconds}");

            while (true) ;
        }
    }
}
