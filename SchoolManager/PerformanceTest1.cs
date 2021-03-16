using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using SchoolManager.School_Models.Higharchy;

namespace SchoolManager
{
    static class PerformanceTest1
    {
        private static List<Subject> subjects;
        private static List<LimitationGroup> limitationGroups;
        private static List<Teacher> teachers;

        private static LimitationTreeNode higharchy = new LimitationTreeNode("root", null);

        private static void init()
        {
            limitationGroups = new List<LimitationGroup>()
            {
                new LimitationGroup("to4ni"),//0
                new LimitationGroup("razkazvatelni"),//1
                new LimitationGroup("matematika"),//2
                new LimitationGroup("istoriq"),//3
                new LimitationGroup("geografiq"),//4
                new LimitationGroup("balgarski"),//5
                new LimitationGroup("angliiski"),//6
                new LimitationGroup("svqt i lichnost"),//7
                new LimitationGroup("fizika"),//8
                new LimitationGroup("fizichesko"),//9
                new LimitationGroup("rosski/nemski"),//10
            };

            subjects = new List<Subject>()
            {
                new Subject("matematika", new List<LimitationGroup>(){ limitationGroups[0], limitationGroups[2] }),//0
                new Subject("istoriq", new List<LimitationGroup>(){ limitationGroups[1], limitationGroups[3] }),//1
                new Subject("geografiq", new List<LimitationGroup>(){ limitationGroups[1], limitationGroups[4] }),//2
                new Subject("balgarski", new List<LimitationGroup>(){ limitationGroups[5]}),//3
                new Subject("angliiski", new List<LimitationGroup>(){ limitationGroups[6]}),//4
                new Subject("svqt i lichnost", new List<LimitationGroup>(){ limitationGroups[1], limitationGroups[7]}),//5
                new Subject("fizika", new List<LimitationGroup>(){ limitationGroups[0], limitationGroups[8]}),//6
                new Subject("fizichesko", new List<LimitationGroup>(){ limitationGroups[9] }),//7
                new Subject("rosski/nemski", new List<LimitationGroup>(){ limitationGroups[10] }),//8
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
                new Teacher("daskalova", new List<Subject>(){  }),//14
                new Teacher("ruskinqta", new List<Subject>(){  }),//15
            };

            higharchy.addChild(new LimitationTreeNode(limitationGroups[0]));
            higharchy.addChild(new LimitationTreeNode(limitationGroups[1]));
            higharchy.addChild(new SubjectTreeNode(subjects[3]));
            higharchy.addChild(new SubjectTreeNode(subjects[4]));
            higharchy.addChild(new SubjectTreeNode(subjects[7]));
            higharchy.addChild(new SubjectTreeNode(subjects[8]));
            
            (higharchy.children[0] as LimitationTreeNode).addChild(new SubjectTreeNode(subjects[0]));
            (higharchy.children[1] as LimitationTreeNode).addChild(new SubjectTreeNode(subjects[1]));
            (higharchy.children[1] as LimitationTreeNode).addChild(new SubjectTreeNode(subjects[2]));
            (higharchy.children[1] as LimitationTreeNode).addChild(new SubjectTreeNode(subjects[5]));
            (higharchy.children[0] as LimitationTreeNode).addChild(new SubjectTreeNode(subjects[6]));

            //higharchy.printTree();
        }

        //данните за това колко са максималните часове по даден LimitationGroup за седмица (weekLims)
        //трябва да се допълват до 25

        //общите LimitationGroup-и (точни, разказвателни)
        //когато са в weekLims, трябва да бъдат сложени на голямо число, за да не пречат

        private static Group _12a()
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
                Tuple.Create(limitationGroups[10], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 5),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 2),
                Tuple.Create(limitationGroups[10], 2),
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
                Tuple.Create(subjects[8], (Teacher)null),
            };

            Group g = new Group("12a", dayLims, weekLims, subject2Teacher);
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[0], subjects[0], new Generation_utils.IntInInterval(1, 1)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[2], subjects[3], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[4], subjects[4], new Generation_utils.IntInInterval(2, 2)));

            return g;
        }

        private static Group _12b()
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
                Tuple.Create(limitationGroups[10], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 5),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 2),
                Tuple.Create(limitationGroups[10], 2),
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
                Tuple.Create(subjects[8], (Teacher)null),
            };

            Group g = new Group("12b", dayLims, weekLims, subject2Teacher);
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[2], subjects[3], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[5], subjects[0], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[4], subjects[4], new Generation_utils.IntInInterval(2, 2)));

            return g;
        }

        private static Group _12v()
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
                Tuple.Create(limitationGroups[10], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 5),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 2),
                Tuple.Create(limitationGroups[10], 2),
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
                Tuple.Create(subjects[8], (Teacher)null),
            };

            Group g = new Group("12v", dayLims, weekLims, subject2Teacher);
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[2], subjects[3], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[6], subjects[4], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[5], subjects[0], new Generation_utils.IntInInterval(2, 2)));

            return g;
        }

        private static Group _12g()
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
                Tuple.Create(limitationGroups[10], 2),
            };  

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 5),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 2),
                Tuple.Create(limitationGroups[10], 2),
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
                Tuple.Create(subjects[8], (Teacher)null),
            };

            Group g = new Group("12g", dayLims, weekLims, subject2Teacher);
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[2], subjects[3], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[5], subjects[0], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[6], subjects[4], new Generation_utils.IntInInterval(2, 2)));

            return g;
        }

        private static Group _11a()
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
                Tuple.Create(limitationGroups[10], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 5),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 2),
                Tuple.Create(limitationGroups[10], 2),
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
                Tuple.Create(subjects[8], (Teacher)null),
            };

            Group g = new Group("11a", dayLims, weekLims, subject2Teacher);
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[6], subjects[4], new Generation_utils.IntInInterval(2, 2)));

            return g;
        }

        private static Group _11b()
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
                Tuple.Create(limitationGroups[10], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 5),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 2),
                Tuple.Create(limitationGroups[10], 2),
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
                Tuple.Create(subjects[8], (Teacher)null),
            };

            Group g = new Group("11b", dayLims, weekLims, subject2Teacher);
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[6], subjects[4], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[7], subjects[3], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[0], subjects[0], new Generation_utils.IntInInterval(2, 2)));

            return g;
        }

        private static Group _11v()
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
                Tuple.Create(limitationGroups[10], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 5),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 2),
                Tuple.Create(limitationGroups[10], 2),
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
                Tuple.Create(subjects[8], (Teacher)null),
            };

            Group g = new Group("11v", dayLims, weekLims, subject2Teacher);
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[7], subjects[3], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[0], subjects[0], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[6], subjects[4], new Generation_utils.IntInInterval(2, 2)));

            return g;
        }

        private static Group _10a()
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

            Group g = new Group("10a", dayLims, weekLims, subject2Teacher);
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[4], subjects[4], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[7], subjects[3], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[9], subjects[0], new Generation_utils.IntInInterval(2, 2)));

            return g;
        }

        private static Group _11d()
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
                Tuple.Create(limitationGroups[10], 2),
            };

            var weekLims = new List<Tuple<LimitationGroup, int>>()
            {
                Tuple.Create(limitationGroups[0], 6969),
                Tuple.Create(limitationGroups[1], 6969),

                Tuple.Create(limitationGroups[2], 5),
                Tuple.Create(limitationGroups[3], 2),
                Tuple.Create(limitationGroups[4], 2),
                Tuple.Create(limitationGroups[5], 4),
                Tuple.Create(limitationGroups[6], 4),
                Tuple.Create(limitationGroups[7], 2),
                Tuple.Create(limitationGroups[8], 2),
                Tuple.Create(limitationGroups[9], 2),
                Tuple.Create(limitationGroups[10], 2),
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
                Tuple.Create(subjects[8], (Teacher)null),
            };

            Group g = new Group("11d", dayLims, weekLims, subject2Teacher);
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[2], subjects[3], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[4], subjects[4], new Generation_utils.IntInInterval(2, 2)));
            g.requiredMultilessons.Add(new Generation_utils.Multilesson(g, teachers[5], subjects[0], new Generation_utils.IntInInterval(2, 2)));

            return g;
        }
    
        private static SuperGroup _vtoriEzik12BV()
        {
            List<Tuple<Group, Subject>> currGroups = new List<Tuple<Group, Subject>>()
            {
                Tuple.Create(_12b(), subjects[8]),
                Tuple.Create(_12v(), subjects[8]),
            };

            List<Teacher> currTeachers = new List<Teacher>()
            {
                teachers[14],
                teachers[15],
            };

            List<int> requiredMultilessons = new List<int>() { 2 };
            SuperGroup sg = new SuperGroup("vtori12BV", currGroups, currTeachers, 2, requiredMultilessons);

            return sg;
        }

        private static SuperGroup _vtoriEzik11BV()
        {
            List<Tuple<Group, Subject>> currGroups = new List<Tuple<Group, Subject>>()
            {
                Tuple.Create(_11b(), subjects[8]),
                Tuple.Create(_11v(), subjects[8]),
            };

            List<Teacher> currTeachers = new List<Teacher>()
            {
                teachers[14],
                teachers[15],
            };

            List<int> requiredMultilessons = new List<int>() { 2 };
            SuperGroup sg = new SuperGroup("vtori11BV", currGroups, currTeachers, 2, requiredMultilessons);

            return sg;
        }

        private static SuperGroup _vtoriEzik11AD()
        {
            List<Tuple<Group, Subject>> currGroups = new List<Tuple<Group, Subject>>()
            {
                Tuple.Create(_11a(), subjects[8]),
                Tuple.Create(_11d(), subjects[8]),
            };

            List<Teacher> currTeachers = new List<Teacher>()
            {
                teachers[14],
                teachers[15],
            };

            List<int> requiredMultilessons = new List<int>() { 2 };
            SuperGroup sg = new SuperGroup("vtori11AD", currGroups, currTeachers, 2, requiredMultilessons);

            return sg;
        }

        private static SuperGroup _vtoriEzik12AG()
        {
            List<Tuple<Group, Subject>> currGroups = new List<Tuple<Group, Subject>>()
            {
                Tuple.Create(_12a(), subjects[8]),
                Tuple.Create(_12g(), subjects[8]),
            };

            List<Teacher> currTeachers = new List<Teacher>()
            {
                teachers[14],
                teachers[15],
            };

            List<int> requiredMultilessons = new List<int>() { 2 };
            SuperGroup sg = new SuperGroup("vtori12AG", currGroups, currTeachers, 2, requiredMultilessons);

            return sg;
        }

        public static void test()
        {
            init();
            List<Group> groups = new List<Group>();
            List<SuperGroup> superGroups = new List<SuperGroup>();

            groups.Add(_12a());
            groups.Add(_12b());
            groups.Add(_12v());
            groups.Add(_12g());
            groups.Add(_11a());
            groups.Add(_11b());
            groups.Add(_11v());
            groups.Add(_11d());
            groups.Add(_10a());

            superGroups.Add(_vtoriEzik12BV());
            superGroups.Add(_vtoriEzik11BV());
            superGroups.Add(_vtoriEzik11AD());
            superGroups.Add(_vtoriEzik12AG());

            List<Generation_utils.Multilesson>[] multilessons = new List<Generation_utils.Multilesson>[5 + 1];
            for (int day = 1; day <= 5; day++)
                multilessons[day] = new List<Generation_utils.Multilesson>();
            //multilessons[5].Add(new Multilesson(groups[0], PerformanceTest1.teachers[4], PerformanceTest1.subjects[4], new IntInInterval(2, 2)));
            //multilessons[5].Add(new Multilesson(groups[7], PerformanceTest1.teachers[4], PerformanceTest1.subjects[4], new IntInInterval(2, 2)));
            //multilessons[5].Add(new Multilesson(groups[0], PerformanceTest1.teachers[0], PerformanceTest1.subjects[0], new IntInInterval(1, 2)));
            //multilessons[5].Add(new Multilesson(groups[0], PerformanceTest1.teachers[2], PerformanceTest1.subjects[3], new IntInInterval(1, 1)));
            //multilessons[3].Add(new Multilesson(groups[2], PerformanceTest1.teachers[5], PerformanceTest1.subjects[0], new IntInInterval(2, 3)));

            Generation_utils.ScheduleGenerator4 sg = new Generation_utils.ScheduleGenerator4(groups, teachers, subjects,
                                                                                             higharchy, multilessons, superGroups);//за общи проблеми

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            sg.gen();
            sw.Stop();

            Console.WriteLine($"Ellapsed total time = {sw.ElapsedMilliseconds}");
        }
    }
}