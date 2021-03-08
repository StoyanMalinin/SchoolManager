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
            PerformanceTest1.init();
            List<Group> groups = new List<Group>();
            
            groups.Add(PerformanceTest1._12a());
            groups.Add(PerformanceTest1._12b());
            groups.Add(PerformanceTest1._12v());
            groups.Add(PerformanceTest1._12g());
            groups.Add(PerformanceTest1._11а());
            groups.Add(PerformanceTest1._11b());
            groups.Add(PerformanceTest1._11v());
            groups.Add(PerformanceTest1._11d());
            groups.Add(PerformanceTest1._10a());

            List<Multilesson>[] multilessons = new List<Multilesson>[5 + 1];
            for (int day = 1; day <= 5; day++)
                multilessons[day] = new List<Multilesson>();
            //multilessons[5].Add(new Multilesson(groups[0], PerformanceTest1.teachers[4], PerformanceTest1.subjects[4], new IntInInterval(2, 2)));
            //multilessons[5].Add(new Multilesson(groups[7], PerformanceTest1.teachers[4], PerformanceTest1.subjects[4], new IntInInterval(2, 2)));
            //multilessons[5].Add(new Multilesson(groups[0], PerformanceTest1.teachers[0], PerformanceTest1.subjects[0], new IntInInterval(1, 2)));
            //multilessons[5].Add(new Multilesson(groups[0], PerformanceTest1.teachers[2], PerformanceTest1.subjects[3], new IntInInterval(1, 1)));
            //multilessons[3].Add(new Multilesson(groups[2], PerformanceTest1.teachers[5], PerformanceTest1.subjects[0], new IntInInterval(2, 3)));

            ScheduleGenerator4 sg = new ScheduleGenerator4(groups, PerformanceTest1.teachers, PerformanceTest1.subjects, PerformanceTest1.higharchy, multilessons);//за общи проблеми
            
            Stopwatch sw = new Stopwatch();
            sw.Start();
            sg.gen();
            sw.Stop();
            
            Console.WriteLine($"Ellapsed total time = {sw.ElapsedMilliseconds}");
            while (true) ;
        }
    }
}