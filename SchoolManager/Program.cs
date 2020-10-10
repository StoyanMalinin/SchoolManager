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
                new Subject("matematika", new List<LimitationGroup>(){ limitationGroups[0], limitationGroups[2] }),
                new Subject("istoriq", new List<LimitationGroup>(){ limitationGroups[1], limitationGroups[3] }),
                new Subject("geografiq", new List<LimitationGroup>(){ limitationGroups[1], limitationGroups[4] }),
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
                new Teacher("krusteva", new List<Subject>(){  }),
                new Teacher("dancheto", new List<Subject>(){  }),
                new Teacher("balieva", new List<Subject>(){  }),
                new Teacher("lateva", new List<Subject>(){  }),
            };

            var dayLims = new List<Tuple<LimitationGroup, int>>() 
            { 
                Tuple.Create(limitationGroups[0], 2), 
                Tuple.Create(limitationGroups[1], 2),

                Tuple.Create(limitationGroups[2], 2),
                Tuple.Create(limitationGroups[3], 1),
                Tuple.Create(limitationGroups[4], 1),
                Tuple.Create(limitationGroups[5], 2),
                Tuple.Create(limitationGroups[6], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>() 
            { 
                Tuple.Create(limitationGroups[0], 6969), 
                Tuple.Create(limitationGroups[1], 6969),
                
                Tuple.Create(limitationGroups[2], 6),
                Tuple.Create(limitationGroups[3], 3),
                Tuple.Create(limitationGroups[4], 4),
                Tuple.Create(limitationGroups[5], 6),
                Tuple.Create(limitationGroups[6], 6),
            };

            var subject2Teacher1 = new List<Tuple<Subject, Teacher>>() 
            { 
                Tuple.Create(subjects[0], teachers[0]), 
                Tuple.Create(subjects[3], teachers[2]),
                Tuple.Create(subjects[4], teachers[4]),
                Tuple.Create(subjects[2], teachers[3]),
                Tuple.Create(subjects[1], teachers[1]),
            };

            List<Group> groups = new List<Group>()
            {
                new Group("12b", dayLims, weekLims, subject2Teacher1),
                new Group("11a", dayLims, weekLims, subject2Teacher1),
                new Group("12a", dayLims, weekLims, subject2Teacher1),
                //new Group("10v", dayLims, weekLims, subject2Teacher1),
                //new Group("9b", dayLims, weekLims, subject2Teacher),
            };

            var subject2Teacher2 = new List<Tuple<Subject, Teacher>>()
            {
                Tuple.Create(subjects[0], teachers[5]),
                Tuple.Create(subjects[3], teachers[7]),
                Tuple.Create(subjects[4], teachers[6]),
                Tuple.Create(subjects[1], teachers[8]),
                Tuple.Create(subjects[2], teachers[3]),
            };

            groups.Add(new Group("10v", dayLims, weekLims, subject2Teacher2));
            groups.Add(new Group("9b", dayLims, weekLims, subject2Teacher2));
            groups.Add(new Group("8g", dayLims, weekLims, subject2Teacher2));

            //Shuffle(groups);
            ScheduleGenerator sg = new ScheduleGenerator(groups, teachers, subjects);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            string[,,] schedule = sg.generate();
            //sg.printSchedule(schedule);

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            while (true) ;
        }
    }
}
