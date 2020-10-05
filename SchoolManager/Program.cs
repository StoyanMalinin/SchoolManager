using System;
using System.Collections.Generic;
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
            };

            List<Subject> subjects = new List<Subject>()
            {
                new Subject("matematika", new List<LimitationGroup>(){ limitationGroups[0]}),
                new Subject("istoriq", new List<LimitationGroup>(){ limitationGroups[1]}),
                new Subject("geografiq", new List<LimitationGroup>(){ limitationGroups[1]}),
            };

            List<Teacher> teachers = new List<Teacher>()
            {
                new Teacher("batimkata", new List<Subject>(){ subjects[0] }),
                new Teacher("kireto", new List<Subject>(){ subjects[1], subjects[2] }),
            };

            List<Group> groups = new List<Group>()
            {
                new Group("12b",
                          new List<Tuple<LimitationGroup, int>>() { Tuple.Create(limitationGroups[0], 2), Tuple.Create(limitationGroups[1], 1) },
                          new List<Tuple<LimitationGroup, int>>() { Tuple.Create(limitationGroups[0], 6), Tuple.Create(limitationGroups[1], 3)  },
                          new List<Tuple<Subject, Teacher>>() { Tuple.Create(subjects[0], teachers[0]), Tuple.Create(subjects[1], teachers[1]) })
            };

            ScheduleGenerator sg = new ScheduleGenerator(groups, teachers, subjects);
            string[,,] schedule = sg.generate();

            sg.printSchedule(schedule);
        }
    }
}
