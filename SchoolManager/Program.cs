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
            List<LimitationGroup> limitationGroups = new List<LimitationGroup>()
            {
                new LimitationGroup("to4ni"),
                new LimitationGroup("razkazvatelni"),
                new LimitationGroup("matematika"),
                new LimitationGroup("istoriq"),
                new LimitationGroup("geografiq"),
                new LimitationGroup("balgarski"),
                new LimitationGroup("angliiski"),
            };

            List<Subject> subjects = new List<Subject>()
            {
                new Subject("matematika", new List<LimitationGroup>(){ limitationGroups[0]}),
                new Subject("istoriq", new List<LimitationGroup>(){ limitationGroups[1]}),
                new Subject("geografiq", new List<LimitationGroup>(){ limitationGroups[1]}),
                new Subject("balgarski", new List<LimitationGroup>(){ limitationGroups[5]}),
                new Subject("angliiski", new List<LimitationGroup>(){ limitationGroups[6]}),
            };

            List<Teacher> teachers = new List<Teacher>()
            {
                new Teacher("batimkata", new List<Subject>(){ subjects[0] }),
                new Teacher("kireto", new List<Subject>(){ subjects[1], subjects[2] }),
                new Teacher("valcheto", new List<Subject>(){ subjects[1], subjects[2] }),
                new Teacher("slav4eto", new List<Subject>(){ subjects[1], subjects[2] }),
                new Teacher("gabarcheto", new List<Subject>(){  }),
            };

            var dayLims = new List<Tuple<LimitationGroup, int>>() 
            { 
                Tuple.Create(limitationGroups[0], 2), 
                Tuple.Create(limitationGroups[1], 2),
                Tuple.Create(limitationGroups[5], 2),
                Tuple.Create(limitationGroups[6], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>() 
            { 
                Tuple.Create(limitationGroups[0], 5), 
                Tuple.Create(limitationGroups[1], 3),
                Tuple.Create(limitationGroups[5], 3),
                Tuple.Create(limitationGroups[6], 5),
            };

            var subject2Teacher = new List<Tuple<Subject, Teacher>>() 
            { 
                Tuple.Create(subjects[0], teachers[0]), 
                Tuple.Create(subjects[1], teachers[1]),
                Tuple.Create(subjects[2], teachers[3]),
                Tuple.Create(subjects[3], teachers[2]),
                Tuple.Create(subjects[4], teachers[4]),
            };

            List<Group> groups = new List<Group>()
            {
                new Group("12b", dayLims, weekLims, subject2Teacher),
                new Group("11a", dayLims, weekLims, subject2Teacher),
                //new Group("10v", dayLims, weekLims, subject2Teacher),
            };

            ScheduleGenerator sg = new ScheduleGenerator(groups, teachers, subjects);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            string[,,] schedule = sg.generate();
            sg.printSchedule(schedule);

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            while (true) ;
        }
    }
}
