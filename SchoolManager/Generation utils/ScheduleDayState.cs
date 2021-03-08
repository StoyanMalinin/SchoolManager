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

        public bool check(int g, int s, int t)
        {
            if (groupLeftLessons[g] == 0 || teacherLeftLessons[t] == 0 || groups[g].getSubjectDayLim(s) == 0) return false;
            return true;
        }

        public bool updateLimits(int g, int s, int t, int sign)
        {
            groupLeftLessons[g] -= sign;
            teacherLeftLessons[t] -= sign;
            groups[g].applySubject(s, sign);

            if (groupLeftLessons[g] < 0 || teacherLeftLessons[t] < 0 || groups[g].getSubjectDayLim(s) < 0) return false;
            return true;
        }
    }
}
