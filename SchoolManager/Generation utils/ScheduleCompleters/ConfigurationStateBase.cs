using System;
using System.Linq;
using System.Collections.Generic;
using SchoolManager.School_Models;


namespace SchoolManager.Generation_utils.ScheduleCompleters
{
    class ConfigurationStateBase
    {
        protected readonly int maxLessons;

        protected readonly List<Group> state;
        protected readonly bool onlyConsequtive;
        protected readonly List<Teacher> teachers;
        protected readonly List<Tuple<SuperGroup, int>> supergroupMultilessons;

        protected readonly int[] lastPosSeen;
        protected int[,] lessonTeacher;
        protected bool[] isTeacherLocked;
        protected bool[,] teacherPosLocked;

        public readonly int[] superTeacherInd;
        protected readonly List<int>[] teacherDependees;

        public readonly List<Tuple<int, Subject>>[] teacherList;
        public readonly IEnumerable<TeacherList>[] teacherPermList;

        public ConfigurationStateBase(List<Group> state, List<Teacher> teachers, List<Tuple<SuperGroup, int>> supergroupMultilessons, bool onlyConsequtive, int maxLessons)
        {
            this.maxLessons = maxLessons;
            this.onlyConsequtive = onlyConsequtive;

            this.state = state;
            this.teachers = teachers;
            this.supergroupMultilessons = supergroupMultilessons;

            teacherList = new List<Tuple<int, Subject>>[state.Count];
            teacherPermList = new IEnumerable<TeacherList>[state.Count];
            lessonTeacher = new int[maxLessons + 1, teachers.Count + supergroupMultilessons.Count + 1];
            superTeacherInd = new int[supergroupMultilessons.Count];
            teacherDependees = new List<int>[teachers.Count + supergroupMultilessons.Count + 1];

            for (int l = 1; l <= maxLessons; l++)
                for (int t = 0; t < teachers.Count + supergroupMultilessons.Count + 1; t++)
                    lessonTeacher[l, t] = 0;

            teacherPosLocked = new bool[maxLessons + 1, teachers.Count + supergroupMultilessons.Count + 1];
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

                for (int iter = 0; iter < supergroupMultilessons[i].Item2; iter++)
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
            }

            lastPosSeen = new int[teachers.Count + supergroupMultilessons.Count];
            for (int t = 0; t < lastPosSeen.Length; t++)
            {
                lastPosSeen[t] = -1;
                for (int g = state.Count - 1; g >= 0; g--)
                {
                    if (teacherList[g].Any(x => x.Item1 == t) == true)
                    {
                        lastPosSeen[t] = g;
                        break;
                    }
                    if (t < teachers.Count && teacherList[g].Where(x => x.Item1 >= teachers.Count).Any(x => teacherDependees[x.Item1].Contains(t) == true))
                    {
                        lastPosSeen[t] = g;
                        break;
                    }
                }

                if (t >= teachers.Count)
                    lastPosSeen[t] = Math.Max(lastPosSeen[t], teacherDependees[t].Max(x => lastPosSeen[x]));
            }

            for (int g = 0; g < state.Count; g++)
            {
                teacherPermList[g] = genPerms(teacherList[g]).Where(t => t.isGood == true || onlyConsequtive == false).ToList();
            }
        }

        private List<TeacherList> genPerms(List<Tuple<int, Subject>> l)
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
                    cnt++;

                    List<Tuple<int, Subject>> cpy = curr.Select(x => x).ToList();
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
            return ans.ToList();
        }

        public bool checkSuitable(TeacherList tl, bool onlyConsequtive)
        {
            if (onlyConsequtive == true && tl.isGood == false) return false;

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
                    if (isTeacherLocked[tl.l[lesson].Item1] == false)
                    {
                        foreach (int tInd in teacherDependees[tl.l[lesson].Item1])
                        {
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

        public virtual void applyPermution(TeacherList tl)
        {
            for (int lesson = 0; lesson < tl.l.Count; lesson++)
            {
                if (tl.l[lesson].Item1 >= teachers.Count)
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
        }

        public virtual void undoPermutation(TeacherList tl)
        {
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
}
