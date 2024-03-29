﻿using SchoolManager.Generation_utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SchoolManager.School_Models
{
    class LimitationGroupPair
    {
        public LimitationGroup g;
        public int cnt;

        public LimitationGroupPair() { }
        public LimitationGroupPair(LimitationGroup g, int cnt)
        {
            this.g = g;
            this.cnt = cnt;
        }
        public LimitationGroupPair(Tuple<LimitationGroup, int> x)
        {
            this.g = x.Item1;
            this.cnt = x.Item2;
        }

        public LimitationGroupPair Clone()
        {
            return new LimitationGroupPair(g, cnt);
        }
    }

    class Group
    {
        public int minLessons;
        public string name;

        private List<LimitationGroupPair> dayLims;
        public List<LimitationGroupPair> weekLims { get; private set; }
        public List<Tuple<Subject, Teacher>> subject2Teacher;

        public List<Multilesson> requiredMultilessons;

        private List<int>[] subjectDayDependees, subjectWeekDependees;
        private int[] subjectDaySelf, subjectWeekSelf;

        public List<Subject> curriculum; 

        public Group() { }
        public Group(int minLessons, string name, List<Tuple<LimitationGroup, int>> dayLims,
                                                  List<Tuple<LimitationGroup, int>> weekLims,
                                                  List<Tuple<Subject, Teacher>> subject2Teacher)
        {
            this.name = name;
            this.minLessons = minLessons;
            this.curriculum = new List<Subject>();
            this.dayLims = dayLims.Select(x => new LimitationGroupPair(x)).ToList();
            this.weekLims = weekLims.Select(x => new LimitationGroupPair(x)).ToList();
            this.subject2Teacher = subject2Teacher;

            this.subjectDayDependees = new List<int>[this.subject2Teacher.Count];
            this.subjectWeekDependees = new List<int>[this.subject2Teacher.Count];

            this.subjectDaySelf = new int[this.subject2Teacher.Count];
            this.subjectWeekSelf = new int[this.subject2Teacher.Count];

            for (int i = 0; i < subject2Teacher.Count; i++)
            {
                subjectDayDependees[i] = new List<int>();
                subjectWeekDependees[i] = new List<int>();

                foreach (LimitationGroup lg in subject2Teacher[i].Item1.limGroups)
                {
                    for (int j = 0; j < dayLims.Count; j++)
                    {                        
                        if (dayLims[j].Item1.Equals(lg) == true)
                        {
                            subjectDayDependees[i].Add(j);
                            if (dayLims[j].Item1.name == subject2Teacher[i].Item1.name)
                            {
                                subjectDaySelf[i] = j;
                            }
                        }
                    }

                    for (int j = 0; j < weekLims.Count; j++)
                    {
                        if (weekLims[j].Item1.Equals(lg)==true)
                        {
                            subjectWeekDependees[i].Add(j);
                            if (weekLims[j].Item1.name == subject2Teacher[i].Item1.name) subjectWeekSelf[i] = j;
                        }
                    }
                }
            }

            this.requiredMultilessons = new List<Multilesson>();
        }
        public Group(int minLessons, string name, List<Tuple<LimitationGroup, int>> dayLims,
                                     List<Tuple<LimitationGroup, int>> weekLims,
                                     List<Tuple<Subject, Teacher>> subject2Teacher, 
                                     List <Multilesson> requiredMultilessons) : this(minLessons, name, dayLims, weekLims, subject2Teacher)
        {
            this.requiredMultilessons = requiredMultilessons;
        }

        public int getSubjectDayLim(int s)
        {
            return dayLims[subjectDaySelf[s]].cnt;
        }

        public int getSubjectWeekLim(int s)
        {
            return weekLims[subjectWeekSelf[s]].cnt;
        }

        public int getLimGroupDayLim(LimitationGroup lg)
        {
            var x = dayLims.FirstOrDefault(item => item.g.Equals(lg)==true);
            
            if (x == null) return int.MaxValue;
            return x.cnt;
        }

        public int findSubject(Subject s)
        {
            for(int i = 0;i<subject2Teacher.Count;i++)
            {
                if (subject2Teacher[i].Item1.Equals(s) == true) 
                    return i;
            }

            return -1;
        }

        public Group CloneFull()
        {
            Group output = new Group();

            output.name = name;
            output.minLessons = minLessons;
            output.subject2Teacher = subject2Teacher;

            output.curriculum = curriculum.Select(x => x).ToList();
            output.dayLims = dayLims.Select(x => x.Clone()).ToList();
            output.weekLims = weekLims.Select(x => x.Clone()).ToList();
            
            output.subjectDayDependees = subjectDayDependees;
            output.subjectWeekDependees = subjectWeekDependees;

            output.subjectDaySelf = subjectDaySelf;
            output.subjectWeekSelf = subjectWeekSelf;

            output.requiredMultilessons = requiredMultilessons;

            return output;
        }

        public Group ClonePartial(List <LimitationGroupPair> weekLimsKeep)
        {
            Group output = new Group();

            output.name = name;
            output.minLessons = minLessons;
            output.subject2Teacher = subject2Teacher;

            output.dayLims = dayLims.Select(x => x.Clone()).ToList();
            output.curriculum = curriculum.Select(x => x).ToList();
            output.weekLims = weekLimsKeep;

            output.subjectDayDependees = subjectDayDependees;
            output.subjectWeekDependees = subjectWeekDependees;

            output.subjectDaySelf = subjectDaySelf;
            output.subjectWeekSelf = subjectWeekSelf;

            output.requiredMultilessons = requiredMultilessons;

            return output;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType()==typeof(Group))
            {
                return name.Equals((obj as Group).name);
            }

            return false;
        }

        public int getDayBottleneck(int s)
        {
            int bottleneck = int.MaxValue;
            foreach (int ind in subjectDayDependees[s])
            {
                bottleneck = Math.Min(bottleneck, dayLims[ind].cnt);
            }

            return bottleneck;
        }

        public int getWeekBottleneck(int s)
        {
            int bottleneck = int.MaxValue;
            foreach (int ind in subjectWeekDependees[s])
            {
                bottleneck = Math.Min(bottleneck, weekLims[ind].cnt);
            }

            return bottleneck;
        }

        public bool checkSubject(int s, int cnt = 1)
        {
            foreach(int ind in subjectDayDependees[s])
            {
                if (dayLims[ind].cnt < cnt) return false;
            }
            foreach (int ind in subjectWeekDependees[s])
            {
                if (weekLims[ind].cnt < cnt) return false;
            }

            return true;
        }

        public bool applySubject(int s, int sign)
        {
            if (sign == +1) curriculum.Add(subject2Teacher[s].Item1);
            else curriculum.Remove(subject2Teacher[s].Item1);

            //Console.WriteLine(subjectDayDependees[s].Count);
            foreach (int ind in subjectDayDependees[s])
            {
                dayLims[ind].cnt -= sign;
                if (dayLims[ind].cnt < 0) return false;
            }
            foreach (int ind in subjectWeekDependees[s])
            {
                weekLims[ind].cnt -= sign;
                if (weekLims[ind].cnt < 0) return false; 
            }

            return true;
        }
    }
}
