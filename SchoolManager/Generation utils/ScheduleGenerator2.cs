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

        private int[,] groupLeftLessons; 
        private List<DaySchedule>[] ds;
        private string[,,] result = null;
        private int[,] teacherLeftLessons;
        private int[,] groupSubject2Teacher;
        private List<int>[,] teacherGroup2Subjects; 



        int cntGenerated = 0;

        private bool checkFailed(int day, int groupInd)
        {
            for (int g = 0; g < groups.Count; g++)
            {
                /*
                int lessonsPossible = 0;
                for(int s = 0;s<ds[day][g].g.subject2Teacher.Count;s++)
                {
                    if (ds[day][g].g.subject2Teacher[s].Item2 == null) continue;

                    int add = Math.Min(ds[day][g].g.dayLims[ds[day][g].g.subjectDaySelf[s]].cnt,
                                       ds[day][g].g.weekLims[ds[day][g].g.subjectWeekSelf[s]].cnt);
                    add = Math.Min(add, teacherLeftLessons[day,groupSubject2Teacher[g, s]]);

                    lessonsPossible += add;
                }
                //if (lessonsPossible < maxLessons) return true;
                */

                for (int s = 0; s < ds[day][g].g.subject2Teacher.Count; s++)
                {
                    if (groups[g].subject2Teacher[s].Item2 == null) continue;

                    int teacherInd = groupSubject2Teacher[g, s];
                    int lessonsLeft = ds[day][g].g.weekLims[ds[day][g].g.subjectWeekSelf[s]].cnt;

                    for (int d = day; d <= workDays; d++)
                    {
                        int rm = Math.Min(ds[d][g].g.dayLims[ds[d][g].g.subjectDaySelf[s]].cnt, teacherLeftLessons[d, teacherInd]);
                        rm = Math.Min(rm, groupLeftLessons[d, g]);

                        lessonsLeft -= rm;
                    }

                    if (lessonsLeft > 0) return true;
                }
            }

            for (int t = 0; t < teachers.Count; t++)
            {
                int requested = 0;
                for (int g = 0; g < groups.Count; g++)
                {
                    foreach (int s in teacherGroup2Subjects[t, g])
                    {
                        if (ds[day][g].g.subject2Teacher[s].Item2.name == teachers[t].name)
                        {
                            requested += ds[day][g].g.weekLims[ds[day][g].g.subjectWeekSelf[s]].cnt;
                        }
                    }
                }

                for (int d = day; d <= workDays; d++)
                    requested -= teacherLeftLessons[d, t];

                if (requested > 0) return true;
            }

            return false;
        }

        int lastDayPrinted = -1, dayChanges = 0;

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

                int teacherInd = groupSubject2Teacher[groupInd, sInd];
                if (lessonsTaken < maxLessons && teacherLeftLessons[day, teacherInd] > 0
                    && ds[day][groupInd].g.checkSubject(sInd) == true)
                {
                    groupLeftLessons[day, groupInd]--;
                    teacherLeftLessons[day, teacherInd]--;
                    ds[day][groupInd].applySubject(sInd, +1);

                    rec(day, groupInd, sInd, lessonsTaken + 1);

                    groupLeftLessons[day, groupInd]++;
                    ds[day][groupInd].applySubject(sInd, -1);
                    teacherLeftLessons[day, teacherInd]++;
                }

                rec(day, groupInd, sInd + 1, lessonsTaken);
            }

            if (groupInd==groups.Count)
            {
                cntGenerated++;
                if (cntGenerated % (int)1e4 == 0) Console.WriteLine(cntGenerated);

                groupInd = 0;
                day++;
            }
            if (day == workDays + 1)
            {
                Console.WriteLine("aideee");

                Console.WriteLine(string.Join(" ", teachers.Select(t => t.name)));
                for(int d = 1;d<=workDays;d++)
                {
                    ScheduleCompleter completer = new ScheduleCompleter(ds[d], teachers, maxLessons);

                    string[,] currDay = completer.gen();
                    if (currDay == null)
                    {
                        Console.WriteLine("----------------------------failed");
                        foreach(var x in ds[d])
                        {
                            Console.WriteLine(x.g.name);
                            foreach(var y in x.curriculum)
                                Console.WriteLine($"{y.Item1.name} -> {y.Item2}");
                        }

                        return;
                    }

                    for (int lesson = 1; lesson <= maxLessons; lesson++)
                        for (int t = 0; t < teachers.Count; t++)
                            result[d, lesson, t] = currDay[lesson, t];
                }

                printSchedule(result);

                Console.WriteLine($"dayChanges = {dayChanges}");
                Console.WriteLine($"Ellapsed milliseconds = {sw.ElapsedMilliseconds}");

                while (true) ;

                return;
            }

            if (checkFailed(day, groupInd) == true) return;

            //if (day == 2 && groupInd == 4) Console.ReadLine();
            
            //Console.WriteLine($"{day} {groupInd}");
            if (day != lastDayPrinted)
            {
                dayChanges++;
                lastDayPrinted = day;

                //Console.ReadLine();
            }

            rec(day, groupInd, 0, 0);
        }

        Stopwatch sw = new Stopwatch();
        public string[,,] generate()
        {
            int[,,] a = new int[workDays + 1, maxLessons + 1, teachers.Count];
            initGeneration();

            sw.Start();

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

            groupLeftLessons = new int[workDays + 1, groups.Count];
            for (int day = 1; day <= workDays; day++)
                for (int g = 0; g < groups.Count; g++)
                    groupLeftLessons[day, g] = maxLessons;

            ds = new List<DaySchedule>[workDays + 1];
            ds[1] = groups.Select(g => new DaySchedule(g.CloneFull())).ToList();
            for (int day = 2; day <= workDays; day++)
                ds[day] = ds[1].Select(x => new DaySchedule(x.g.ClonePartial(x.g.weekLims))).ToList();

            groupSubject2Teacher = new int[groups.Count, subjects.Count];
            for(int g = 0;g<groups.Count;g++)
            {
                for(int s = 0;s<groups[g].subject2Teacher.Count;s++)
                {
                    if (groups[g].subject2Teacher[s].Item2 == null)
                    {
                        continue;
                    }

                    groupSubject2Teacher[g, s] = teachers.FindIndex(t => t.name == groups[g].subject2Teacher[s].Item2.name);
                }
            }

            teacherGroup2Subjects = new List<int>[teachers.Count, groups.Count];
            for (int t = 0; t < teachers.Count; t++)
            {
                for (int g = 0; g < groups.Count; g++)
                {
                    teacherGroup2Subjects[t, g] = new List<int>();
                    for (int s = 0; s < groups[g].subject2Teacher.Count; s++)
                    {
                        if (groups[g].subject2Teacher[s].Item2 == null) continue;

                        if (groups[g].subject2Teacher[s].Item2.name == teachers[t].name)
                        {
                            teacherGroup2Subjects[t, g].Add(s);
                        }
                    }
                }
            }

            result = new string[workDays + 1, maxLessons + 1, teachers.Count];
        }
    }
}
