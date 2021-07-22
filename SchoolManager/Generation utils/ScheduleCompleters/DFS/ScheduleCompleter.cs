using System;
using System.Linq;
using System.Collections.Generic;
using SchoolManager.ScheduleUtils;
using SchoolManager.School_Models;

namespace SchoolManager.Generation_utils.ScheduleCompleters.DFS
{
    class ScheduleCompleter
    {
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
            //redo later when we can differenciate better between SuperGroups
            l = l.OrderBy(x => x.Item1.name).ToList();

            List<Tuple<SuperGroup, int>> output = new List<Tuple<SuperGroup, int>>();
            for(int i = 0;i<l.Count;)
            {
                int startInd = i, sum = 0;
                for (; i < l.Count && l[startInd].Equals(l[i]); i++) sum += l[i].Item2;

                output.Add(Tuple.Create(l[startInd].Item1, sum));
            }

            return output;
        }

        private DaySchedule output = null;
        private List<Tuple<int, Subject>>[] solution;
        private HashSet <long> reachedStates = new HashSet<long>();

        private void rec(int g, bool onlyConsequtive)
        {
            if (output != null) return;
            if (g == state.Count)
            {
                output = new DaySchedule(solution, state, teachers, supergroupMultilessons, config, maxLessons);
                return;
            }

            if (sw.ElapsedMilliseconds > 2 * 1000) return;

            long currState = config.getState(config.allTeachersSelected, g);
            if(reachedStates.Contains(currState)==true) return;
            reachedStates.Add(currState);

            long[] skeletonStates = new long[config.teacherSelections.Count];
            bool failsFound = config.checkFailByTeacherSelections(g, onlyConsequtive, skeletonStates);

            if(failsFound==true) return;
            for(int gInd = g+1;gInd<state.Count;gInd++)
            {
                if (config.teacherPermList[gInd].Any(tl => config.checkSuitable(tl, onlyConsequtive)==true)==false) return;
            }
            
            HashSet<long>[] currSelectionBraches = new HashSet<long>[config.teacherSelections.Count];
            List <TeacherList> teacherLists = config.teacherPermList[g].Where(tl => config.checkSuitable(tl, onlyConsequtive)==true).ToList();
            config.loadTeacherSeletionBranches(g, onlyConsequtive, skeletonStates, currSelectionBraches);
            
            bool[] failed = new bool[config.teacherSelections.Count];
            for(int i = 0;i<config.teacherSelections.Count;i++) failed[i] = false;

            foreach (TeacherList tl in teacherLists)
            {
                solution[g] = tl.l;
                config.applyPermution(tl);
                    
                rec(g + 1, onlyConsequtive);
                config.updateTeacherSelectionBranches(g, failed, currSelectionBraches);

                config.undoPermutation(tl);
                solution[g] = null;
            }

            for(int i = 0;i<config.teacherSelections.Count;i++)
            {
                if(currSelectionBraches[i].Count==0)
                {
                    config.teacherSelections[i].failedStates.Add(skeletonStates[i]);
                }
            }
        }

        private ConfigurationState config;

        private void init(bool onlyConsequtive)
        {
            solution = new List<Tuple<int, Subject>>[teachers.Count];
            config = new ConfigurationState(state, teachers, supergroupMultilessons, onlyConsequtive, maxLessons);

            sw = new System.Diagnostics.Stopwatch();
        }

        private static Dictionary<string, DaySchedule> calculated = new Dictionary<string, DaySchedule>();
        System.Diagnostics.Stopwatch sw;

        public DaySchedule gen(bool onlyConsequtive)
        {
            init(onlyConsequtive);
            
            Console.WriteLine(calculated.Count);
            string str = string.Join("|", Enumerable.Range(0, state.Count).Select(gInd => string.Join(" ", config.teacherList[gInd].Select(x => x.Item2.name))));
            if (calculated.ContainsKey(str) == true) return calculated[str];

            sw.Start();
            rec(0, onlyConsequtive);

            Console.WriteLine($"Generation time = {sw.ElapsedMilliseconds}");
            sw.Stop();

            calculated[str] = output;
            return output;
        }
    }
}