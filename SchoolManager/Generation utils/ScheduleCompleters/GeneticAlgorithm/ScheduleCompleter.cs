﻿using SchoolManager.ScheduleUtils;
using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchoolManager.Generation_utils.ScheduleCompleters.GeneticAlgorithm
{
    class ScheduleCompleter
    {
        private int maxLessons;

        private List<Group> state;
        private List<Teacher> teachers;
        private List<Tuple<SuperGroup, int>> supergroupMultilessons;

        private ConfigurationState baseConfig;

        public ScheduleCompleter() { }
        public ScheduleCompleter(List<Group> state, List<Teacher> teachers, List<Tuple<SuperGroup, int>> supergroupMultilessons, int maxLessons)
        {
            this.state = state;
            this.teachers = teachers;
            this.maxLessons = maxLessons;
            this.supergroupMultilessons = compressSGMultilesons(supergroupMultilessons.Select(x => Tuple.Create(x.Item1.Clone(), x.Item2)).ToList());

            this.baseConfig = new ConfigurationState(this.state, this.teachers, this.supergroupMultilessons, true, this.maxLessons);            
        }

        private List<Tuple<SuperGroup, int>> compressSGMultilesons(List<Tuple<SuperGroup, int>> l)
        {
            //redo later when we can differenciate better between SuperGroups
            l = l.OrderBy(x => x.Item1.name).ToList();

            List<Tuple<SuperGroup, int>> output = new List<Tuple<SuperGroup, int>>();
            for (int i = 0; i < l.Count;)
            {
                int startInd = i, sum = 0;
                for (; i < l.Count && l[startInd].Equals(l[i]); i++) sum += l[i].Item2;

                output.Add(Tuple.Create(l[startInd].Item1, sum));
            }

            return output;
        }

        public DaySchedule geneticAlgorithm(bool onlyConsequtive)
        {
            if (supergroupMultilessons.Count == 3)
            {

            }

            List<ConfigurationState> generation = new List<ConfigurationState>() { baseConfig };
            for (int g = 0; g < state.Count; g++)
            {
                //var watch = System.Diagnostics.Stopwatch.StartNew();
                List<ConfigurationState> newGeneration = new List<ConfigurationState>();
                foreach (ConfigurationState cs in generation)
                {
                    cs.prepareForMutations(g);
                    for (int iter = 0; iter < Math.Min(50, cs.options.Count*1.5); iter++)
                    {
                        var newElement = cs.Clone();
                        if(newElement.mutate(g)==true) newGeneration.Add(newElement);
                    }
                }
                //watch.Stop(); Console.WriteLine($"New gen time = {watch.ElapsedMilliseconds}");

                newGeneration = newGeneration.GroupBy(cs => cs.getState(g)).Select(x => x.First()).ToList();
                Console.WriteLine($"Group {g} -> {newGeneration.Count}");

                //watch.Restart();
                generation = newGeneration.OrderByDescending(cs => cs.fitness(g)).Where(cs => cs.fitness(g)!=double.MinValue).Take(50).ToList();
                //watch.Stop(); Console.WriteLine($"Nz i az kvo vreme = {watch.ElapsedMilliseconds}");

                //Console.WriteLine(string.Join(" ", generation.Select(cs => cs.fitness(g))));
            }

            if (generation.Count == 0) return null;

            var output = new DaySchedule(generation[0].solution, state, teachers, supergroupMultilessons, generation[0], maxLessons);
            return output;
        }

        private static Dictionary<string, DaySchedule> calculated = new Dictionary<string, DaySchedule>();
        public DaySchedule gen(bool onlyConsequtive)
        {
            string str = string.Join("|", Enumerable.Range(0, state.Count).Select(gInd => string.Join(" ", baseConfig.teacherList[gInd].Select(x => x.Item2.name))));
            if (calculated.ContainsKey(str) == true) return calculated[str];

            var output = geneticAlgorithm(onlyConsequtive);
            calculated[str] = output;

            return output;
        }
    }
}
