using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SchoolManager.Generation_utils
{
    class ScheduleGenerator
    {
        const int wordDays = 5;
        const int maxLessons = 7;

        List<Group> groups;
        List<Teacher> teachers;
        List<Subject> subjects;

        public ScheduleGenerator() { }
        public ScheduleGenerator(List<Group> groups, List <Teacher> teachers, List <Subject> subjects)
        {
            this.groups = groups;
            this.teachers = teachers;
            this.subjects = subjects;
        }

        private void gen(int day, int lesson, int teacherInd, List <Group> currState, Group[,] a)
        {
            //TODO: corner cases

            for(int i = 0;i<groups.Count;i++)
            {

            }
        }

        public Group[,] generate()
        {
            Group[,] a = new Group[maxLessons*wordDays, teachers.Count];
            gen(1, 1, 0, groups.Select(x => x.Clone()).ToList(), a);

            return a;
        }
    }
}
