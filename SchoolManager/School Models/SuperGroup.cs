using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.School_Models
{
    class SuperGroup
    {
        public string name;

        public List<Tuple<Group, Subject>> groups;
        public List<Teacher> teachers;

        public int weekLessons;
        public List<int> requiredMultilessons;

        public SuperGroup() { }
        public SuperGroup(string name, List <Tuple <Group, Subject>> groups, List <Teacher> teachers, int weekLessons, List <int> requiredMultilessons)
        {
            this.name = name;
            this.groups = groups;
            this.teachers = teachers;
            this.weekLessons = weekLessons;
            this.requiredMultilessons = requiredMultilessons;
        }

        public SuperGroup Clone()
        {
            return new SuperGroup(name, groups, teachers, weekLessons, requiredMultilessons);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != typeof(SuperGroup)) return false;

            return name.Equals((obj as SuperGroup).name);
        }
    }
}
