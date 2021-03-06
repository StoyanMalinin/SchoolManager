﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic.FileIO;
using SchoolManager.MaxFlow;
using SchoolManager.ScheduleUtils;
using SchoolManager.School_Models;

namespace SchoolManager.Generation_utils
{
    class ScheduleCompleter
    {
        class TeacherList : IEquatable<TeacherList>
        {
            public int id { get; set; }
            public bool isGood { get; set; }
            public List<Tuple<int, Subject>> l { get; set; }

            public TeacherList() { }
            public TeacherList(int id, List<Tuple <int, Subject>> l, bool isGood)
            {
                this.isGood = isGood;
                this.id = id;
                this.l = l;
            }

            public override int GetHashCode()
            {
                long h = 0;
                long key = 1009, mod = (long)1e9 + 7;

                foreach (var x in l)
                {
                    h = (h * key + x.Item1) % mod;
                }

                return (int)h;
            }

            public bool Equals(TeacherList other)
            {
                if (other.l.Count != l.Count) return false;
                for (int i = 0; i < l.Count; i++)
                    if (l[i].Item1 != other.l[i].Item1)
                        return false;

                return true;
            }
        }

        private int maxLessons;

        private List<Group> state;
        private List<Teacher> teachers;
        private List<Tuple<SuperGroup, int>> supergroupMultilessons;

        public ScheduleCompleter() { }
        public ScheduleCompleter(List<Group> state, List<Teacher> teachers, List <Tuple <SuperGroup, int>> supergroupMultilessons, int maxLessons)
        {
            this.state = state;
            this.teachers = teachers;
            this.maxLessons = maxLessons;
            this.supergroupMultilessons = compressSGMultilesons(supergroupMultilessons.Select(x => Tuple.Create(x.Item1.Clone(), x.Item2)).ToList());
        }

        private List<Tuple<SuperGroup, int>> compressSGMultilesons(List <Tuple <SuperGroup, int>> l)
        {
            //Console.WriteLine($"%%{l.Count}");

            //redo later when we can differenciate better between SuperGroups
            l = l.OrderBy(x => x.Item1.name).ToList();

            List<Tuple<SuperGroup, int>> output = new List<Tuple<SuperGroup, int>>();
            for(int i = 0;i<l.Count;)
            {
                int startInd = i, sum = 0;
                for (; i < l.Count && l[startInd].Equals(l[i]); i++) sum += l[i].Item2;

                output.Add(Tuple.Create(l[startInd].Item1, sum));
                //Console.WriteLine($"{l[startInd].Item1.name} || {sum}");
            }

            return output;
        }

        private DaySchedule output = null;
        private int[,] lessonTeacher;
        private List<Tuple<int, Subject>>[] solution;

        private List<Tuple <int, Subject>>[] teacherList;
        private HashSet<TeacherList>[] teacherPermList;

        private int[] superTeacherInd;
        private List<int>[] teacherDependees;

        private bool[] isTeacherLocked;
        private bool[,] teacherPosLocked;

        private HashSet<TeacherList> genPerms(List<Tuple <int, Subject>> l)
        {
            List<Tuple<int, Subject>> curr = new List<Tuple<int, Subject>>();
            HashSet<TeacherList> ans = new HashSet<TeacherList>();

            bool[] used = new bool[l.Count];
            for (int i = 0; i < used.Length; i++) used[i] = false;

            int cnt = 0;
            void rec(int ind)
            {
                if (ind == l.Count)
                {
                    bool isGood = true;
                    for(int i = 0;i<curr.Count;i++)
                    {
                        bool diff = false;
                        for(int j = i+1;j<curr.Count;j++)
                        {
                            if (curr[i].Item1 != curr[j].Item1) diff = true;
                            if (curr[i].Item1 == curr[j].Item1 && diff == true)
                            {
                                isGood = false;
                                break;
                            }
                        }
                    }

                    cnt++;

                    List<Tuple<int, Subject>> cpy = new List<Tuple<int, Subject>>(curr.Select(x => x).ToList());
                    ans.Add(new TeacherList(cnt, cpy, isGood));

                    return;
                }

                for (int i = 0; i < l.Count; i++)
                {
                    if (used[i] == false)
                    {
                        used[i] = true;
                        curr.Add(l[i]);

                        rec(ind + 1);

                        used[i] = false;
                        curr.RemoveAt(curr.Count - 1);
                    }
                }
            }

            rec(0);
            return ans;
        }

        private bool checkSuitable(TeacherList tl, bool onlyConsequtive)
        {
            if (onlyConsequtive == true && tl.isGood == false) return false;

            bool fail = false;
            for (int lesson = 0; lesson < tl.l.Count; lesson++)
            {
                if (isTeacherLocked[tl.l[lesson].Item1] == true && teacherPosLocked[lesson, tl.l[lesson].Item1] == false)
                {
                    return false;
                }

                if (tl.l[lesson].Item1 < teachers.Count)
                {
                    if (lessonTeacher[lesson, tl.l[lesson].Item1] > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if(isTeacherLocked[tl.l[lesson].Item1]==false)
                    {
                        int sgInd = Enumerable.Range(0, supergroupMultilessons.Count).First(ind => superTeacherInd[ind] == tl.l[lesson].Item1);
                        foreach (Teacher t in supergroupMultilessons[sgInd].Item1.teachers)
                        {
                            int tInd = teachers.FindIndex(x => x.Equals(t) == true);
                            if (lessonTeacher[lesson, tInd] > 0)
                            {
                                return false;
                            }
                        }
                    }                        
                }
            }

            return true;
        }

        private void rec(int g, bool onlyConsequtive)
        {
            if (output != null) return;
            if (g == state.Count)
            {
                //Console.WriteLine("aideeee");

                output = new DaySchedule(solution.Where(x => (!(x is null))).Select(x => x.Select(y => ((y is null)?null:y.Item2)).ToList()).ToList(), 
                                         teachers, state, 
                                         solution.Where(x => (!(x is null))).Select(x => x.Select(y => ((y is null || y.Item1 < teachers.Count) ? null 
                                         : supergroupMultilessons[Enumerable.Range(0, supergroupMultilessons.Count)
                                                                            .First(ind => superTeacherInd[ind]==y.Item1)].Item1)).ToList()).ToList()
                                         , maxLessons);
                return;
            }

            for(int gInd = g+1;gInd<state.Count;gInd++)
            {
                if (teacherPermList[gInd].Any(tl => checkSuitable(tl, onlyConsequtive)==true)==false) return;
            }

            IEnumerable<TeacherList> teacherLists = teacherPermList[g].Where(tl => checkSuitable(tl, onlyConsequtive) == true);
            foreach (TeacherList tl in teacherLists)
            {
                solution[g] = tl.l;
                for (int lesson = 0; lesson < tl.l.Count; lesson++)
                {
                    if(tl.l[lesson].Item1>=teachers.Count)
                    {
                        if (lessonTeacher[lesson, tl.l[lesson].Item1] == 0)
                        {
                            teacherPosLocked[lesson, tl.l[lesson].Item1] = true;
                            isTeacherLocked[tl.l[lesson].Item1] = true;
                            
                            foreach (int t in teacherDependees[tl.l[lesson].Item1])
                                lessonTeacher[lesson, t]++;
                        }
                    }
                    lessonTeacher[lesson, tl.l[lesson].Item1]++;
                }
                    
                rec(g + 1, onlyConsequtive);

                solution[g] = null;
                for (int lesson = 0; lesson < tl.l.Count; lesson++)
                {
                    if (tl.l[lesson].Item1 >= teachers.Count)
                    {
                        if (lessonTeacher[lesson, tl.l[lesson].Item1] == 1)
                        {
                            teacherPosLocked[lesson, tl.l[lesson].Item1] = false;
                            isTeacherLocked[tl.l[lesson].Item1] = false;
                            
                            foreach (int t in teacherDependees[tl.l[lesson].Item1])
                                lessonTeacher[lesson, t]--;
                        }
                    }
                    lessonTeacher[lesson, tl.l[lesson].Item1]--;
                }
            }
        }

        private void init()
        {
            solution = new List<Tuple<int, Subject>>[teachers.Count];
            teacherList = new List<Tuple<int, Subject>>[state.Count];
            teacherPermList = new HashSet<TeacherList>[state.Count];
            lessonTeacher = new int[maxLessons + 1, teachers.Count + supergroupMultilessons.Count + 1];
            superTeacherInd = new int[supergroupMultilessons.Count];
            teacherDependees = new List<int>[teachers.Count + supergroupMultilessons.Count + 1];

            for (int l = 1; l <= maxLessons; l++)
                for (int t = 0; t < teachers.Count + supergroupMultilessons.Count + 1; t++)
                    lessonTeacher[l, t] = 0;

            teacherPosLocked = new bool[maxLessons+1, teachers.Count + supergroupMultilessons.Count + 1];
            for (int l = 0; l < maxLessons + 1; l++)
                for (int t = 0; t < teachers.Count + supergroupMultilessons.Count + 1; t++)
                    teacherPosLocked[l, t] = false;

            isTeacherLocked = new bool[teachers.Count + supergroupMultilessons.Count + 1];
            for (int t = 0; t < isTeacherLocked.Length; t++)
                isTeacherLocked[t] = false;

            for (int g = 0; g < state.Count; g++)
                teacherList[g] = new List<Tuple<int, Subject>>();

            for (int i = 0; i < supergroupMultilessons.Count; i++)
            {
                superTeacherInd[i] = teachers.Count + i;
                teacherDependees[superTeacherInd[i]] = supergroupMultilessons[i].Item1.teachers.Select(x => teachers.FindIndex(t => t.Equals(x))).ToList();

                for(int iter = 0;iter<supergroupMultilessons[i].Item2;iter++)
                    foreach (var item in supergroupMultilessons[i].Item1.groups)
                        teacherList[state.FindIndex(g => g.Equals(item.Item1) == true)].Add(Tuple.Create(superTeacherInd[i], item.Item2));
            }

            for (int g = 0; g < state.Count; g++)
            {
                for (int s = 0; s < state[g].subject2Teacher.Count; s++)
                {
                    if (state[g].subject2Teacher[s].Item2 is null) continue;

                    int cnt = state[g].curriculum.Count(x => x.Equals(state[g].subject2Teacher[s].Item1));
                    if (cnt == 0) continue;

                    int t = -1;
                    for (int i = 0; i < teachers.Count; i++)
                    {
                        if (teachers[i].name == state[g].subject2Teacher[s].Item2.name)
                        {
                            t = i;
                            break;
                        }
                    }

                    for (int iter = 0; iter < cnt; iter++)
                        teacherList[g].Add(Tuple.Create(t, state[g].subject2Teacher[s].Item1));
                }

                //Console.Write($"{g}: ");
                //Console.WriteLine(string.Join(" ", teacherList[g].Select(x => x.Item1).ToList()));
            }

            for (int g = 0; g < state.Count; g++)
            {
                teacherPermList[g] = genPerms(teacherList[g]);
                //Console.WriteLine($"{g}: {string.Join(" ", teacherList[g].Select(x => x.Item1))} || {teacherPermList[g].Count}");
            }
        }

        public DaySchedule gen(bool onlyConsequtive = false)
        {
            //Console.WriteLine("KKKKKKKKKKKKKKKKKKKKKKK");
            //Console.WriteLine(state.Count);
            init();

            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            
            rec(0, onlyConsequtive);
            //if (output != null) Console.WriteLine("opa naredihme gi");
            //else Console.WriteLine("opa ne gi naredihme");

            //Console.WriteLine($"Generation time = {sw.ElapsedMilliseconds}");
            //sw.Stop();


            return output;
        }
    }
}