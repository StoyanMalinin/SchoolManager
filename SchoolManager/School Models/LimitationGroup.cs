using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.School_Models
{
    class LimitationGroup
    {
        public string name { get; set; }

        public LimitationGroup() { }
        public LimitationGroup(string name)
        {
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            if(obj is null) return false;

            if (obj.GetType() != typeof(LimitationGroup)) return false;
            return name.Equals((obj as LimitationGroup).name);
        }
    }

}
