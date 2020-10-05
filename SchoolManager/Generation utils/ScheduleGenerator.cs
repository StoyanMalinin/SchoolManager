using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SchoolManager.Generation_utils
{
    class ScheduleGenerator
    {
        const int workDays = 4;
        const int maxLessons = 3;

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

        private void gen(int day, int lesson, int teacherInd, string[,,] a)
        {
            if (result != null) return;

            if(teacherInd==teachers.Count) 
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
            }
            if (day == workDays + 1)
            {
                result = a.Clone() as string[,,];
                return;
            }

            for(int g = 0;g<dayState[day].Count;g++)
            {
                //optimize later
                for(int s = 0;s<dayState[day][g].subject2Teacher.Count;s++)
                {
                    if (dayState[day][g].subject2Teacher[s].Item2 == teachers[teacherInd]
                        && dayState[day][g].checkSubject(dayState[day][g].subject2Teacher[s].Item1)==true
                        && usedGroup[day, lesson, g] == false)
                    {
                        usedGroup[day, lesson, g] = true;
                        a[day, lesson, teacherInd] = dayState[day][g].name;
                        dayState[day][g].applySubject(dayState[day][g].subject2Teacher[s].Item1, +1);

                        gen(day, lesson, teacherInd + 1, a);

                        dayState[day][g].applySubject(dayState[day][g].subject2Teacher[s].Item1, -1);
                        a[day, lesson, teacherInd] = "-";
                        usedGroup[day, lesson, g] = false;
                    }
                }
            }

            a[day, lesson, teacherInd] = "-";
            gen(day, lesson, teacherInd + 1, a);
            a[day, lesson, teacherInd] = "-";
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
