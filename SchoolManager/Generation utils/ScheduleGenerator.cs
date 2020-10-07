using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;

namespace SchoolManager.Generation_utils
{
    class ScheduleGenerator
    {
        const int workDays = 4;
        const int maxLessons = 4;

        List<Group> groups;
        List<Teacher> teachers;
        List<Subject> subjects;

        public ScheduleGenerator() { }
        public ScheduleGenerator(List<Group> groups, List <Teacher> teachers, List <Subject> subjects)
        {
            this.groups = groups;
            this.teachers = teachers;
            this.subjects = subjects;
        }

        private string[,,] result = null;

        private bool[,,] usedGroup;
        private List<Group>[] dayState;

        private List<int>[,] groupTeacherMatches; 

        private void gen(int day, int lesson, int teacherInd, string[,,] a)
        {
            if (result != null) return;

            if (teacherInd==teachers.Count) 
            {
                //add special skip condition
                for (int i = 0; i < groups.Count; i++)
                {
                    if (usedGroup[day, lesson, i] == false)
                    {
                        return;
                    }
                }

                teacherInd = 0; 
                lesson++;
            }
            if(lesson==maxLessons+1)
            {
                lesson = 1; 
                day++;

                if (day<=workDays)
                {
                    //redo later
                    foreach (Group g in dayState[day])
                    {
                        int lessonsPossible = 0;
                        for (int i = 0; i < g.dayLims.Count; i++)
                        {
                            lessonsPossible += Math.Min(g.dayLims[i].Item2, g.weekLims[i].Item2);
                        }

                        if (lessonsPossible < maxLessons) return;
                    }
                }
            }
            if (day == workDays + 1)
            {
                result = a.Clone() as string[,,];
                foreach (Group g in dayState[day - 1]) Console.WriteLine(g.dayLims[1].Item2);
                printSchedule(a);

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();

                return;
            }

            for (int g = 0;g < dayState[day].Count;g++)
            {
                foreach(int s in groupTeacherMatches[g, teacherInd])
                {
                    if (dayState[day][g].checkSubject(s)==true
                        && usedGroup[day, lesson, g] == false)
                    {
                        usedGroup[day, lesson, g] = true;
                        a[day, lesson, teacherInd] = dayState[day][g].name;
                        dayState[day][g].applySubject(s, +1);

                        gen(day, lesson, teacherInd + 1, a);

                        dayState[day][g].applySubject(s, -1);
                        a[day, lesson, teacherInd] = "---";
                        usedGroup[day, lesson, g] = false;
                    }
                }
            }

            a[day, lesson, teacherInd] = "---";
            gen(day, lesson, teacherInd + 1, a);
            a[day, lesson, teacherInd] = "---";
        }

        public string[,,] generate()
        {
            string[,,] a = new string[workDays+1, maxLessons+1, teachers.Count];

            usedGroup = new bool[workDays + 1, maxLessons + 1, teachers.Count];
            for (int day = 1; day <= workDays; day++)
                for (int lesson = 1; lesson <= maxLessons; lesson++)
                    for (int groupInd = 0; groupInd < groups.Count; groupInd++)
                        usedGroup[day, lesson, groupInd] = false;

            dayState = new List<Group>[workDays + 1];
            
            dayState[1] = groups.Select(x => x.CloneFull()).ToList();
            for (int i = 2; i <= workDays; i++)
            {
                dayState[i] = new List<Group>();
                for(int j = 0;j<dayState[1].Count;j++)
                {
                    dayState[i].Add(dayState[1][j].ClonePartial(dayState[1][j].weekLims));
                }
            }

            groupTeacherMatches = new List<int>[groups.Count, teachers.Count];
            for(int g = 0;g<groups.Count;g++)
            {
                for(int t = 0;t<teachers.Count;t++)
                {
                    groupTeacherMatches[g, t] = new List<int>();
                    for(int s = 0;s<groups[g].subject2Teacher.Count;s++)
                    {
                        if (groups[g].subject2Teacher[s].Item2 == teachers[t])
                            groupTeacherMatches[g, t].Add(s);
                    }
                }
            }

            gen(1, 1, 0, a);
            return result;
        }

        public void printSchedule(string[,,] schedule)
        {
            for(int day = 1;day<=workDays;day++)
            {
                Console.WriteLine($"day {day}");
                for(int lesson = 1;lesson<=maxLessons;lesson++)
                {
                    Console.Write($"{lesson}:");
                    for (int teacherInd = 0; teacherInd < teachers.Count; teacherInd++)
                        Console.Write($" {schedule[day, lesson, teacherInd]}");
                    Console.WriteLine();
                }
            }
        }
    }
}
