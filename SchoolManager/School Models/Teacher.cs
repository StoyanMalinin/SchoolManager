using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.School_Models
{
    class Teacher
    {
        public string name { get; set; }

        public List<Subject> subjects { get; set; }

        public Teacher() { }
        public Teacher(string name, List<Subject> subjects) 
        {
            this.name = name;
            this.subjects = subjects;
        }

        public Teacher Clone()
        {
            Teacher output = new Teacher(name, subjects);
            return output;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() == typeof(Teacher))
            {
                return name.Equals((obj as Teacher).name);
            }

            return false;
        }
    }
}
