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
        private List<Multilesson>[] multilessons;

        public ScheduleGenerator4() { }
        public ScheduleGenerator4(List<Group> groups, List<Teacher> teachers,
                                  List<Subject> subjects, TreeNode higharchy)
        {
            this.groups = groups;
            this.teachers = teachers;
            this.subjects = subjects;
            this.higharchy = higharchy;

            this.multilessons = new List<Multilesson>[workDays + 1];
            for (int day = 1; day <= workDays; day++)
                this.multilessons[day] = new List<Multilesson>();
        }
        public ScheduleGenerator4(List<Group> groups, List<Teacher> teachers,
                                  List<Subject> subjects, TreeNode higharchy, List<Multilesson>[] multilessons) : this(groups, teachers, subjects, higharchy)
        {
            this.multilessons = multilessons;
        }

        private List<Multilesson> allMultilessons;
        private bool[] usedMultilessons;

        private WeekSchedule ans = null;
        private ScheduleDayState[] dayState;

        private bool rec(int day, int ind)
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

                ScheduleGenerator3 sg = new ScheduleGenerator3(groups, teachers, subjects, higharchy, multilessons);
                WeekSchedule sgRes = sg.gen();

                if (sgRes is null) return false;

                Console.WriteLine("PABEDA");
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

                    ScheduleGenerator3 sg = new ScheduleGenerator3(groups, teachers, subjects, higharchy, multilessons);
                    if (sg.gen() != null)
                    {
                        res |= rec(day, ind + 1);
                    }

                    multilessons[day].RemoveAt(multilessons[day].Count - 1);
                    usedMultilessons[ind] = false;
                }
            }

            if (res==false)
            {
                if(rec(day, ind+1)==true)
                {
                    res = true;
                }
            }

            return res;
        }

        public WeekSchedule gen()
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

            bool res = rec(1, 0);

            if(res==true)
            {
                for(int day = 1;day<=workDays;day++)
                {
                    Console.WriteLine($" --- {day} ---");
                    ans.days[day].print();
                }
            }

            return ans;
        }
    }
}
