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

        public Group() { }
        public Group(string name, List<Tuple<LimitationGroup, int>> dayLims, 
                                  List<Tuple<LimitationGroup, int>> weekLims,
                                  List<Tuple<Subject, Teacher>> subject2Teacher)
        {
            this.name = name;
            this.dayLims = dayLims;
            this.weekLims = weekLims;
            this.subject2Teacher = subject2Teacher;
        }

        public Group CloneFull()
        {
            Group output = new Group();

            output.name = name;
            output.subject2Teacher = subject2Teacher;

            output.dayLims = dayLims.Select(x => Tuple.Create(x.Item1, x.Item2)).ToList();
            output.weekLims = weekLims.Select(x => Tuple.Create(x.Item1, x.Item2)).ToList();

            return output;
        }

        public Group ClonePartial(List <Tuple<LimitationGroup, int>> weekLimsKeep)
        {
            Group output = new Group();

            output.name = name;
            output.subject2Teacher = subject2Teacher;

            output.dayLims = dayLims.Select(x => Tuple.Create(x.Item1, x.Item2)).ToList();
            output.weekLims = weekLimsKeep;

            return output;
        }

        public bool checkSubject(Subject s)
        {
            //optimize later
            foreach(LimitationGroup lg in s.limGroups)
            {
                foreach(var x in dayLims)
                {
                    if (x.Item1 == lg && x.Item2 == 0) return false; 
                }
                foreach (var x in weekLims)
                {
                    if (x.Item1 == lg && x.Item2 == 0) return false;
                }
            }

            return true;
        }

        public void applySubject(Subject s, int sign)
        {
            //optimize later
            foreach (LimitationGroup lg in s.limGroups)
            {
                for (int i = 0; i < dayLims.Count; i++)
                {
                    if (dayLims[i].Item1 == lg)
                        dayLims[i] = Tuple.Create(dayLims[i].Item1, dayLims[i].Item2 - sign);
                }
                for (int i = 0;i<weekLims.Count;i++)
                {
                    if (weekLims[i].Item1 == lg)
                        weekLims[i] = Tuple.Create(weekLims[i].Item1, weekLims[i].Item2 - sign);
                }
            }
        }
    }
}
