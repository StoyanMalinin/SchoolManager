using SchoolManager.School_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using System.Reflection;
using System.IO;

namespace SchoolManager.ScheduleUtils
{
    class DaySchedule
    {
        public int maxLessons;
        public List<Teacher> teachers;
        public Subject[,] lessonTeacher2Subject;
        public Subject[,] lessonGroup2Subject;

        public Group[,] lessonTeacher2Group;
        public SuperGroup[,] lessonTeacher2SuperGroup; 

        public DaySchedule(List<Tuple<int, Subject>>[] solution, List<Group> state, List <Teacher> teachers, 
                           List<Tuple<SuperGroup, int>> supergroupMultilessons, 
                           Generation_utils.ScheduleCompleters.ConfigurationStateBase config,
                           int maxLessons) 
                           : this(solution.Where(x => (!(x is null))).Select(x => x.Select(y => ((y is null) ? null : y.Item2)).ToList()).ToList(),
                           teachers, state,
                           solution.Where(x => (!(x is null))).Select(x => x.Select(y => ((y is null || y.Item1 < teachers.Count) ? null
                           : supergroupMultilessons[Enumerable.Range(0, supergroupMultilessons.Count)
                                                              .First(ind => config.superTeacherInd[ind] == y.Item1)].Item1)).ToList()).ToList()
                           , maxLessons)
        {

        }

        public DaySchedule(List <List<Subject>> orderedCurriculums, List <Teacher> teachers, List <Group> groups, 
                           List <List<SuperGroup>> orderedSuperGroupCurriculums, int maxLessons)
        {
            this.teachers = teachers;
            this.maxLessons = maxLessons;
            this.lessonGroup2Subject = new Subject[maxLessons+1, groups.Count];
            this.lessonTeacher2Subject = new Subject[maxLessons + 1, teachers.Count];
            
            this.lessonTeacher2Group = new Group[maxLessons + 1, teachers.Count];
            this.lessonTeacher2SuperGroup = new SuperGroup[maxLessons + 1, teachers.Count];

            for(int g = 0;g<orderedCurriculums.Count;g++)
            {
                for(int l = 0;l<orderedCurriculums[g].Count;l++)
                {
                    Subject s = orderedCurriculums[g][l];
                    
                    Teacher t = groups[g].subject2Teacher[groups[g].findSubject(s)].Item2;
                    int tInd = teachers.FindIndex(x => x.Equals(t));

                    lessonGroup2Subject[l+1, g] = orderedCurriculums[g][l];
                    if (tInd != -1)
                    {
                        lessonTeacher2Group[l + 1, tInd] = groups[g];
                        lessonTeacher2Subject[l + 1, tInd] = s;
                    }
                    else
                    {
                        foreach (Teacher item in orderedSuperGroupCurriculums[g][l].teachers)
                            lessonTeacher2SuperGroup[l+1, teachers.FindIndex(x => x.Equals(item))] = orderedSuperGroupCurriculums[g][l];
                    }
                    
                }
            }
        }
        public void print()
        { 
            Console.Write("   ");
            Console.WriteLine(string.Join(" ", teachers.Select(t => String.Format("{0, -12}", $"|{t.name}")).ToList()));
            for(int l = 1;l<=maxLessons;l++)
            {
                Console.Write(String.Format("{0, -3}", $"{l}: "));
                for (int t = 0; t < teachers.Count; t++)
                {
                    if(lessonTeacher2Group[l, t]!=null) Console.Write(String.Format("{0, -13}", $"|{lessonTeacher2Group[l, t].name}"));
                    else if(lessonTeacher2SuperGroup[l, t]!= null) Console.Write(String.Format("{0, -13}", $"|{lessonTeacher2SuperGroup[l, t].name}"));
                    else Console.Write(String.Format("{0, -13}", "|"));
                }

                Console.WriteLine();
            }
        }
    }
}
