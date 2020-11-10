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
        public string name { get; set; }

        public List<LimitationGroupPair> dayLims { get; set; }
        public List<LimitationGroupPair> weekLims { get; set; }
     
        public List<Tuple<Subject, Teacher>> subject2Teacher { get; set; }
        
        private List<int>[] subjectDayDependees, subjectWeekDependees;
        public int[] subjectDaySelf, subjectWeekSelf;

        public Group() { }
        public Group(string name, List<Tuple<LimitationGroup, int>> dayLims, 
                                  List<Tuple<LimitationGroup, int>> weekLims,
                                  List<Tuple<Subject, Teacher>> subject2Teacher)
        {
            this.name = name;
            this.dayLims = dayLims.Select(x => new LimitationGroupPair(x)).ToList();
            this.weekLims = weekLims.Select(x => new LimitationGroupPair(x)).ToList();
            this.subject2Teacher = subject2Teacher;

            this.subjectDayDependees = new List<int>[this.subject2Teacher.Count];
            this.subjectWeekDependees = new List<int>[this.subject2Teacher.Count];

            this.subjectDaySelf = new int[this.subject2Teacher.Count];
            this.subjectWeekSelf = new int[this.subject2Teacher.Count];

            for(int i = 0;i<subject2Teacher.Count;i++)
            {
                subjectDayDependees[i] = new List<int>();
                subjectWeekDependees[i] = new List<int>();

                foreach (LimitationGroup lg in subject2Teacher[i].Item1.limGroups)
                {
                    for (int j = 0; j < dayLims.Count; j++)
                    {
                        if (dayLims[j].Item1 == lg)
                        {
                            subjectDayDependees[i].Add(j);
                            if (dayLims[j].Item1.name == subject2Teacher[i].Item1.name) subjectDaySelf[i] = j;
                        }
                    }

                    for (int j = 0; j < weekLims.Count; j++)
                    {
                        if (weekLims[j].Item1 == lg)
                        {
                            subjectWeekDependees[i].Add(j);
                            if (weekLims[j].Item1.name == subject2Teacher[i].Item1.name) subjectWeekSelf[i] = j;
                        }
                    }
                }
            }
        }

        public Group CloneFull()
        {
            Group output = new Group();

            output.name = name;
            output.subject2Teacher = subject2Teacher;

            output.dayLims = dayLims.Select(x => x.Clone()).ToList();
            output.weekLims = weekLims.Select(x => x.Clone()).ToList();
            
            output.subjectDayDependees = subjectDayDependees;
            output.subjectWeekDependees = subjectWeekDependees;

            output.subjectDaySelf = subjectDaySelf;
            output.subjectWeekSelf = subjectWeekSelf;

            return output;
        }

        public Group ClonePartial(List <LimitationGroupPair> weekLimsKeep)
        {
            Group output = new Group();

            output.name = name;
            output.subject2Teacher = subject2Teacher;

            output.dayLims = dayLims.Select(x => x.Clone()).ToList();
            output.weekLims = weekLimsKeep;

            output.subjectDayDependees = subjectDayDependees;
            output.subjectWeekDependees = subjectWeekDependees;

            output.subjectDaySelf = subjectDaySelf;
            output.subjectWeekSelf = subjectWeekSelf;

            return output;
        }

        public int getBottleneck(int s)
        {
            int bottleneck = int.MaxValue;
            foreach (int ind in subjectDayDependees[s])
            {
                bottleneck = Math.Min(bottleneck, dayLims[ind].cnt);
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

        public void applySubject(int s, int sign)
        {
            foreach (int ind in subjectDayDependees[s])
            {
                dayLims[ind].cnt -= sign;
            }
            foreach (int ind in subjectWeekDependees[s])
            {
                weekLims[ind].cnt -= sign;
            }
        }
    }
}
