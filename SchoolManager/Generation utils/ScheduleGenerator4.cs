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
                WeekSchedule sgRes = sg.gen();

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
                    if (sg.gen() != null)
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

        List<Tuple<SuperGroup, int>>[] supergroupMultilessons = new List<Tuple<SuperGroup, int>>[workDays + 1];
        bool solveSuperGroup(int sgInd, int mInd, int weekLessons)
        {
            if (weekLessons == 0) return arrangeSuperGroups(sgInd + 1);

            bool res = false;
            if (mInd == superGroups[sgInd].requiredMultilessons.Count)
            {
                for (int day = 1; day <= workDays; day++)
                {
                    for (int lessons = 1; lessons <= weekLessons; lessons++)
                    {
                        supergroupMultilessons[day].Add(Tuple.Create(superGroups[sgInd], lessons));

                        if (solveSuperGroup(sgInd, mInd, weekLessons - lessons) == true)
                        {
                            res = true;
                            break;
                        }

                        supergroupMultilessons[day].RemoveAt(supergroupMultilessons[day].Count - 1);
                    }

                    if (res == true) break;
                }
            }
            else
            {
                for (int day = 1; day <= workDays; day++)
                {
                    if (weekLessons < superGroups[sgInd].requiredMultilessons[mInd]) continue;

                    supergroupMultilessons[day].Add(Tuple.Create(superGroups[sgInd], superGroups[sgInd].requiredMultilessons[mInd]));

                    if (solveSuperGroup(sgInd, mInd+1, weekLessons - superGroups[sgInd].requiredMultilessons[mInd]) == true)
                    {
                        res = true;
                        break;
                    }

                    supergroupMultilessons[day].RemoveAt(supergroupMultilessons[day].Count - 1);
                }
            }

            return res;
        }

        private bool arrangeSuperGroups(int sgInd)
        {
            if(sgInd==superGroups.Count)
            {
                Console.WriteLine("ANFANGEN");
                return arrangeMultilessons(1, 0);
            }

            return solveSuperGroup(sgInd, 0, superGroups[sgInd].weekLessons);
        }

        public WeekSchedule gen()
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

            for (int day = 1; day <= workDays; day++) supergroupMultilessons[day] = new List<Tuple<SuperGroup, int>>();

            Console.WriteLine($"allMultilessons = {allMultilessons.Count}");
            bool res = arrangeSuperGroups(0);

            if(res==true)
            {
                for(int day = 1;day<=workDays;day++)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Day {day}");
                    ans.days[day].print();
                }
            }

            return ans;
        }
    }
}
