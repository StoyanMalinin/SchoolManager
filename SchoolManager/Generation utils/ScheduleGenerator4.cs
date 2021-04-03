using SchoolManager.ScheduleUtils;
using SchoolManager.School_Models;
using SchoolManager.School_Models.Higharchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchoolManager.Generation_utils
{
    class ScheduleGenerator4
    {
        const int workDays = 5;
        const int minLessons = 6, maxLessons = 7;

        private TreeNode higharchy;
        private List<Group> groups;
        private List<Subject> subjects;
        private List<Teacher> teachers;
        private List<SuperGroup> superGroups;
        private List<Multilesson>[] multilessons;

        public ScheduleGenerator4() { }
        public ScheduleGenerator4(List<Group> groups, List<Teacher> teachers,
                                  List<Subject> subjects, TreeNode higharchy,
                                  List <SuperGroup> superGroups)
        {
            this.groups = groups;
            this.teachers = teachers;
            this.subjects = subjects;
            this.higharchy = higharchy;
            this.superGroups = superGroups;

            this.multilessons = new List<Multilesson>[workDays + 1];
            for (int day = 1; day <= workDays; day++)
                this.multilessons[day] = new List<Multilesson>();
        }
        public ScheduleGenerator4(List<Group> groups, List<Teacher> teachers,
                                  List<Subject> subjects, TreeNode higharchy, List<Multilesson>[] multilessons,
                                  List <SuperGroup> superGroups) : this(groups, teachers, subjects, higharchy, superGroups)
        {
            this.multilessons = multilessons;
        }

        private List<Multilesson> allMultilessons;
        private bool[] usedMultilessons;

        private WeekSchedule ans = null;
        private ScheduleDayState[] dayState;

        private int[] teacherDemand;
        private List <int>[] supergroup2TeacherInds;
        private List <Tuple<int, int>>[] supergroup2GroupSubjetInds;

        bool [][] supergroupMultilessonsAdded;
        List<Tuple<SuperGroup, int>>[] supergroupMultilessons = new List<Tuple<SuperGroup, int>>[workDays + 1];

        private bool arrangeMultilessons(int day, int ind)
        {
            while(day!=workDays+1 && ind==allMultilessons.Count)
            {
                ind = 0;
                day++;
            }
            if (day == workDays + 1)
            {
                int cnt = 0;
                for(int i = 0;i<usedMultilessons.Length;i++)
                {
                    if (usedMultilessons[i] == true) cnt++;
                }

                System.Console.WriteLine($"---------------------------------------------------managed to achieve: {cnt}");
                if(cnt!=usedMultilessons.Length) return false;
                
                ScheduleGenerator3 sg = new ScheduleGenerator3(groups, teachers, subjects, higharchy, multilessons, supergroupMultilessons);
                WeekSchedule sgRes = sg.gen(workDays, true);
                if (sgRes is null) return false;

                ans = sgRes;
                return true;
            }

            for(int i = 0;i<allMultilessons.Count;i++)
            {
                if(usedMultilessons[i]==true) continue;

                bool fitted = false;
                for(int d = day;d<=workDays;d++)
                {
                    fitted |= applyMultilesson(d, i, allMultilessons[i].val.l, +1);
                    applyMultilesson(d, i, allMultilessons[i].val.l, -1);
                }

                if(fitted==false) return false;
            }

            bool res = false;
            if (usedMultilessons[ind] == false)
            {
                usedMultilessons[ind] = true;
                multilessons[day].Add(allMultilessons[ind]);

                bool successful = applyMultilesson(day, ind, allMultilessons[ind].val.l, +1);

                if(successful==true)
                {
                    ScheduleGenerator3 sg = new ScheduleGenerator3(groups, teachers, subjects, higharchy, multilessons, supergroupMultilessons);
                    if (sg.gen(workDays, true) != null)
                    {
                        res |= arrangeMultilessons(day, ind + 1);
                    }
                }
                

                applyMultilesson(day, ind, allMultilessons[ind].val.l, -1);

                multilessons[day].RemoveAt(multilessons[day].Count - 1);
                usedMultilessons[ind] = false;

                if(res==true) return true;
            }
            
            if((usedMultilessons[ind]==true || day!=workDays) && arrangeMultilessons(day, ind+1)==true)
            {
                res = true;
                return true;                
            }

            System.Console.WriteLine($"em pone {usedMultilessons.Count(x => x==true)}");
            return false;
        }

        bool applySupergroupMultilesson(int day, int sgInd, int lessonsCnt, int sign)
        {
            bool successful = true;
            
            if(sign==+1)
            {
                superGroups[sgInd].weekLessons -= lessonsCnt;
                supergroupMultilessons[day].Add(Tuple.Create(superGroups[sgInd], lessonsCnt));
            }
            else
            {
                superGroups[sgInd].weekLessons += lessonsCnt;
                supergroupMultilessons[day].RemoveAt(supergroupMultilessons[day].Count - 1);
            }
            successful &= (superGroups[sgInd].weekLessons>=0);

            for(int iter = 0;iter<lessonsCnt;iter++)
            {
                foreach(var item in supergroup2GroupSubjetInds[sgInd])
                {
                    successful &= dayState[day].updateLimitsNoTeacher(item.Item1, item.Item2, sign);
                }
                foreach(int tInd in supergroup2TeacherInds[sgInd])
                {
                    successful &= dayState[day].updateTeacherLimits(tInd, sign);
                    teacherDemand[tInd] -= sign*lessonsCnt;
                }
                
            }

            return successful;
        }

        private bool applyMultilesson(int day, int ind, int lessonsCnt, int sign)
        {
            bool successful = true;
            int gInd = groups.FindIndex(g => g.Equals(allMultilessons[ind].g));
            int tInd = teachers.FindIndex(t => t.Equals(allMultilessons[ind].t));
            int sInd = allMultilessons[ind].g.findSubject(allMultilessons[ind].s);

            for(int iter = 0;iter<lessonsCnt;iter++)
            {
                successful &= dayState[day].updateLimits(gInd, sInd, tInd, sign);
            }

            return successful;
        }

        bool solveSuperGroup(int day, int sgInd, int mInd)
        {
            if (superGroups[sgInd].weekLessons == 0)
            {
                return arrangeSuperGroups(sgInd + 1, day);
            }

            bool res = false;

            //using required multilessons
            for(int i = 0;i<superGroups[sgInd].requiredMultilessons.Count;i++)
            {
                if(supergroupMultilessonsAdded[sgInd][i]==true) continue;

                supergroupMultilessonsAdded[sgInd][i] = true;
                bool succ = applySupergroupMultilesson(day, sgInd, superGroups[sgInd].requiredMultilessons[i], +1);
                
                if(succ==true && arrangeSuperGroups(sgInd+1, day)==true) res = true;
                
                supergroupMultilessonsAdded[sgInd][i] = false;
                applySupergroupMultilesson(day, sgInd, superGroups[sgInd].requiredMultilessons[i], -1);
            
                if(res==true) return true;
            }

            int requiredLeft = Enumerable.Range(0, superGroups[sgInd].requiredMultilessons.Count)
            .Sum(ind => ((supergroupMultilessonsAdded[sgInd][ind]==false)?superGroups[sgInd].requiredMultilessons[ind]:0)); 

            //using free multilessons
            for(int lessons = superGroups[sgInd].weekLessons - requiredLeft;lessons>=((day != workDays) ? 0 : superGroups[sgInd].weekLessons);lessons--)
            {
                bool succ = applySupergroupMultilesson(day, sgInd, lessons, +1);
                if(succ==true && arrangeSuperGroups(sgInd+1, day)==true) res = true;
                applySupergroupMultilesson(day, sgInd, lessons, -1);
            
                if(res==true) return true;
            }

            return false;
        }

        private bool arrangeSuperGroups(int sgInd, int day)
        {
            ScheduleGenerator3 sg = null;
            if (sgInd==superGroups.Count)
            {
                for (int t = 0; t < teachers.Count; t++)
                {
                    int demand = teacherDemand[t];
                    int today = minLessons - dayState[day].teacherLeftLessons[t];

                    if (demand > (workDays - day) * minLessons) return false;
                }

                
                sg = new ScheduleGenerator3(groups, teachers, subjects, higharchy, multilessons, supergroupMultilessons);
                if (sg.gen(day, (day==workDays)) == null) return false;

                Console.WriteLine($"--------------> {day} {sgInd}");

                if (day == workDays) return arrangeMultilessons(1, 0);
                return arrangeSuperGroups(0, day + 1);
            }

            for (int t = 0; t < teachers.Count; t++)
            {
                int demand = teacherDemand[t];
                int today = minLessons - dayState[day].teacherLeftLessons[t];

                if (demand > (workDays - day+1) * minLessons - today) return false;
            }

            if(day!=workDays)
            {
                sg = new ScheduleGenerator3(groups, teachers, subjects, higharchy, multilessons, supergroupMultilessons);
                if (sg.gen(day, false) == null) return false;
            }
            

            return solveSuperGroup(day, sgInd, 0);
        }

        void init()
        {
            allMultilessons = new List<Multilesson>();
            foreach(Group g in groups)
            {
                foreach (Multilesson m in g.requiredMultilessons)
                    allMultilessons.Add(m);
            }

            usedMultilessons = new bool[allMultilessons.Count];
            for (int i = 0; i < usedMultilessons.Length; i++) usedMultilessons[i] = false;

            dayState = new ScheduleDayState[workDays + 1];
            List<Group> refList = groups.Select(g => g.CloneFull()).ToList();

            for (int day = 1; day <= workDays; day++)
            {
                dayState[day] = new ScheduleDayState(refList.Select(g => g.ClonePartial(g.weekLims)).ToList(), teachers, maxLessons);
            }

            for (int day = 1; day <= workDays; day++)
            {
                supergroupMultilessons[day] = new List<Tuple<SuperGroup, int>>(100);
            }

            teacherDemand = new int[teachers.Count];
            for(int t = 0;t<teachers.Count;t++)
            {
                teacherDemand[t] = superGroups.Where(sg => sg.teachers.Any(x => x.Equals(teachers[t]) == true) == true).Sum(sg => sg.weekLessons);
            }

            supergroup2TeacherInds = new List<int>[superGroups.Count];
            supergroup2GroupSubjetInds = new List<Tuple<int, int>>[superGroups.Count];

            for(int i = 0;i<superGroups.Count;i++)
            {
                supergroup2TeacherInds[i] = new List<int>();
                foreach(Teacher t in superGroups[i].teachers) supergroup2TeacherInds[i].Add(teachers.FindIndex(x => x.Equals(t)));
            
                supergroup2GroupSubjetInds[i] = new List<Tuple<int, int>>();
                foreach(var g in superGroups[i].groups)
                    supergroup2GroupSubjetInds[i].Add(Tuple.Create(groups.FindIndex(x => x.Equals(g.Item1)==true),
                                                                   g.Item1.findSubject(g.Item2)));
            }

            supergroupMultilessonsAdded = new bool[superGroups.Count][];
            for(int i = 0;i<supergroupMultilessonsAdded.Length;i++) 
            {
                supergroupMultilessonsAdded[i] = new bool[superGroups[i].requiredMultilessons.Count];
                for(int j = 0;j<supergroupMultilessonsAdded[i].Length;j++) supergroupMultilessonsAdded[i][j] = false;
            }
        }

        public WeekSchedule gen()
        {
            init();

            Console.WriteLine($"allMultilessons = {allMultilessons.Count}");
            bool res = arrangeSuperGroups(0, 1);

            if(res==true)
            {
                ans.print();
                ans.exportToExcell("programa1");
            }

            return ans;
        }
    }
}
