using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.School_Models
{
    class Subject
    {
        public string name { get; set; }
        public List <LimitationGroup> limGroups { get; set; }

        public Subject() { }
        public Subject(string name, List<LimitationGroup> limGroups)
        {
            this.name = name;
            this.limGroups = limGroups;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Subject))
            {
                return name.Equals((obj as Subject).name);
            }

            return false;
        }
    }
}
