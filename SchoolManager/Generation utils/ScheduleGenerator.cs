using SchoolManager.MaxFlow;
using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;

namespace SchoolManager.Generation_utils
{
    class ScheduleGenerator
    {
        const int workDays = 5;
        const int maxLessons = 5;

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
        private int cntGenerated = 0;

        private int[] teacherFreeLesons;

        int lastDayTime = 0;
        Stopwatch sw = new Stopwatch();

        private static Random rng = new Random();
        private void Shuffle<T>(IList<T> list)
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

        private bool completeSchedule(int [,,] a)
        {
            foreach (Group g in dayState[workDays])
            {
                for (int i = 0; i < g.subject2Teacher.Count; i++)
                {
                    if (g.getBottleneck(i) < g.getSubjectWeekLim(i)) return false;
                }
            }

            cntGenerated++;
            if (cntGenerated == 1) Console.WriteLine("found you!");
            if (cntGenerated % ((int)1e4) == 0) Console.WriteLine(cntGenerated);

            List<DaySchedule> state = new List<DaySchedule>();
            foreach(Group g in dayState[workDays])
            {
                DaySchedule curr = new DaySchedule();
                
                curr.g = g;
                curr.curriculum = new List<Tuple<Subject, int>>();
                foreach(var x in g.weekLims)
                {
                    var help = g.subject2Teacher.FirstOrDefault(y => y.Item1.name == x.g.name);
                    
                    if (help == null) continue;
                    Subject s = help.Item1;

                    curr.curriculum.Add(Tuple.Create(s, x.cnt));
                }

                state.Add(curr);
            }

            sw.Restart();
            ScheduleCompleter sc = new ScheduleCompleter(state, teachers, maxLessons);
            string[,] lastDay = sc.gen();
            sw.Stop();

            lastDayTime += (int)sw.ElapsedMilliseconds;

            if (lastDay == null) return false;

            result = new string[workDays+1, maxLessons+1, teachers.Count];
            for (int d = 1; d < workDays; d++)
                for (int l = 1; l <= maxLessons; l++)
                    for (int t = 0; t < teachers.Count; t++)
                        result[d, l, t] = ((a[d, l, t]!= groups.Count + 1) ?groups[a[d, l, t]-1].name:"---");

            for (int lesson = 1; lesson <= maxLessons; lesson++)
                for (int t = 0; t < teachers.Count; t++)
                    result[workDays, lesson, t] = lastDay[lesson, t];

            return true;
        }

        private bool checkLesson(int day, int lesson)
        {
            int[] groupNode = new int[groups.Count];
            int[] teacherNode = new int[teachers.Count];

            int nodeCnt = 3;
            for (int g = 0; g < groups.Count; g++) {groupNode[g] = nodeCnt; nodeCnt++;}
            for (int t = 0; t < teachers.Count; t++) {teacherNode[t] = nodeCnt; nodeCnt++;}

            MaxFlowGraph G = new MaxFlowGraph(groups.Count + teachers.Count + 2, 1, 2);
            for (int g = 0;g<groups.Count;g++)
            {
                for(int t = 0;t<teachers.Count;t++)
                {
                    foreach (int s in groupTeacherMatches[g, t])
                    {
                        if (usedGroup[day, lesson, g] == false
                            && dayState[day][g].checkSubject(s) == true)
                        {
                            G.addEdge(groupNode[g], teacherNode[t], 1);
                            break;
                        }
                    }
                }
            }
            for (int g = 0; g < groups.Count; g++) G.addEdge(1, groupNode[g], 1);
            for (int t = 0; t < teachers.Count; t++) G.addEdge(teacherNode[t], 2, 1);

            int matching = (int)G.Dinic();
            if (matching != groups.Count) return false;

            return true;
        }

        private void gen(int day, int lesson, int teacherInd, int[,,] a, 
                         short[] lastLessonCode, bool lastLessonSmaller)
        {
            if (result != null) return;
            //if (day >= 4) Console.WriteLine($"{day} {lesson} {teacherInd}");

            if (teacherInd==teachers.Count) 
            {
                cntGenerated++;
                if (cntGenerated % (int)1e5 == 0) Console.WriteLine(cntGenerated);

                //return;

                //add а special skip condition
                for (int i = 0; i < groups.Count; i++)
                {
                    if (usedGroup[day, lesson, i] == false)
                    {
                        return;
                    }
                }

                lastLessonSmaller = false;
                for (int t = 0; t < teachers.Count; t++)
                    lastLessonCode[t] = (short)a[day, lesson, t];

                teacherInd = 0; 
                lesson++;

                /*
                if(lesson!=maxLessons+1)
                {
                    foreach (Group g in dayState[day])
                    {
                        int lessonsPossible = 0;
                        for (int i = 2; i < g.dayLims.Count; i++)
                        {
                            lessonsPossible += Math.Min(g.dayLims[i].cnt, g.weekLims[i].cnt);
                        }
                        if (lessonsPossible < maxLessons - lesson + 1) return;
                    }
                }
                */
                
                //if (lesson != maxLessons + 1 && checkLesson(day, lesson) == false) return;
            }

            /*
            if(lesson==maxLessons+1)
            {
                /*
                lastDaySmaller = false;
                for (int l = 1; l <= maxLessons; l++)
                     for (int t = 0; t < teachers.Count; t++)
                         lastDayCode[(l-1)*teachers.Count+t] = (short)a[day, l, t];
                */

                lesson = 1; 
                day++;

                /*
                if (day<=workDays)
                {
                    //redo later
                    foreach (Group g in dayState[day])
                    {
                        for (int s = 0;s < g.subject2Teacher.Count;s++)
                        {
                            int perDay = g.getSubjectDayLim(s);
                            int lessonsLeft = g.getSubjectWeekLim(s);

                            if ((workDays - day + 1) * perDay < lessonsLeft) return;
                        }

                        //redo later
                        int lessonsPossible = 0;
                        for (int i = 2; i < g.dayLims.Count; i++)
                        {
                            lessonsPossible += Math.Min(g.dayLims[i].cnt, g.weekLims[i].cnt);
                        }
                        if (lessonsPossible < maxLessons) return;
                    }

                    for(int t = 0;t<teachers.Count;t++)
                    {
                        int requested = 0;
                        foreach(Group g in dayState[day])
                        {
                            for(int s = 0;s<g.subject2Teacher.Count;s++)
                            {
                                if(g.subject2Teacher[s].Item2.name==teachers[t].name)
                                {
                                    requested += g.weekLims[g.subjectWeekSelf[s]].cnt;
                                    break;
                                }
                            }
                        }

                        if (requested > (workDays - day + 1) * maxLessons) return;
                    }
                }
                
            }
            if (day == workDays)
            {
                if (completeSchedule(a) == false) return;
                
                foreach (Group g in dayState[day - 1])
                {
                    Console.WriteLine(g.name);
                    foreach(var x in g.weekLims)
                        Console.WriteLine($"{x.g.name} - {x.cnt}");
                    Console.WriteLine();
                }
                printSchedule(result);

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();

                return;
            }
            */

            List<int> groupInds = new List<int>();
            for (int i = 0; i < groups.Count; i++) groupInds.Add(i);
            //Shuffle(groupInds);

            foreach (int g in groupInds)
            {
                foreach(int s in groupTeacherMatches[g, teacherInd])
                {
                    if (lastLessonSmaller == false && lesson != 1 && lastLessonCode[teacherInd] > g+1) continue;
                    //if (lastDaySmaller == false && day != 1 && lastDayCode[teachers.Count * (lesson - 1) + teacherInd] > g+1) continue;

                    if (usedGroup[day, lesson, g] == false
                        && dayState[day][g].checkSubject(s) == true)
                    {
                        usedGroup[day, lesson, g] = true;
                        a[day, lesson, teacherInd] = g + 1;
                        dayState[day][g].applySubject(s, +1);

                        bool newLastLessonSmaller = (lastLessonSmaller | (lastLessonCode[teacherInd] < g + 1));
                        //bool newLastDaySmaller = (lastDaySmaller | (lastDayCode[teachers.Count * (lesson - 1) + teacherInd] < g + 1));

                        gen(day, lesson, teacherInd + 1, a, lastLessonCode, newLastLessonSmaller);

                        dayState[day][g].applySubject(s, -1);
                        a[day, lesson, teacherInd] = 0;
                        usedGroup[day, lesson, g] = false;
                    }
                }
            }

            if(teacherFreeLesons[teacherInd]>0
                && (!(lastLessonSmaller == false && lesson != 1 && lastLessonCode[teacherInd] > groups.Count + 1)))
                //&& (!(lastDaySmaller == false && day != 1 && lastDayCode[teachers.Count * (lesson - 1) + teacherInd] > 0)))
            {
                teacherFreeLesons[teacherInd]--;
                a[day, lesson, teacherInd] = groups.Count + 1;

                bool newLastLessonSmaller = (lastLessonSmaller | (lastLessonCode[teacherInd] < groups.Count + 1));
                //bool newLastDaySmaller = (lastDaySmaller | (lastDayCode[teachers.Count * (lesson - 1) + teacherInd] < 0));

                gen(day, lesson, teacherInd + 1, a, lastLessonCode, newLastLessonSmaller);
                
                a[day, lesson, teacherInd] = groups.Count + 1;
                teacherFreeLesons[teacherInd]++;
            }
            
        }

        public string[,,] generate()
        {
            int[,,] a = new int[workDays+1, maxLessons+1, teachers.Count];
            initGeneration();

            gen(1, 1, 0, a, new short[teachers.Count], false);
            Console.WriteLine($"cntGenerated = {cntGenerated}");
            Console.WriteLine($"lastDayTime = {lastDayTime}");

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

        private void initGroupTeacherInfo()
        {
            groupTeacherMatches = new List<int>[groups.Count, teachers.Count];
            for (int g = 0; g < groups.Count; g++)
            {
                for (int t = 0; t < teachers.Count; t++)
                {
                    groupTeacherMatches[g, t] = new List<int>();
                    for (int s = 0; s < groups[g].subject2Teacher.Count; s++)
                    {
                        if (groups[g].subject2Teacher[s].Item2 == teachers[t])
                            groupTeacherMatches[g, t].Add(s);
                    }
                }
            }

            teacherFreeLesons = new int[teachers.Count];
            for (int t = 0; t < teachers.Count; t++) teacherFreeLesons[t] = workDays * maxLessons;

            for (int g = 0; g < groups.Count; g++)
            {
                for (int t = 0; t < teachers.Count; t++)
                {
                    foreach (int s in groupTeacherMatches[g, t])
                        teacherFreeLesons[t] -= groups[g].getSubjectWeekLim(s);
                }
            }
        }

        private void orderTeachers()
        {
            List<int> help = new List<int>();//{ 5, 0, 2, 7, 6, 4, 3, 9, 1, 8, 10, 11, 12 };
            for (int t = 0; t < teachers.Count; t++) help.Add(t);

            for (int t = 0; t < teachers.Count; t++) Console.WriteLine(teacherFreeLesons[t]);
            help = help.OrderBy(x => teacherFreeLesons[x]).ToList();

            List<Teacher> teachersCpy = new List<Teacher>();
            foreach (Teacher t in teachers) teachersCpy.Add(t);

            teachers.Clear();
            foreach (int ind in help) teachers.Add(teachersCpy[ind]);

            Console.WriteLine(string.Join(" ", teachers.Select(x => x.name)));
        }

        private void initGeneration()
        {
            usedGroup = new bool[workDays + 1, maxLessons + 1, teachers.Count];
            for (int day = 1; day <= workDays; day++)
                for (int lesson = 1; lesson <= maxLessons; lesson++)
                    for (int groupInd = 0; groupInd < groups.Count; groupInd++)
                        usedGroup[day, lesson, groupInd] = false;

            initGroupTeacherInfo();
            orderTeachers();
            initGroupTeacherInfo();

            dayState = new List<Group>[workDays + 1];

            dayState[1] = groups.Select(x => x.CloneFull()).ToList();
            for (int i = 2; i <= workDays; i++)
            {
                dayState[i] = new List<Group>();
                for (int j = 0; j < dayState[1].Count; j++)
                {
                    dayState[i].Add(dayState[1][j].ClonePartial(dayState[1][j].weekLims));
                }
            }

            for (int t = 0; t < teachers.Count; t++)
                Console.WriteLine($"{teachers[t].name} - {teacherFreeLesons[t]}");
        }
    }
}
