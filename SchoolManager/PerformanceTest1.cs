using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SchoolManager
{
    static class PerformanceTest1
    {
        public static List<Subject> subjects;
        public static List<LimitationGroup> limitationGroups;
        public static List<Teacher> teachers;

        public static void init()
        {
            limitationGroups = new List<LimitationGroup>()
            {
                new LimitationGroup("to4ni"),
                new LimitationGroup("razkazvatelni"),
                new LimitationGroup("matematika"),
                new LimitationGroup("istoriq"),
                new LimitationGroup("geografiq"),
                new LimitationGroup("balgarski"),
                new LimitationGroup("angliiski"),
                new LimitationGroup("svqt i lichnost"),
                new LimitationGroup("fizika"),
                new LimitationGroup("fizichesko"),
            };

            subjects = new List<Subject>()
            {
                new Subject("matematika", new List<LimitationGroup>(){ limitationGroups[0], limitationGroups[2] }),
                new Subject("istoriq", new List<LimitationGroup>(){ limitationGroups[1], limitationGroups[3] }),
                new Subject("geografiq", new List<LimitationGroup>(){ limitationGroups[1], limitationGroups[4] }),
                new Subject("balgarski", new List<LimitationGroup>(){ limitationGroups[5]}),
                new Subject("angliiski", new List<LimitationGroup>(){ limitationGroups[6]}),
                new Subject("svqt i lichnost", new List<LimitationGroup>(){ limitationGroups[7]}),
                new Subject("fizika", new List<LimitationGroup>(){ limitationGroups[8]}),
                new Subject("fizichesko", new List<LimitationGroup>(){ limitationGroups[9]}),
            };

            teachers = new List<Teacher>()
            {
                new Teacher("batimkata", new List<Subject>(){ }),//0
                new Teacher("kireto", new List<Subject>(){ }),//1
                new Teacher("valcheto", new List<Subject>(){ }),//2
                new Teacher("slav4eto", new List<Subject>(){ }),//3
                new Teacher("gabarcheto", new List<Subject>(){  }),//4
                new Teacher("krusteva", new List<Subject>(){  }),//5
                new Teacher("dancheto", new List<Subject>(){  }),//6
                new Teacher("balieva", new List<Subject>(){  }),//7
                new Teacher("lateva", new List<Subject>(){  }),//8
                new Teacher("klisarova", new List<Subject>(){  }),//9
                new Teacher("ivanova", new List<Subject>(){  }),//10
                new Teacher("vlashka", new List<Subject>(){  }),//11
                new Teacher("tisheto", new List<Subject>(){  }),//12
                new Teacher("kongalov", new List<Subject>(){  }),//13
            };
        }

        public static Group _12a()
        {
            var dayLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 2),
                Tuple.Create(limitationGroups[1], 2),

                Tuple.Create(limitationGroups[2], 2),
                Tuple.Create(limitationGroups[3], 1),
                Tuple.Create(limitationGroups[4], 1),
                Tuple.Create(limitationGroups[5], 2),
                Tuple.Create(limitationGroups[6], 2),
                Tuple.Create(limitationGroups[7], 1),
                Tuple.Create(limitationGroups[8], 1),
                Tuple.Create(limitationGroups[9], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 6),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 3),
            };

            var subject2Teacher = new List<Tuple<Subject, Teacher>>()
            {
                Tuple.Create(subjects[0], teachers[0]),
                Tuple.Create(subjects[1], teachers[3]),
                Tuple.Create(subjects[2], teachers[1]),
                Tuple.Create(subjects[3], teachers[2]),
                Tuple.Create(subjects[4], teachers[4]),
                Tuple.Create(subjects[5], teachers[10]),
                Tuple.Create(subjects[6], teachers[11]),
                Tuple.Create(subjects[7], teachers[12]),
            };

            Group g = new Group("12a", dayLims, weekLims, subject2Teacher);
            return g;
        }

        public static Group _12b()
        {
            var dayLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 2),
                Tuple.Create(limitationGroups[1], 2),

                Tuple.Create(limitationGroups[2], 2),
                Tuple.Create(limitationGroups[3], 1),
                Tuple.Create(limitationGroups[4], 1),
                Tuple.Create(limitationGroups[5], 2),
                Tuple.Create(limitationGroups[6], 2),
                Tuple.Create(limitationGroups[7], 1),
                Tuple.Create(limitationGroups[8], 1),
                Tuple.Create(limitationGroups[9], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 6),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 3),
            };

            var subject2Teacher = new List<Tuple<Subject, Teacher>>()
            {
                Tuple.Create(subjects[0], teachers[5]),
                Tuple.Create(subjects[1], teachers[3]),
                Tuple.Create(subjects[2], teachers[1]),
                Tuple.Create(subjects[3], teachers[2]),
                Tuple.Create(subjects[4], teachers[4]),
                Tuple.Create(subjects[5], teachers[10]),
                Tuple.Create(subjects[6], teachers[11]),
                Tuple.Create(subjects[7], teachers[12]),
            };

            Group g = new Group("12b", dayLims, weekLims, subject2Teacher);
            return g;
        }

        public static Group _12v()
        {
            var dayLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 2),
                Tuple.Create(limitationGroups[1], 2),

                Tuple.Create(limitationGroups[2], 2),
                Tuple.Create(limitationGroups[3], 1),
                Tuple.Create(limitationGroups[4], 1),
                Tuple.Create(limitationGroups[5], 2),
                Tuple.Create(limitationGroups[6], 2),
                Tuple.Create(limitationGroups[7], 1),
                Tuple.Create(limitationGroups[8], 1),
                Tuple.Create(limitationGroups[9], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 6),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 3),
            };

            var subject2Teacher = new List<Tuple<Subject, Teacher>>()
            {
                Tuple.Create(subjects[0], teachers[5]),
                Tuple.Create(subjects[1], teachers[3]),
                Tuple.Create(subjects[2], teachers[1]),
                Tuple.Create(subjects[3], teachers[2]),
                Tuple.Create(subjects[4], teachers[6]),
                Tuple.Create(subjects[5], teachers[10]),
                Tuple.Create(subjects[6], teachers[11]),
                Tuple.Create(subjects[7], teachers[12]),
            };

            Group g = new Group("12v", dayLims, weekLims, subject2Teacher);
            return g;
        }

        public static Group _12g()
        {
            var dayLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 2),
                Tuple.Create(limitationGroups[1], 2),

                Tuple.Create(limitationGroups[2], 2),
                Tuple.Create(limitationGroups[3], 1),
                Tuple.Create(limitationGroups[4], 1),
                Tuple.Create(limitationGroups[5], 2),
                Tuple.Create(limitationGroups[6], 2),
                Tuple.Create(limitationGroups[7], 1),
                Tuple.Create(limitationGroups[8], 1),
                Tuple.Create(limitationGroups[9], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 6),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 3),
            };

            var subject2Teacher = new List<Tuple<Subject, Teacher>>()
            {
                Tuple.Create(subjects[0], teachers[5]),
                Tuple.Create(subjects[1], teachers[3]),
                Tuple.Create(subjects[2], teachers[1]),
                Tuple.Create(subjects[3], teachers[2]),
                Tuple.Create(subjects[4], teachers[6]),
                Tuple.Create(subjects[5], teachers[10]),
                Tuple.Create(subjects[6], teachers[11]),
                Tuple.Create(subjects[7], teachers[12]),
            };

            Group g = new Group("12g", dayLims, weekLims, subject2Teacher);
            return g;
        }

        public static Group _11а()
        {
            var dayLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 2),
                Tuple.Create(limitationGroups[1], 2),

                Tuple.Create(limitationGroups[2], 2),
                Tuple.Create(limitationGroups[3], 1),
                Tuple.Create(limitationGroups[4], 1),
                Tuple.Create(limitationGroups[5], 2),
                Tuple.Create(limitationGroups[6], 2),
                Tuple.Create(limitationGroups[7], 1),
                Tuple.Create(limitationGroups[8], 1),
                Tuple.Create(limitationGroups[9], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 6),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 3),
            };

            var subject2Teacher = new List<Tuple<Subject, Teacher>>()
            {
                Tuple.Create(subjects[0], teachers[0]),
                Tuple.Create(subjects[1], teachers[3]),
                Tuple.Create(subjects[2], teachers[1]),
                Tuple.Create(subjects[3], teachers[2]),
                Tuple.Create(subjects[4], teachers[6]),
                Tuple.Create(subjects[5], teachers[10]),
                Tuple.Create(subjects[6], teachers[11]),
                Tuple.Create(subjects[7], teachers[12]),
            };

            Group g = new Group("11a", dayLims, weekLims, subject2Teacher);
            return g;
        }

        public static Group _11b()
        {
            var dayLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 2),
                Tuple.Create(limitationGroups[1], 2),

                Tuple.Create(limitationGroups[2], 2),
                Tuple.Create(limitationGroups[3], 1),
                Tuple.Create(limitationGroups[4], 1),
                Tuple.Create(limitationGroups[5], 2),
                Tuple.Create(limitationGroups[6], 2),
                Tuple.Create(limitationGroups[7], 1),
                Tuple.Create(limitationGroups[8], 1),
                Tuple.Create(limitationGroups[9], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 6),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 3),
            };

            var subject2Teacher = new List<Tuple<Subject, Teacher>>()
            {
                Tuple.Create(subjects[0], teachers[0]),
                Tuple.Create(subjects[1], teachers[3]),
                Tuple.Create(subjects[2], teachers[1]),
                Tuple.Create(subjects[3], teachers[7]),
                Tuple.Create(subjects[4], teachers[6]),
                Tuple.Create(subjects[5], teachers[10]),
                Tuple.Create(subjects[6], teachers[11]),
                Tuple.Create(subjects[7], teachers[12]),
            };

            Group g = new Group("11b", dayLims, weekLims, subject2Teacher);
            return g;
        }

        public static Group _11v()
        {
            var dayLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 2),
                Tuple.Create(limitationGroups[1], 2),

                Tuple.Create(limitationGroups[2], 2),
                Tuple.Create(limitationGroups[3], 1),
                Tuple.Create(limitationGroups[4], 1),
                Tuple.Create(limitationGroups[5], 2),
                Tuple.Create(limitationGroups[6], 2),
                Tuple.Create(limitationGroups[7], 1),
                Tuple.Create(limitationGroups[8], 1),
                Tuple.Create(limitationGroups[9], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 6),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 3),
            };

            var subject2Teacher = new List<Tuple<Subject, Teacher>>()
            {
                Tuple.Create(subjects[0], teachers[0]),
                Tuple.Create(subjects[1], teachers[3]),
                Tuple.Create(subjects[2], teachers[1]),
                Tuple.Create(subjects[3], teachers[7]),
                Tuple.Create(subjects[4], teachers[6]),
                Tuple.Create(subjects[5], teachers[10]),
                Tuple.Create(subjects[6], teachers[11]),
                Tuple.Create(subjects[7], teachers[12]),
            };

            Group g = new Group("11b", dayLims, weekLims, subject2Teacher);
            return g;
        }

        public static Group _10a()
        {
            var dayLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 2),
                Tuple.Create(limitationGroups[1], 2),

                Tuple.Create(limitationGroups[2], 2),
                Tuple.Create(limitationGroups[3], 1),
                Tuple.Create(limitationGroups[4], 1),
                Tuple.Create(limitationGroups[5], 2),
                Tuple.Create(limitationGroups[6], 2),
                Tuple.Create(limitationGroups[7], 1),
                Tuple.Create(limitationGroups[8], 1),
                Tuple.Create(limitationGroups[9], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 6),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 3),
            };

            var subject2Teacher = new List<Tuple<Subject, Teacher>>()
            {
                Tuple.Create(subjects[0], teachers[9]),
                Tuple.Create(subjects[1], teachers[3]),
                Tuple.Create(subjects[2], teachers[1]),
                Tuple.Create(subjects[3], teachers[7]),
                Tuple.Create(subjects[4], teachers[4]),
                Tuple.Create(subjects[5], teachers[10]),
                Tuple.Create(subjects[6], teachers[11]),
                Tuple.Create(subjects[7], teachers[13]),
            };

            Group g = new Group("12b", dayLims, weekLims, subject2Teacher);
            return g;
        }

        public static Group _11d()
        {
            var dayLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 2),
                Tuple.Create(limitationGroups[1], 2),

                Tuple.Create(limitationGroups[2], 2),
                Tuple.Create(limitationGroups[3], 1),
                Tuple.Create(limitationGroups[4], 1),
                Tuple.Create(limitationGroups[5], 2),
                Tuple.Create(limitationGroups[6], 2),
                Tuple.Create(limitationGroups[7], 1),
                Tuple.Create(limitationGroups[8], 1),
                Tuple.Create(limitationGroups[9], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 6),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 3),
            };

            var subject2Teacher = new List<Tuple<Subject, Teacher>>()
            {
                Tuple.Create(subjects[0], teachers[5]),
                Tuple.Create(subjects[1], teachers[3]),
                Tuple.Create(subjects[2], teachers[1]),
                Tuple.Create(subjects[3], teachers[2]),
                Tuple.Create(subjects[4], teachers[4]),
                Tuple.Create(subjects[5], teachers[10]),
                Tuple.Create(subjects[6], teachers[11]),
                Tuple.Create(subjects[7], teachers[12]),
            };

            Group g = new Group("12b", dayLims, weekLims, subject2Teacher);
            return g;
        }
    }
}
