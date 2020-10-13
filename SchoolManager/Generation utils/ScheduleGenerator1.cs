using SchoolManager.MaxFlow;
using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;

namespace SchoolManager.Generation_utils
{
    class ScheduleGenerator1
    {
        const int workDays = 5;
        const int maxLessons = 5;

        List<Group> groups;
        List<Teacher> teachers;
        List<Subject> subjects;

        public ScheduleGenerator1() { }
        public ScheduleGenerator1(List<Group> groups, List<Teacher> teachers, List<Subject> subjects)
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

        private bool[] teacherDone;
        private int[] teacherFreeLesons;

        int lastDayTime = 0;
        Stopwatch sw = new Stopwatch();

        List <List <int>> separateGroups(List <int> groupInds)
        {
            bool[] used = new bool[groupInds.Count];
            for (int i = 0; i < groupInds.Count; i++) used[i] = false;

            List<List<int>> output = new List<List<int>>();

            void dfs(int i)
            {
                used[i] = true;
                output[output.Count - 1].Add(groupInds[i]);

                for(int j = 0;j<groupInds.Count;j++)
                {
                    if (used[j] == true) continue;

                    bool common = false;
                    for(int t = 0;t<teachers.Count;t++)
                    {
                        if (teacherDone[t] == true) continue;

                        if (groups[groupInds[i]].subject2Teacher.Any(x => x.Item2.name == teachers[t].name)
                           && groups[groupInds[j]].subject2Teacher.Any(x => x.Item2.name == teachers[t].name))
                        {
                            common = true;
                            break;
                        }
                    }

                    if (common == true) dfs(j);
                }
            }

            for(int i = 0;i<groupInds.Count;i++)
            {
                if(used[i]==false)
                {
                    output.Add(new List<int>());
                    dfs(i);
                }
            }

            //return new List<List<int>>() { groupInds };
            return output;
        }

        //redo later when an actual limgroups structuring is added
        private bool checkMatching(int teacherInd, List <int> groupInds)
        {
            int nodeInd = 3;
            int source = 1, sink = 2;

            int[,] timeNode = new int[workDays + 1, maxLessons+1];
            //int[,] dayLimNode = 
            
            for (int day = 1; day <= workDays; day++)
            {
                for (int lesson = 1; lesson <= maxLessons; lesson++)
                {
                    timeNode[day, lesson] = nodeInd;
                    nodeInd++;
                }
            }



            MaxFlowGraph G = new MaxFlowGraph(nodeInd+1, source, sink);
            

            return false;
        }

        int cntFailed = 0;
        List<List<Tuple<int, int>>> schedules = new List<List<Tuple<int, int>>>();

        private bool gen(int[,,] a, List<int> groupInds)
        {
            int teacherInd = -1;
            for (int t = 0; t < teachers.Count; t++)
            {
                if (teacherDone[t] == false)
                {
                    teacherInd = t;
                    break;
                }
            }
            if (teacherInd == -1)
            {
                for (int day = 1; day <= workDays; day++)
                {
                    Console.WriteLine($"day {day}");
                    for (int lesson = 1; lesson <= maxLessons; lesson++)
                    {
                        Console.Write($"{lesson}:");
                        for (int t = 0; t < teachers.Count; t++)
                            Console.Write($" {((a[day, lesson, t]!=groups.Count+1 && a[day, lesson, t] != 0) ?groups[a[day, lesson, t]-1].name:"---")}");

                        Console.WriteLine();
                    }
                }

                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);

                Console.WriteLine(schedules.Count);
                foreach(var l in schedules)
                {
                    Console.WriteLine(string.Join(" ", l.Select(x => $"[{x.Item1}, {x.Item2}]")));
                }

                while (true) ;
                return true;
            }

            bool finished = false;

            //use an actual matcing algorithm later
            bool doMatching(int posInd, List<Tuple<int, int>> schedulePos)
            {
                //Console.WriteLine($"{day} {lesson}");

                if(posInd==schedulePos.Count)
                {
                    finished = true;

                    bool success = true;
                    List<List<int>> groupClusters = separateGroups(groupInds);

                    foreach (List<int> l in groupClusters)
                    {
                        if(gen(a, l)==false)
                        {
                            success = false;
                            break;
                        }
                    }

                    return success;
                }

                int day = schedulePos[posInd].Item1;
                int lesson = schedulePos[posInd].Item2;

                bool res = false;
                foreach (int g in groupInds)
                {
                    if (usedGroup[day, lesson, g] == true) continue;
                    
                    foreach (int s in groupTeacherMatches[g, teacherInd])
                    {
                        if(dayState[day][g].checkSubject(s)==true)
                        {
                            usedGroup[day, lesson, g] = true;
                            a[day, lesson, teacherInd] = g + 1;
                            dayState[day][g].applySubject(s, +1);

                            res |= doMatching(posInd+1, schedulePos);

                            usedGroup[day, lesson, g] = false;
                            a[day, lesson, teacherInd] = 0;
                            dayState[day][g].applySubject(s, -1);

                            //if (res == true) break;
                        }
                    }
                }

                if (teacherFreeLesons[teacherInd] > 0)
                {
                    teacherFreeLesons[teacherInd]--;
                    a[day, lesson, teacherInd] = groups.Count + 1;

                    res |= doMatching(posInd+1, schedulePos);

                    a[day, lesson, teacherInd] = groups.Count + 1;
                    teacherFreeLesons[teacherInd]++;
                }

                return res;
            }

            List<Tuple<int, int>> schedulePos = new List<Tuple<int, int>>();

            for (int day = 1; day <= workDays; day++)
                for (int lesson = 1; lesson <= maxLessons; lesson++)
                    schedulePos.Add(Tuple.Create(day, lesson));
            Shuffle(schedulePos);

            teacherDone[teacherInd] = true;
            schedules.Add(schedulePos.Select(x => Tuple.Create(x.Item1, x.Item2)).ToList());

            bool res = doMatching(0, schedulePos);

            schedules.RemoveAt(schedules.Count - 1);
            teacherDone[teacherInd] = false;

            //Console.WriteLine($"{string.Join(" ", groupInds)} -> {teacherInd} {res}");
            /*
            for (int day = 1; day <= workDays; day++)
            {
                Console.WriteLine($"day {day}");
                for (int lesson = 1; lesson <= maxLessons; lesson++)
                {
                    Console.Write($"{lesson}:");
                    for (int t = 0; t < teachers.Count; t++)
                        Console.Write($" {((a[day, lesson, t] != groups.Count + 1 && a[day, lesson, t] != 0) ? groups[a[day, lesson, t] - 1].name : "---")}");

                    Console.WriteLine();
                }
            }
            */

            if (finished == false)
            {
                cntFailed++;
                if (cntFailed % (int)1e5 == 0)
                {
                    Console.WriteLine(cntFailed);
                    
                }
            }

            return res;
        }

        public string[,,] generate()
        {
            int[,,] a = new int[workDays + 1, maxLessons + 1, teachers.Count];
            initGeneration();

            List<int> groupInds = new List<int>();
            for (int g = 0; g < groups.Count; g++) groupInds.Add(g);

            sw.Start();
            gen(a, groupInds);
            Console.WriteLine($"cntGenerated = {cntGenerated}");
            Console.WriteLine($"lastDayTime = {lastDayTime}");

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
                        teacherFreeLesons[t] -= groups[g].weekLims[groups[g].subjectWeekSelf[s]].cnt;
                }
            }
        }

        private void orderTeachers()
        {
            List<int> help = new List<int>();//{ 12, 11, 10, 2, 3, 1, 4, 6, 0, 5, 7, 8, 9 };
            for (int t = 0; t < teachers.Count; t++) help.Add(t);

            Console.WriteLine($"**********{help.Count}");

            for (int t = 0; t < teachers.Count; t++) Console.WriteLine(teacherFreeLesons[t]);
            //help = help.OrderBy(x => teacherFreeLesons[x]).ToList();

            List<Teacher> teachersCpy = new List<Teacher>();
            foreach (Teacher t in teachers) teachersCpy.Add(t);

            teachers.Clear();
            foreach (int ind in help) teachers.Add(teachersCpy[ind]);

            Console.WriteLine(string.Join(" ", teachers.Select(x => x.name)));
        }

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

        private void initGeneration()
        {
            /*
            schedulePos = new List<Tuple<int, int>>();
            for (int day = 1; day <= workDays; day++)
                for (int lesson = 1; lesson <= maxLessons; lesson++)
                    schedulePos.Add(Tuple.Create(day, lesson));
            Shuffle(schedulePos);
            */

            usedGroup = new bool[workDays + 1, maxLessons + 1, teachers.Count];
            for (int day = 1; day <= workDays; day++)
                for (int lesson = 1; lesson <= maxLessons; lesson++)
                    for (int groupInd = 0; groupInd < groups.Count; groupInd++)
                        usedGroup[day, lesson, groupInd] = false;

            teacherDone = new bool[teachers.Count];
            for (int t = 0; t < teachers.Count; t++) teacherDone[t] = false;

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
