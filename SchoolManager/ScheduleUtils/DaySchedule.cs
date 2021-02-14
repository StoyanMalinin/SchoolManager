using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchoolManager.ScheduleUtils
{
    class DaySchedule
    {
        private int maxLessons;
        private List<Teacher> teachers;
        public Group[,] lessonTeacher2Group;
        public Subject[,] lessonTeacher2Subject;
        public Subject[,] lessonGroup2Subject;

        public DaySchedule(List <List<Subject>> orderedCurriculums, List <Teacher> teachers, List <Group> groups, int maxLessons)
        {
            this.teachers = teachers;
            this.maxLessons = maxLessons;
            this.lessonGroup2Subject = new Subject[maxLessons+1, groups.Count];
            this.lessonTeacher2Group = new Group[maxLessons + 1, teachers.Count];
            this.lessonTeacher2Subject = new Subject[maxLessons + 1, teachers.Count];

            for(int g = 0;g<orderedCurriculums.Count;g++)
            {
                for(int l = 0;l<orderedCurriculums[g].Count;l++)
                {
                    Subject s = orderedCurriculums[g][l];
                    
                    Teacher t = groups[g].subject2Teacher[groups[g].findSubject(s)].Item2;
                    int tInd = teachers.FindIndex(x => x.Equals(t));

                    lessonGroup2Subject[l+1, g] = orderedCurriculums[g][l];
                    lessonTeacher2Group[l+1, tInd] = groups[g];
                    lessonTeacher2Subject[l+1, tInd] = s;
                }
            }
        }
        public void print()
        { 
            Console.Write("     ");
            Console.WriteLine(string.Join(" ", teachers.Select(t => String.Format("{0, -14}", $"|{t.name}")).ToList()));
            for(int l = 1;l<=maxLessons;l++)
            {
                Console.Write(String.Format("{0, -5}", $"{l}: "));
                for (int t = 0; t < teachers.Count; t++)
                {
                    if(lessonTeacher2Group[l, t]!=null) Console.Write(String.Format("{0, -15}", $"|{lessonTeacher2Group[l, t].name}"));
                    else Console.Write(String.Format("{0, -15}", "|"));
                }

                Console.WriteLine();
            }
        }
    }
}
