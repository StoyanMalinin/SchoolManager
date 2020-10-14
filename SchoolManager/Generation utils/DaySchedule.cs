using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchoolManager.Generation_utils
{
    class DaySchedule
    {
        public Group g;
        public List<Tuple<Subject, int>> curriculum;

        public DaySchedule() { }
        public DaySchedule(Group g)
        {
            this.g = g;
            this.curriculum = new List<Tuple<Subject, int>>();

            this.curriculum = new List<Tuple<Subject, int>>();
            foreach (var x in g.weekLims)
            {
                var help = g.subject2Teacher.FirstOrDefault(y => y.Item1.name == x.g.name);

                if (help == null) continue;
                Subject s = help.Item1;

                this.curriculum.Add(Tuple.Create(s, 0));
            }
        }
        public DaySchedule(Group g, List <Tuple<Subject, int>> curriculum)
        {
            this.g = g;
            this.curriculum = curriculum;
        }

        //optimize later
        public void applySubject(int s, int sign)
        {
            g.applySubject(s, sign);
            for(int i = 0;i<curriculum.Count;i++)
            {
                if(curriculum[i].Item1.name==g.subject2Teacher[s].Item1.name)
                {
                    curriculum[i] = Tuple.Create(curriculum[i].Item1, curriculum[i].Item2 + sign);
                    break;
                }
            }
        }
    }
}
