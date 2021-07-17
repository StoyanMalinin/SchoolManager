using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchoolManager.Generation_utils.ScheduleCompleters.DFS
{
    class ConfigurationState : ConfigurationStateBase
    {
        public TeacherSelection allTeachersSelected;
        public List<TeacherSelection> teacherSelections;

        public ConfigurationState(List<Group> state, List<Teacher> teachers, List<Tuple<SuperGroup, int>> supergroupMultilessons, 
                                  bool onlyConsequtive, int maxLessons) 
               : base(state, teachers, supergroupMultilessons, onlyConsequtive, maxLessons) 
        {
            this.teacherSelections = new List<TeacherSelection>();
            List<int> sortedTeachers = Enumerable.Range(0, teachers.Count).OrderByDescending(ind => teacherList
            .Sum(tl => tl.Count(x => ((x.Item1 < teachers.Count && x.Item1 == ind) ||
            (x.Item1 >= teachers.Count))))).ToList();

            const int blocksSz = 15;
            for (int i = 0; i < teachers.Count; i += blocksSz)
            {
                List<int> l = new List<int>();
                for (int j = i; j < Math.Min(i + blocksSz, sortedTeachers.Count); j++) l.Add(sortedTeachers[j]);

                this.teacherSelections.Add(new TeacherSelection(teachers.Count, l));
            }

            this.allTeachersSelected = new TeacherSelection(teachers.Count, Enumerable.Range(0, teachers.Count).ToList());
        }

        public bool checkSuitable(TeacherList tl, bool onlyConsequtive, TeacherSelection ts = null)
        {
            if (onlyConsequtive == true && tl.isGood == false) return false;

            for (int lesson = 0; lesson < tl.l.Count; lesson++)
            {
                if (ts != null)
                {
                    if (tl.l[lesson].Item1 < teachers.Count)
                    {
                        if (ts.isSelected[tl.l[lesson].Item1] == false) continue;
                    }
                }

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

        public long getState(TeacherSelection ts, int g)
        {
            long stateVal = 0;

            const long key = 1019;
            const long mod = 67772998972500529;
            const long emptySymbol = key - 1;
            const long separatingSymbol = key - 2;

            stateVal = (stateVal * key + g + 1) % mod;
            for (int i = 0; i < ts.isSelected.Length; i++)
                stateVal = (stateVal * key + Convert.ToInt64(ts.isSelected[i]) + 1);
            stateVal = (stateVal * key + separatingSymbol) % mod;

            for (int lesson = 0; lesson < maxLessons; lesson++)
            {
                for (int t = 0; t < teachers.Count + supergroupMultilessons.Count; t++)
                {
                    if (lastPosSeen[t] < g || (t < teachers.Count && ts.isSelected[t] == false))
                        stateVal = (stateVal * key + emptySymbol) % mod;
                    else
                        stateVal = (stateVal * key + Convert.ToInt64(lessonTeacher[lesson, t] != 0) + 1) % mod;
                }
            }

            return stateVal;
        }

        public bool checkFailByTeacherSelections(int g, bool onlyConsequtive, long[] skeletonStates)
        {
            bool failsFound = false;
            for (int i = 0; i < teacherSelections.Count; i++)
            {
                skeletonStates[i] = getState(teacherSelections[i], g);
                if (teacherSelections[i].failedStates.Contains(skeletonStates[i]) == true)
                {
                    failsFound = true;
                    break;
                }

                bool fail = false;
                for (int gInd = g; gInd < state.Count; gInd++)
                {
                    if (teacherPermList[gInd].Any(tl => checkSuitable(tl, onlyConsequtive, teacherSelections[i]) == true) == false)
                    {
                        fail = true;
                        break;
                    }
                }

                if (fail == true)
                {
                    failsFound = true;
                    teacherSelections[i].failedStates.Add(skeletonStates[i]);

                    break;
                }
            }

            return failsFound;
        }

        public void loadTeacherSeletionBranches(int g, bool onlyConsequtive, long[] skeletonStates, HashSet<long>[] currSelectionBraches)
        {
            for (int i = 0; i < teacherSelections.Count; i++)
            {
                long stateVal = skeletonStates[i];
                TeacherSelection ts = teacherSelections[i];

                if (ts.branchesLeft.ContainsKey(stateVal) == false)
                {
                    ts.branchesLeft.Add(stateVal, new HashSet<long>());
                    currSelectionBraches[i] = ts.branchesLeft[stateVal];
                    IEnumerable<TeacherList> curr = teacherPermList[g].Where(tl => checkSuitable(tl, onlyConsequtive, ts) == true);

                    foreach (TeacherList tl in curr)
                    {
                        applyPermution(tl);

                        long branchState = getState(ts, g + 1);
                        currSelectionBraches[i].Add(branchState);

                        undoPermutation(tl);
                    }
                }
                else
                {
                    currSelectionBraches[i] = ts.branchesLeft[stateVal];
                }
            }
        }

        public void updateTeacherSelectionBranches(int g, bool[] failed, HashSet<long>[] currSelectionBraches)
        {
            for (int i = 0; i < teacherSelections.Count; i++)
            {
                if (failed[i] == true) continue;
                if (currSelectionBraches[i].Count == 0) continue;

                long branchState = getState(teacherSelections[i], g + 1);
                if (teacherSelections[i].failedStates.Contains(branchState) == true)
                {
                    currSelectionBraches[i].Remove(branchState);
                }
                else
                {
                    failed[i] = true;
                }
            }
        }
    }
}
