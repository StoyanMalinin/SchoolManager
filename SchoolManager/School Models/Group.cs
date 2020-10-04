using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchoolManager.School_Models
{
    class Group
    {
        public string name { get; set; }

        List<Tuple<LimitationGroup, int>> dayLims;
        List<Tuple<LimitationGroup, int>> weekLims;

        public Group() { }
        public Group(string name, List<Tuple<LimitationGroup, int>> dayLims, List<Tuple<LimitationGroup, int>> weekLims)
        {
            this.name = name;
            this.dayLims = dayLims;
            this.weekLims = weekLims;
        }

        public Group Clone()
        {
            Group output = new Group();

            output.name = name;
            output.dayLims = dayLims.Select(x => Tuple.Create(x.Item1, x.Item2)).ToList();
            output.weekLims = weekLims.Select(x => Tuple.Create(x.Item1, x.Item2)).ToList();

            return output;
        }
    }
}
