using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.Generation_utils
{
    class ScheduleDayState
    {
        public List<Group> groups { get; }
        public int[] groupLeftLessons { get; }
        public int[] teacherLeftLessons { get; }

        public ScheduleDayState() { }
        public ScheduleDayState(List <Group> groups, List <Teacher> teachers, int maxLessons)
        {
            this.groups = groups;
            this.groupLeftLessons = new int[groups.Count];
            this.teacherLeftLessons = new int[teachers.Count];

            for (int i = 0; i < groups.Count; i++) this.groupLeftLessons[i] = maxLessons;
            for (int i = 0; i < teachers.Count; i++) this.teacherLeftLessons[i] = maxLessons;
        }

        public bool check(int g, int s, int t, int change)
        {
            if (groupLeftLessons[g] + change < 0 || teacherLeftLessons[t] + change < 0 || groups[g].checkSubject(s, change)==false) return false;
            return true;
        }

        public bool updateLimits(int g, int s, int t, int sign)
        {
            groupLeftLessons[g] -= sign;
            teacherLeftLessons[t] -= sign;
            bool updateRes = groups[g].applySubject(s, sign);

            if (groupLeftLessons[g] < 0 || teacherLeftLessons[t] < 0 || updateRes==false) return false;
            return true;
        }

        public bool updateLimitsNoTeacher(int g, int s, int sign)
        {
            groupLeftLessons[g] -= sign;
            bool updateRes = groups[g].applySubject(s, sign);

            if (groupLeftLessons[g] < 0 || updateRes==false) return false;
            return true;
        }

        public bool updateTeacherLimits(int t, int sign)
        {
            teacherLeftLessons[t] -= sign;

            if (teacherLeftLessons[t] < 0) return false;
            return true;
        }
    }
}
