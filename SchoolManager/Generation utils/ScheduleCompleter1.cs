using SchoolManager.ScheduleUtils;
using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchoolManager.Generation_utils
{
    class ScheduleCompleter1
    {
        private int maxLessons;
        private List<Group> state;
        private List<Teacher> teachers;

        private DaySchedule output = null;

        private bool[] teacherUsed;
        private int[] teacherDemand, lastTeacher, newTeacher;
        private List<Tuple<int, Subject>>[] teacherList;

        private List<List<Subject>> solution;

        public ScheduleCompleter1() { }
        public ScheduleCompleter1(List<Group> state, List<Teacher> teachers, int maxLessons)
        {
            this.maxLessons = maxLessons;
            this.teachers = teachers;
            this.state = state;
        }

        private void init()
        {
            teacherList = new List<Tuple<int, Subject>>[state.Count];
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
            }

            teacherDemand = new int[teachers.Count];
            for (int i = 0; i < teachers.Count; i++) teacherDemand[i] = 0;

            for (int g = 0; g < state.Count; g++)
                foreach (var item in teacherList[g])
                    teacherDemand[item.Item1]++;

            lastTeacher = new int[state.Count];
            for (int g = 0; g < state.Count; g++) lastTeacher[g] = -1;

            teacherUsed = new bool[teachers.Count];
            newTeacher = new int[state.Count];

            solution = new List<List<Subject>>();
            for (int g = 0; g < state.Count; g++)
            {
                solution.Add(new List<Subject>());
            }

            for (int g = 0; g < state.Count; g++)
            {
                //teacherList[g] = teacherList[g].OrderBy(x => teacherList[g].Count(y => y.Item1 == x.Item1)).ToList();
            }
        }

        public DaySchedule gen()
        {
            bool check(int t, int g)
            {
                return (t != -1 && teacherUsed[t] == false && teacherDemand[t]>0 
                        && teacherList[g].Any(x => x.Item1 == t)==true);
            }

            init();

            for(int lesson = 1;lesson<=maxLessons;lesson++)
            {
                for (int t = 0; t < teachers.Count; t++)
                {
                    teacherUsed[t] = false;
                }
                for(int g = 0;g<state.Count;g++)
                {
                    newTeacher[g] = -1;
                }

                for (int g = 0; g < state.Count; g++)
                {
                    continue;
                    if (newTeacher[g] == -1 && check(lastTeacher[g], g) == true)
                    {
                        newTeacher[g] = lastTeacher[g];

                        teacherDemand[newTeacher[g]]--;
                        teacherUsed[newTeacher[g]] = true;
                    }
                }

                List<int> groupInds = new List<int>();
                for (int g = 0; g < state.Count; g++) groupInds.Add(g);
                groupInds = groupInds.OrderByDescending(ind => (teacherList[ind].Min(x => teacherDemand[x.Item1]))).ToList();

                foreach (int g in groupInds)
                {
                    if (newTeacher[g] != -1) continue;
                    if (teacherList[g].Count == 0) continue;
                    
                    var res = teacherList[g].Where(x => check(x.Item1, g)==true).OrderByDescending(x => teacherDemand[x.Item1]).FirstOrDefault();
                    if(!(res is null))
                    {
                        newTeacher[g] = res.Item1;

                        teacherDemand[newTeacher[g]]--;
                        teacherUsed[newTeacher[g]] = true;
                    }

                    if (newTeacher[g] == -1) throw new Exception();
                }

                Console.WriteLine(string.Join(" ", newTeacher));
                for(int g = 0;g<state.Count;g++)
                {
                    Subject s = null;
                    Subject lastSubject = ((solution[g].Count == 0) ? null : solution[g][solution[g].Count - 1]);

                    var res = teacherList[g].FirstOrDefault(x => x.Item1 == newTeacher[g] && x.Item2 == lastSubject);
                    if (!(res is null)) s = res.Item2;
                    else
                    {
                        res = teacherList[g].FirstOrDefault(x => x.Item1 == newTeacher[g]);
                        if (!(res is null)) s = res.Item2;
                    }
                            

                    if (!(s is null))
                    {
                        solution[g].Add(s);

                        lastTeacher[g] = newTeacher[g];
                        teacherList[g].Remove(Tuple.Create(newTeacher[g], s));
                    }   
                }

                //output = new DaySchedule(solution, teachers, state, maxLessons);
            }

            return output;
        }
    }
}
