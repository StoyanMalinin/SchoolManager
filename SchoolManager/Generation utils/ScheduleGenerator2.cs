﻿using SchoolManager.School_Models;
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
        private int[,] groupSubject2Teacher;

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

                int teacherInd = groupSubject2Teacher[groupInd, sInd];
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
                //ScheduleCompleter completer = new ScheduleCompleter(ds[day], teachers, maxLessons);

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
                
                if(day!=workDays+1)
                {
                    for (int g = groupInd; g < groups.Count; g++)
                    {
                        int lessonsPossible = 0;
                        for (int i = 2; i < ds[day][g].g.dayLims.Count; i++)
                        {
                            lessonsPossible += Math.Min(ds[day][g].g.dayLims[i].cnt, ds[day][g].g.weekLims[i].cnt);
                        }
                        if (lessonsPossible < maxLessons) return;

                        for (int s = 0; s < ds[day][groupInd].g.subject2Teacher.Count; s++)
                        {
                            int teacherInd = groupSubject2Teacher[g, s];
                            int lessonsLeft = ds[day][g].g.weekLims[ds[day][g].g.subjectWeekSelf[s]].cnt;

                            for (int d = day; d <= workDays; d++)
                            {
                                lessonsLeft -= Math.Min(ds[day][g].g.dayLims[ds[day][g].g.subjectDaySelf[s]].cnt, teacherLeftLessons[d, teacherInd]);
                            }

                            if (lessonsLeft > 0) return;
                        }
                    }

                    for (int t = 0; t < teachers.Count; t++)
                    {
                        int requested = 0;
                        for (int g = 0; g < groups.Count; g++)
                        {
                            for (int s = 0; s < ds[day][g].g.subject2Teacher.Count; s++)
                            {
                                if (ds[day][g].g.subject2Teacher[s].Item2.name == teachers[t].name)
                                {
                                    requested += ds[day][g].g.weekLims[ds[day][g].g.subjectWeekSelf[s]].cnt;
                                    break;
                                }
                            }
                        }

                        if (requested > (workDays - day + 1) * maxLessons) return;
                    }
                }
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
                Console.WriteLine($"Ellapsed milliseconds = {sw.ElapsedMilliseconds}");

                while (true) ;

                return;
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

            ds = new List<DaySchedule>[workDays + 1];
            ds[1] = groups.Select(g => new DaySchedule(g.CloneFull())).ToList();
            for (int day = 2; day <= workDays; day++)
                ds[day] = ds[1].Select(x => new DaySchedule(x.g.ClonePartial(x.g.weekLims))).ToList();

            groupSubject2Teacher = new int[groups.Count, subjects.Count];
            for(int g = 0;g<groups.Count;g++)
            {
                for(int s = 0;s<groups[g].subject2Teacher.Count;s++)
                {
                    groupSubject2Teacher[g, s] = teachers.FindIndex(t => t.name == groups[g].subject2Teacher[s].Item2.name);
                }
            }

            result = new string[workDays + 1, maxLessons + 1, teachers.Count];
        }
    }
}