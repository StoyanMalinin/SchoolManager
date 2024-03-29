﻿using System;
using System.Linq;
using System.Collections.Generic;
using SchoolManager.School_Models;

namespace SchoolManager.Generation_utils.ScheduleCompleters.GeneticAlgorithm
{
    class ConfigurationState : ConfigurationStateBase
    {
        private int[] teacherLeftLessons;
        private static Random rnd = new Random(22);
        public  List<Tuple<int, Subject>>[] solution;

        public List<TeacherList> options;

        public ConfigurationState(ConfigurationState other) : base(other)
        {
            this.teacherLeftLessons = new int[other.teacherLeftLessons.GetLength(0)];
            Array.Copy(other.teacherLeftLessons, this.teacherLeftLessons, other.teacherLeftLessons.Length);

            this.options = other.options;

            this.solution = new List<Tuple<int, Subject>>[other.solution.GetLength(0)];
            Array.Copy(other.solution, this.solution, other.solution.Length);
        }
        public ConfigurationState(List<Group> state, List<Teacher> teachers, List<Tuple<SuperGroup, int>> supergroupMultilessons,
                                  bool onlyConsequtive, int maxLessons)
               : base(state, teachers, supergroupMultilessons, onlyConsequtive, maxLessons)
        {
            solution = new List<Tuple<int, Subject>>[state.Count];
            teacherLeftLessons = new int[teachers.Count];
            Array.Fill(teacherLeftLessons, 0);

            for(int g = 0;g<state.Count;g++)
            {
                updateTeacherLessons(teacherList[g], +1);
            }
        }

        public long getState(int g)
        {
            long stateVal = 0;

            const long key = 1019;
            const long mod = 67772998972500529;
            const long emptySymbol = key - 1;
            const long separatingSymbol = key - 2;

            stateVal = (stateVal * key + g + 1) % mod;
            for (int lesson = 0; lesson < maxLessons; lesson++)
            {
                for (int t = 0; t < teachers.Count + supergroupMultilessons.Count; t++)
                {
                    if(lastPosSeen[t]<g)
                        stateVal = (stateVal * key + emptySymbol + 1) % mod;
                    else
                        stateVal = (stateVal * key + Convert.ToInt64(lessonTeacher[lesson, t] != 0) + 1) % mod;
                }
            }

            return stateVal;
        }

        private void updateTeacherLessons(List<Tuple<int, Subject>> l, int change)
        {
            foreach (var item in l)
            {
                if (item.Item1 < teachers.Count) teacherLeftLessons[item.Item1] += change;
                else
                {
                    foreach (var t in teacherDependees[item.Item1])
                        teacherLeftLessons[t] += change;
                }
            }
        }

        public void prepareForMutations(int g)
        {
            options = teacherPermList[g].Where(tl => checkSuitable(tl, onlyConsequtive) == true).ToList();
        }

        private double? fitnessCache = null;
        public bool mutate(int g)
        {
            List<TeacherList> options = teacherPermList[g].Where(tl => checkSuitable(tl, onlyConsequtive) == true).ToList();
            if (options.Count == 0)
                return false;
            fitnessCache = null;

            TeacherList tl = options[rnd.Next(options.Count)];
            solution[g] = tl.l;
            applyPermution(tl);

            return true;
        }

        public new ConfigurationState Clone()
        {
            return new ConfigurationState(this);
        }

        public double fitness(int g)
        {
            if (fitnessCache != null)
                return fitnessCache.Value;

            double lessonGapSum = 0;
            for (int t = 0; t < teachers.Count; t++)
            {
                int cnt = 0;
                for (int l = 1; l <= maxLessons; l++)
                {
                    if (lessonTeacher[l, t] > 0)
                    {
                        lessonGapSum += Math.Pow(cnt, 3)*teacherLeftLessons[t];
                    }
                    else 
                        cnt++;
                }
                lessonGapSum += Math.Pow(cnt, 3)*teacherLeftLessons[t];
            }

            double furtherOptionsProduct = 1;
            for (int gInd = g + 1; gInd < state.Count; gInd++)
            {
                furtherOptionsProduct *= teacherPermList[gInd].Count(tl => checkSuitable(tl, onlyConsequtive) == true);
                if (furtherOptionsProduct == 0) return double.MinValue;
            }

            fitnessCache = furtherOptionsProduct + lessonGapSum;
            return fitnessCache.Value;
        }

        public override void applyPermution(TeacherList tl)
        {
            base.applyPermution(tl);
            updateTeacherLessons(tl.l, -1);
        }

        public override void undoPermutation(TeacherList tl)
        {
            base.undoPermutation(tl);
            updateTeacherLessons(tl.l, +1);
        }
    }
}
