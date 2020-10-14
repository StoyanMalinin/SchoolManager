using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Net.Http.Headers;

namespace SchoolManager.Generation_utils
{
    class ScheduleGenerator2
    {
        const int workDays = 5;
        const int maxLessons = 5;

        List<Group> groups;
        List<Teacher> teachers;
        List<Subject> subjects;

        public ScheduleGenerator2() { }
        public ScheduleGenerator2(List<Group> groups, List<Teacher> teachers, List<Subject> subjects)
        {
            this.groups = groups;
            this.teachers = teachers;
            this.subjects = subjects;
        }

        private List<DaySchedule>[] ds;
        private string[,,] result = null;
        private int[,] teacherLeftLessons;

        int cntGenerated = 0;

        private void gen(int[,,] a, int day, int groupInd)
        {
            void rec(int day, int groupInd, int sInd, int lessonsTaken)
            {
                if (sInd == ds[day][groupInd].g.subject2Teacher.Count)
                {
                    if (lessonsTaken != maxLessons) return;

                    gen(a, day, groupInd + 1);
                    return;
                }

                int teacherInd = teachers.FindIndex(t => t.name==ds[day][groupInd].g.subject2Teacher[sInd].Item2.name);
                if (lessonsTaken < maxLessons && teacherLeftLessons[day, teacherInd] > 0
                    && ds[day][groupInd].g.checkSubject(sInd) == true)
                {
                    teacherLeftLessons[day, teacherInd]--;
                    ds[day][groupInd].applySubject(sInd, +1);

                    rec(day, groupInd, sInd, lessonsTaken + 1);
                    
                    ds[day][groupInd].applySubject(sInd, -1);
                    teacherLeftLessons[day, teacherInd]++;
                }

                rec(day, groupInd, sInd + 1, lessonsTaken);
            }

            if (groupInd==groups.Count)
            {
                cntGenerated++;
                if (cntGenerated % (int)1e4 == 0) Console.WriteLine(cntGenerated);

                //Console.WriteLine("started");
                ScheduleCompleter completer = new ScheduleCompleter(ds[day], teachers, maxLessons);

                /*
                string[,] currDay = completer.gen();
                if (currDay == null)
                {
                    Console.WriteLine("----------------------------failed");
                    foreach(var x in ds[day])
                    {
                        Console.WriteLine(x.g.name);
                        foreach(var y in x.curriculum)
                            Console.WriteLine($"{y.Item1.name} -> {y.Item2}");
                    }

                    return;
                }

                for (int lesson = 1; lesson <= maxLessons; lesson++)
                    for (int t = 0; t < teachers.Count; t++)
                        result[day, lesson, t] = currDay[lesson, t];
                */

                groupInd = 0;
                day++;
            }
            if (day == workDays + 1)
            {
                Console.WriteLine("aideee");
                printSchedule(result);

                while (true) ;

                return;
            }

            
            int lessonsPossible = 0;
            for (int i = 2; i < ds[day][groupInd].g.dayLims.Count; i++)
            {
                lessonsPossible += Math.Min(ds[day][groupInd].g.dayLims[i].cnt, ds[day][groupInd].g.weekLims[i].cnt);
            }
            if (lessonsPossible < maxLessons) return;

            for (int s = 0; s < ds[day][groupInd].g.subject2Teacher.Count; s++)
            {
                int perDay = ds[day][groupInd].g.dayLims[ds[day][groupInd].g.subjectDaySelf[s]].cnt;
                int lessonsLeft = ds[day][groupInd].g.weekLims[ds[day][groupInd].g.subjectWeekSelf[s]].cnt;

                if ((workDays - day + 1) * perDay < lessonsLeft) return;
            }

            /*
            Console.WriteLine($"{day} {groupInd}");
            foreach (var x in ds[day][groupInd].g.dayLims)
                Console.WriteLine($"{x.g.name} - {x.cnt}");
            foreach (var x in ds[day][groupInd].g.weekLims)
                Console.WriteLine($"{x.g.name} - {x.cnt}");
            */

            rec(day, groupInd, 0, 0);
        }

        public string[,,] generate()
        {
            int[,,] a = new int[workDays + 1, maxLessons + 1, teachers.Count];
            initGeneration();

            gen(a, 1, 0);
            return result;
        }

        public void printSchedule(string[,,] schedule)
        {
            for (int day = 1; day <= workDays; day++)
            {
                Console.WriteLine($"day {day}");
                for (int lesson = 1; lesson <= maxLessons; lesson++)
                {
                    Console.Write($"{lesson}:");
                    for (int teacherInd = 0; teacherInd < teachers.Count; teacherInd++)
                        Console.Write($" {schedule[day, lesson, teacherInd]}");

                    Console.WriteLine();
                }
            }
        }

        private void initGeneration()
        {
            teacherLeftLessons = new int[workDays + 1, teachers.Count];
            for (int day = 1; day <= workDays; day++)
                for (int t = 0; t < teachers.Count; t++)
                    teacherLeftLessons[day, t] = maxLessons;

            ds = new List<DaySchedule>[workDays + 1];

            ds[1] = groups.Select(g => new DaySchedule(g.CloneFull())).ToList();
            for (int day = 2; day <= workDays; day++)
                ds[day] = ds[1].Select(x => new DaySchedule(x.g.ClonePartial(x.g.weekLims))).ToList();

            result = new string[workDays + 1, maxLessons + 1, teachers.Count];
        }
    }
}
