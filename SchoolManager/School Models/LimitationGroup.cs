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
    }

}
