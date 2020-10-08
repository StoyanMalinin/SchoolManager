using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchoolManager.School_Models
{
    class Group
    {
        public string name { get; set; }

        public List<Tuple<LimitationGroup, int>> dayLims { get; set; }
        public List<Tuple<LimitationGroup, int>> weekLims { get; set; }
     
        public List<Tuple<Subject, Teacher>> subject2Teacher { get; set; }
        
        private List<int>[] subjectDayDependees, subjectWeekDependees;
        public int[] subjectDaySelf, subjectWeekSelf;

        public Group() { }
        public Group(string name, List<Tuple<LimitationGroup, int>> dayLims, 
                                  List<Tuple<LimitationGroup, int>> weekLims,
                                  List<Tuple<Subject, Teacher>> subject2Teacher)
        {
            this.name = name;
            this.dayLims = dayLims;
            this.weekLims = weekLims;
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

            output.dayLims = dayLims.Select(x => Tuple.Create(x.Item1, x.Item2)).ToList();
            output.weekLims = weekLims.Select(x => Tuple.Create(x.Item1, x.Item2)).ToList();
            
            output.subjectDayDependees = subjectDayDependees;
            output.subjectWeekDependees = subjectWeekDependees;

            output.subjectDaySelf = subjectDaySelf;
            output.subjectWeekSelf = subjectWeekSelf;

            return output;
        }

        public Group ClonePartial(List <Tuple<LimitationGroup, int>> weekLimsKeep)
        {
            Group output = new Group();

            output.name = name;
            output.subject2Teacher = subject2Teacher;

            output.dayLims = dayLims.Select(x => Tuple.Create(x.Item1, x.Item2)).ToList();
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
                bottleneck = Math.Min(bottleneck, dayLims[ind].Item2);
            }

            return bottleneck;
        }

        public bool checkSubject(int s)
        {
            foreach(int ind in subjectDayDependees[s])
            {
                if (dayLims[ind].Item2 == 0) return false;
            }
            foreach (int ind in subjectWeekDependees[s])
            {
                if (weekLims[ind].Item2 == 0) return false;
            }

            return true;
        }

        public void applySubject(int s, int sign)
        {
            foreach (int ind in subjectDayDependees[s])
            {
                dayLims[ind] = Tuple.Create(dayLims[ind].Item1, dayLims[ind].Item2 - sign);
            }
            foreach (int ind in subjectWeekDependees[s])
            {
                weekLims[ind] = Tuple.Create(weekLims[ind].Item1, weekLims[ind].Item2 - sign);
            }
        }
    }
}
