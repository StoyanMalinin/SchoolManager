using System;
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
                    if (l[i] != other.l[i])
                        return false;

                return true;
            }
        }

        private int maxLessons;

        private List<Group> state;
        private List<Teacher> teachers;

        public ScheduleCompleter() { }
        public ScheduleCompleter(List<Group> state, List<Teacher> teachers, int maxLessons)
        {
            this.maxLessons = maxLessons;
            this.teachers = teachers;
            this.state = state;
        }

        private DaySchedule output = null;
        private bool[,] lessonTeacher;
        private List<Tuple<int, Subject>>[] solution;

        private List<Tuple <int, Subject>>[] teacherList;
        private HashSet<TeacherList>[] teacherPermList;

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

        private void rec(int g, bool onlyConsequtive)
        {
            if (output != null) return;
            if (g == state.Count)
            {
                output = new DaySchedule(solution.Where(x => (!(x is null))).Select(x => x.Select(y => ((y is null)?null:y.Item2)).ToList()).ToList(), 
                                         teachers, state, maxLessons);
                return;
            }

            foreach (TeacherList tl in teacherPermList[g])
            {
                if (onlyConsequtive == true && tl.isGood == false) continue;

                bool fail = false;
                for (int lesson = 0; lesson < tl.l.Count; lesson++)
                {
                    if (lessonTeacher[lesson, tl.l[lesson].Item1] == true)
                    {
                        fail = true;
                        break;
                    }
                }
                if (fail == true) continue;

                solution[g] = tl.l;
                for (int lesson = 0; lesson < tl.l.Count; lesson++)
                    lessonTeacher[lesson, tl.l[lesson].Item1] = true;

                rec(g + 1, onlyConsequtive);

                solution[g] = null;
                for (int lesson = 0; lesson < tl.l.Count; lesson++)
                    lessonTeacher[lesson, tl.l[lesson].Item1] = false;
            }
        }

        public DaySchedule gen(bool onlyConsequtive = false)
        {
            //Console.WriteLine("KKKKKKKKKKKKKKKKKKKKKKK");
            //Console.WriteLine(state.Count);

            solution = new List<Tuple<int, Subject>>[teachers.Count];
            teacherList = new List<Tuple<int, Subject>>[state.Count];
            teacherPermList = new HashSet<TeacherList>[state.Count];
            lessonTeacher = new bool[maxLessons + 1, teachers.Count];

            for (int l = 1; l <= maxLessons; l++)
                for (int t = 0; t < teachers.Count; t++)
                    lessonTeacher[l, t] = false;

            for (int g = 0; g < state.Count; g++)
            {
                teacherList[g] = new List<Tuple<int, Subject>>();
                for (int s = 0; s < state[g].subject2Teacher.Count; s++)
                {
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
                Console.WriteLine($"{g} -> {string.Join(" ", teacherList[g].Select(x => x.Item1))}");
            }

            rec(0, onlyConsequtive);
            return output;
        }
    }
}