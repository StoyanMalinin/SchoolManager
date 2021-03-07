using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.Generation_utils
{
    class Multilesson
    {
        public Group g;
        public Teacher t;
        public Subject s;
        public IntInInterval val;

        public Multilesson() { }
        public Multilesson(Group g, Teacher t, Subject s, IntInInterval val)
        {
            this.g = g;
            this.t = t;
            this.s = s;
            this.val = val;
        }
    }
}
