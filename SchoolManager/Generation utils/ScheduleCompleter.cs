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
using SchoolManager.School_Models;

namespace SchoolManager.Generation_utils
{
    class ScheduleCompleter
    {
        class TeacherList : IEquatable<TeacherList>
        {
            public int id { get; set; }
            public List<int> l { get; set; }

            public TeacherList() { }
            public TeacherList(int id, List<int> l)
            {
                this.id = id;
                this.l = l;
            }

            public override int GetHashCode()
            {
                long h = 0;
                long key = 1009, mod = (long)1e9 + 7;

                foreach (int x in l)
                {
                    h = (h * key + x) % mod;
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
            this.state = state.Select(x => x.CloneFull()).ToList();
            this.teachers = teachers.Select(x => x.Clone()).ToList();
        }

        private string[,] output = null;
        private bool[,] lessonTeacher;
        private List<int>[] solution;

        private List<int>[] teacherList;
        private HashSet<TeacherList>[] teacherPermList;

        private HashSet<TeacherList> genPerms(List<int> l)
        {
            List<int> curr = new List<int>();
            HashSet<TeacherList> ans = new HashSet<TeacherList>();

            bool[] used = new bool[l.Count];
            for (int i = 0; i < used.Length; i++) used[i] = false;

            int cnt = 0;
            void rec(int ind)
            {
                if (ind == l.Count)
                {
                    cnt++;

                    List<int> cpy = new List<int>(curr.Select(x => x).ToList());
                    ans.Add(new TeacherList(cnt, cpy));

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

        private void rec(int g)
        {
            if (output != null) return;
            if (g == state.Count)
            {
                output = new string[maxLessons + 1, teachers.Count];
                for (int lesson = 1; lesson <= maxLessons; lesson++)
                    for (int t = 0; t < teachers.Count; t++)
                        output[lesson, t] = "---";

                for (int i = 0; i < state.Count; i++)
                {
                    for (int lesson = 0; lesson < solution[i].Count; lesson++)
                    {
                        output[lesson + 1, solution[i][lesson]] = state[i].name;
                    }
                }

                return;
            }

            foreach (TeacherList tl in teacherPermList[g])
            {
                bool fail = false;
                for (int lesson = 0; lesson < tl.l.Count; lesson++)
                {
                    if (lessonTeacher[lesson, tl.l[lesson]] == true)
                    {
                        fail = true;
                        break;
                    }
                }
                if (fail == true) break;

                solution[g] = tl.l;
                for (int lesson = 0; lesson < tl.l.Count; lesson++)
                    lessonTeacher[lesson, tl.l[lesson]] = true;

                rec(g + 1);

                solution[g] = null;
                for (int lesson = 0; lesson < tl.l.Count; lesson++)
                    lessonTeacher[lesson, tl.l[lesson]] = false;
            }
        }

        public string[,] gen()
        {
            solution = new List<int>[teachers.Count];
            teacherList = new List<int>[state.Count];
            teacherPermList = new HashSet<TeacherList>[state.Count];
            lessonTeacher = new bool[maxLessons + 1, teachers.Count];

            for (int l = 1; l <= maxLessons; l++)
                for (int t = 0; t < teachers.Count; t++)
                    lessonTeacher[l, t] = false;

            for (int g = 0; g < state.Count; g++)
            {
                teacherList[g] = new List<int>();
                for (int s = 0; s < state[g].subject2Teacher.Count; s++)
                {
                    if (state[g].weekLims[state[g].subjectWeekSelf[s]].cnt == 0) continue;

                    int t = -1;
                    for (int i = 0; i < teachers.Count; i++)
                    {
                        if (teachers[i].name == state[g].subject2Teacher[s].Item2.name)
                        {
                            t = i;
                            break;
                        }
                    }

                    for (int iter = 0; iter < state[g].weekLims[state[g].subjectWeekSelf[s]].cnt; iter++)
                        teacherList[g].Add(t);
                }
            }

            for (int g = 0; g < state.Count; g++)
            {
                teacherPermList[g] = genPerms(teacherList[g]);

                //Console.WriteLine(string.Join(", ", teacherList[g]));
                //foreach (var l in teacherPermList[g]) Console.WriteLine(string.Join(" ", l));
            }

            rec(0);

            return output;
        }
    }
}