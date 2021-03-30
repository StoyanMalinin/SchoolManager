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
        const int maxLessons = 5;

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

        bool[,] freeSupergroupMultilessonsAdded;
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
                for(int i = 0;i<usedMultilessons.Length;i++)
                {
                    if (usedMultilessons[i] == false) return false;
                }

                ScheduleGenerator3 sg = new ScheduleGenerator3(groups, teachers, subjects, higharchy, multilessons, supergroupMultilessons);
                WeekSchedule sgRes = sg.gen(workDays, true);

                if (sgRes is null) return false;

                ans = sgRes;
                return true;
            }

            bool res = false;

            if (res == false)
            {
                if (usedMultilessons[ind] == false)
                {
                    usedMultilessons[ind] = true;
                    multilessons[day].Add(allMultilessons[ind]);

                    ScheduleGenerator3 sg = new ScheduleGenerator3(groups, teachers, subjects, higharchy, multilessons, supergroupMultilessons);
                    if (sg.gen(workDays, true) != null)
                    {
                        res |= arrangeMultilessons(day, ind + 1);
                    }

                    multilessons[day].RemoveAt(multilessons[day].Count - 1);
                    usedMultilessons[ind] = false;
                }
            }

            if (res==false)
            {
                if(arrangeMultilessons(day, ind+1)==true)
                {
                    res = true;
                }
            }

            return res;
        }

        bool applySupergroupMultilesson(int day, int sgInd, int lessonsCnt, int sign)
        {
            bool successful = true;
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

        bool solveSuperGroup(int day, int sgInd, int mInd)
        {
            //if (superGroups[sgInd].weekLessons == 0)
            //{
            //    return arrangeSuperGroups(sgInd + 1, day);
            //}

            bool res = false;
            if (mInd == superGroups[sgInd].requiredMultilessons.Count)
            {
                if (freeSupergroupMultilessonsAdded[sgInd, day] == false)
                {
                    freeSupergroupMultilessonsAdded[sgInd, day] = true;

                    //List<int> possible = new List<int>();
                    //for (int lessons = ((day != workDays) ? 0 : superGroups[sgInd].weekLessons); lessons <= superGroups[sgInd].weekLessons; lessons++)
                    //    possible.Add(lessons);
                    //possible.Reverse();
                    //Shuffle(possible);

                    for (int lessons =  superGroups[sgInd].weekLessons;lessons>=((day != workDays) ? 0 : superGroups[sgInd].weekLessons);lessons--)
                    {
                        superGroups[sgInd].weekLessons -= lessons;
                        supergroupMultilessons[day].Add(Tuple.Create(superGroups[sgInd], lessons));
                        bool succ = applySupergroupMultilesson(day, sgInd, lessons, +1);

                        //Console.WriteLine($"{string.Join(", ", superGroups[sgInd].groups.Select(g => $"({g.Item1.name}, {g.Item2.name})"))} {lessons}");
                        if (succ==true && arrangeSuperGroups(sgInd + 1, day) == true)
                        {
                            res = true;
                        }

                        superGroups[sgInd].weekLessons += lessons;
                        applySupergroupMultilesson(day, sgInd, lessons, -1);
                        supergroupMultilessons[day].RemoveAt(supergroupMultilessons[day].Count - 1);

                        if (res == true) break;
                    }
                    freeSupergroupMultilessonsAdded[sgInd, day] = false;
                }
            }
            else if(res==false)
            {
                mInd = supergroupMultilessons[day].Count(x => x.Item1.Equals(superGroups[sgInd]));
                if (superGroups[sgInd].weekLessons >= superGroups[sgInd].requiredMultilessons[mInd])
                {
                    bool succ = applySupergroupMultilesson(day, sgInd, superGroups[sgInd].requiredMultilessons[mInd], +1);
                    supergroupMultilessons[day].Add(Tuple.Create(superGroups[sgInd], superGroups[sgInd].requiredMultilessons[mInd]));

                    if (succ==true && solveSuperGroup(day, sgInd, mInd + 1) == true)
                    {
                        res = true;
                    }

                    applySupergroupMultilesson(day, sgInd, superGroups[sgInd].requiredMultilessons[mInd], -1);
                    supergroupMultilessons[day].RemoveAt(supergroupMultilessons[day].Count - 1);
                }
            }

            return res;
        }

        private bool arrangeSuperGroups(int sgInd, int day)
        {
            //ScheduleGenerator3 sg = new ScheduleGenerator3(groups, teachers, subjects, higharchy, multilessons, supergroupMultilessons);
            //if (sg.gen() == null) return false;

            if (sgInd==superGroups.Count)
            {
                for (int t = 0; t < teachers.Count; t++)
                {
                    int demand = teacherDemand[t];
                    int today = 6 - dayState[day].teacherLeftLessons[t];

                    if (today > 6) return false;
                    if (demand > (workDays - day) * 6) return false;
                }

                
                ScheduleGenerator3 sg = new ScheduleGenerator3(groups, teachers, subjects, higharchy, multilessons, supergroupMultilessons);
                if (sg.gen(day, (day==workDays)) == null) return false;

                Console.WriteLine($"--------------> {day} {sgInd}");

                if (day == workDays) return arrangeMultilessons(1, 0);
                else return arrangeSuperGroups(0, day + 1);
            }

            for (int t = 0; t < teachers.Count; t++)
            {
                //int demand = superGroups.Where(sg => sg.teachers.Any(x => x.Equals(teachers[t]) == true) == true).Sum(sg => sg.weekLessons);
                int demand = teacherDemand[t];

                //int today = supergroupMultilessons[day].Where(x => x.Item1.teachers.Any(y => y.Equals(teachers[t]) == true) == true).Sum(x => x.Item2);
                int today = 6 - dayState[day].teacherLeftLessons[t];

                if (today > 6) return false;
                if (demand > (workDays - day+1) * 6 - today) return false;
            }

            if(day!=workDays)
            {
                ScheduleGenerator3 sg1 = new ScheduleGenerator3(groups, teachers, subjects, higharchy, multilessons, supergroupMultilessons);
                if (sg1.gen(day, false) == null) return false;
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
            //allMultilessons.Clear();

            usedMultilessons = new bool[allMultilessons.Count];
            for (int i = 0; i < usedMultilessons.Length; i++) usedMultilessons[i] = false;

            dayState = new ScheduleDayState[workDays + 1];
            List<Group> refList = groups.Select(g => g.CloneFull()).ToList();

            for (int day = 1; day <= workDays; day++)
            {
                dayState[day] = new ScheduleDayState(refList.Select(g => g.ClonePartial(g.weekLims)).ToList(), teachers, maxLessons);
            }

            freeSupergroupMultilessonsAdded = new bool[superGroups.Count, workDays + 1];
            for (int day = 1; day <= workDays; day++)
            {
                for(int i = 0;i<superGroups.Count;i++) freeSupergroupMultilessonsAdded[i, day] = false;
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
