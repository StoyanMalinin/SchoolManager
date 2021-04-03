using SchoolManager.MaxFlow;
using SchoolManager.ScheduleUtils;
using SchoolManager.School_Models;
using SchoolManager.School_Models.Higharchy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManager.Generation_utils
{
    class ScheduleGenerator3
    {
        const int workDays = 5;
        const int minLessons = 6, maxLessons = 7;

        private List<Group> groups;
        private List<Teacher> teachers;
        private List<Subject> subjects;
        private TreeNode higharchy;

        private List<Tuple<SuperGroup, int>>[] supergroupMultilessons;
        private List<Multilesson>[] multilessons;  

        public ScheduleGenerator3() { }
        public ScheduleGenerator3(List<Group> groups, List<Teacher> teachers,
                                  List<Subject> subjects, TreeNode higharchy)
        {
            this.groups = groups;
            this.teachers = teachers;
            this.subjects = subjects;
            this.higharchy = higharchy;

            groupNode = new int[groups.Count];
            groupHigharchy = new TreeNode[groups.Count];

            teacherNode = new int[teachers.Count];
            groupSubjectNode = new int[groups.Count, subjects.Count];

            this.multilessons = new List<Multilesson>[workDays + 1];
            this.supergroupMultilessons = new List<Tuple<SuperGroup, int>>[workDays + 1];
            for (int day = 1; day <= workDays; day++)
            {
                this.multilessons[day] = new List<Multilesson>();
                this.supergroupMultilessons[day] = new List<Tuple<SuperGroup, int>>();
            }
        }
        public ScheduleGenerator3(List<Group> groups, List<Teacher> teachers,
                                  List<Subject> subjects, TreeNode higharchy, List<Multilesson>[] multilessons) : this(groups, teachers, subjects, higharchy)
        {
            this.multilessons = multilessons;
        }
        public ScheduleGenerator3(List<Group> groups, List<Teacher> teachers,
                                  List<Subject> subjects, TreeNode higharchy, 
                                  List<Multilesson>[] multilessons, 
                                  List<Tuple<SuperGroup, int>>[] supergroupMultilessons) : this(groups, teachers, subjects, higharchy, multilessons)
        {
            this.supergroupMultilessons = supergroupMultilessons;
        }

        private ScheduleDayState[] dayState;

        private int[] teacherNode;
        private int allTeachersNode;
        private int[,] groupSubjectNode;

        private int[] groupNode; 
        private TreeNode[] groupHigharchy;

        private CirculationFlowGraph G;

        private List<int>[,] teacherGroup2Subjects;
        private int[,] groupSubject2Teacher;
        private int[,] groupSubjectEdge;

        private static List <Tuple <int, int>>[][] lastConfig = null;
        private static DaySchedule[] lastConfigSolution = null; 

        private void initNodes()
        {
            int node = 1;

            //initializing nodes
            for (int g = 0;g<groups.Count;g++)
            {
                groupNode[g] = node;
                node++;
            }
            for(int t = 0;t<teachers.Count;t++)
            {
                teacherNode[t] = node;
                node++;
            }

            for(int g = 0;g<groups.Count;g++)
            {
                groupHigharchy[g] = higharchy.Clone();
                groupHigharchy[g].encodeTree(ref node);
            }
            for(int g = 0;g<groups.Count;g++)
            {
                void findSubjects(TreeNode x)
                {
                    if(x.GetType()==typeof(SubjectTreeNode))
                    {
                        int sInd = groups[g].findSubject((x as SubjectTreeNode).s);
                        if(sInd!=-1) groupSubjectNode[g, sInd] = x.nodeCode;

                        return;
                    }

                    foreach (TreeNode y in x.children)
                        findSubjects(y);
                }

                findSubjects(groupHigharchy[g]);
            }

            allTeachersNode = node;
            node++;

            G = new CirculationFlowGraph(node*2);
        }

        void initArrays()
        {
            groupSubjectEdge = new int[groups.Count, subjects.Count];

            dayState = new ScheduleDayState[workDays + 1];
            List <Group> refList = groups.Select(g => g.CloneFull()).ToList();

            for(int day = 1;day<=workDays;day++)
            {
                dayState[day] = new ScheduleDayState(refList.Select(g => g.ClonePartial(g.weekLims)).ToList(), teachers, maxLessons);
            }

            groupSubject2Teacher = new int[groups.Count, subjects.Count];
            for (int g = 0; g < groups.Count; g++)
            {
                for (int s = 0; s < groups[g].subject2Teacher.Count; s++)
                {
                    if (groups[g].subject2Teacher[s].Item2 == null)
                    {
                        continue;
                    }

                    groupSubject2Teacher[g, s] = teachers.FindIndex(t => t.name == groups[g].subject2Teacher[s].Item2.name);
                }
            }

            teacherGroup2Subjects = new List<int>[teachers.Count, groups.Count];
            for (int t = 0; t < teachers.Count; t++)
            {
                for (int g = 0; g < groups.Count; g++)
                {
                    teacherGroup2Subjects[t, g] = new List<int>();
                    for (int s = 0; s < groups[g].subject2Teacher.Count; s++)
                    {
                        if (groups[g].subject2Teacher[s].Item2 == null) continue;

                        if (groups[g].subject2Teacher[s].Item2.name == teachers[t].name)
                        {
                            teacherGroup2Subjects[t, g].Add(s);
                        }
                    }
                }
            }

            if(lastConfig is null)
            {
                lastConfigSolution = new DaySchedule[workDays+1];
                lastConfig = new List<Tuple<int, int>>[workDays+1][];
                for(int day = 1;day<=workDays;day++) lastConfig[day] = null;
            }
        }

        private bool tryConfig(List <Tuple <int, int>>[] config, int day)
        {
            bool succ = true;
            for(int g = 0;g<config.Length;g++)
            {
                foreach(var tup in config[g]) succ &= dayState[day].updateLimits(g, tup.Item1, tup.Item2, +1);

                if (dayState[day].groups[g].curriculum.Count < dayState[day].groups[g].minLessons
                    || dayState[day].groups[g].curriculum.Count > maxLessons)
                {
                    succ = false;
                }
            }

            for(int g = 0;g<groups.Count;g++)
            {
                foreach(Multilesson m in multilessons[day].Where(x => (x.g.Equals(groups[g])==true)))
                {
                    int cnt = dayState[day].groups[g].curriculum.Count(s => s.Equals(m.s)==true);
                    if (cnt < m.val.l || cnt > m.val.r) succ = false;
                }
            }

            succ &= checkPossible(day+1);

            for(int g = 0;g<config.Length;g++)
            {
                foreach(var tup in config[g]) dayState[day].updateLimits(g, tup.Item1, tup.Item2, -1);
            }
            return succ;
        }

        private DaySchedule useConfig(List <Tuple <int, int>>[] config, int day)
        {
            for(int g = 0;g<config.Length;g++)
            {
                foreach(var tup in config[g]) dayState[day].updateLimits(g, tup.Item1, tup.Item2, +1);
            }

            return lastConfigSolution[day];
        }

        private DaySchedule generateDay(int day)
        {
            if((!(lastConfig[day] is null)) && tryConfig(lastConfig[day], day)==true)
            {
                return useConfig(lastConfig[day], day);
            }

            G.reset();

            // ----- setting node demands -----
            G.setDemand(allTeachersNode, -(Enumerable.Range(0, groups.Count).Sum(ind => dayState[day].groupLeftLessons[ind])));
            for (int g = 0; g < groups.Count; g++) G.setDemand(groupNode[g], dayState[day].groupLeftLessons[g]);
        
            // ----- adding edges ----- 
            
            //connecting teachers to allTeachersNode
            for(int t = 0;t<teachers.Count;t++)
            {
                int requested = 0;
                for (int g = 0; g < groups.Count; g++)
                {
                    foreach (int s in teacherGroup2Subjects[t, g])
                    {
                        if (groups[g].subject2Teacher[s].Item2.Equals(teachers[t])==true)
                        {
                            requested += dayState[day].groups[g].getSubjectWeekLim(s);
                        }
                    }
                }

                for (int d = day+1; d <= workDays; d++)
                    requested -= dayState[d].teacherLeftLessons[t];

                int u = allTeachersNode;
                int v = teacherNode[t];
                int l = Math.Max(requested, 0);
                int c = dayState[day].teacherLeftLessons[t];

                //Console.WriteLine($"{teachers[t].name} -> {l} {c}");
                G.addEdge(u, v, l, Math.Min(c, minLessons), true);
            }

            //connecting teachers to possible lessons
            for(int t = 0;t<teachers.Count;t++)
            {
                for (int g = 0; g < groups.Count; g++)
                {
                    foreach (int s in teacherGroup2Subjects[t, g])
                    {
                        IntInInterval scheduledLessons = new IntInInterval(0);
                        List<Multilesson> important = multilessons[day].Where(m => m.t.Equals(teachers[t]) == true
                                                           && m.g.Equals(groups[g]) == true && m.s.Equals(groups[g].subject2Teacher[s].Item1)).ToList();

                        foreach (Multilesson r in important)
                            scheduledLessons = scheduledLessons + r.val;

                        if(important.Count>0) G.addEdge(teacherNode[t], groupSubjectNode[g, s], scheduledLessons.l, scheduledLessons.r, true);
                        else G.addEdge(teacherNode[t], groupSubjectNode[g, s], 0, minLessons, true);
                    }
                }
            }

            //connecting the actual higharcy structures
            for(int g = 0;g<groups.Count;g++)
            {
                void dfs(TreeNode x)
                {
                    if(x.GetType()==typeof(SubjectTreeNode))
                    {
                        int s = dayState[day].groups[g].findSubject((x as SubjectTreeNode).s);
                        
                        if (s == -1) return;
                        if (dayState[day].groups[g].subject2Teacher[s].Item2 == null) return;

                        int teacherInd = groupSubject2Teacher[g, s];
                        int lessonsLeft = dayState[day].groups[g].getSubjectWeekLim(s);

                        for (int d = day+1; d <= workDays; d++)
                        {
                            IEnumerable <Multilesson> important = multilessons[d].Where(m => m.g.Equals(groups[g])==true 
                                                                                 && m.s.Equals((x as SubjectTreeNode).s) == true);
                            
                            int rm = 0;
                            if(important.Any()==false)
                            {
                                rm = Math.Min(dayState[d].groups[g].getSubjectDayLim(s), dayState[d].teacherLeftLessons[teacherInd]);
                                rm = Math.Min(rm, dayState[d].groupLeftLessons[g]);
                            }
                            else
                            {
                                rm = important.Sum(m => m.val.r);
                            }

                            lessonsLeft -= rm;
                        }

                        int subjectLim = Math.Min(dayState[day].groups[g].getSubjectDayLim(s),
                                                  dayState[day].groups[g].getSubjectWeekLim(s));

                        int subjectDemand = 0;
                        for(int d = day+1;d<=workDays;d++)
                        {
                            subjectDemand += multilessons[d].Where(r => r.g.Equals(groups[g]) == true && r.s.Equals((x as SubjectTreeNode).s))
                                                            .Sum(r => r.val.l);
                        }

                        int fixatedLessons = multilessons[day].Where(r => r.g.Equals(groups[g]) == true && r.s.Equals((x as SubjectTreeNode).s))
                                                              .Sum(r => r.val.l);

                        groupSubjectEdge[g, s] = G.addEdge(x.nodeCode, x.parent.nodeCode, 
                                                           Math.Max(fixatedLessons, Math.Max(0, lessonsLeft)), 
                                                           Math.Min(subjectLim, dayState[day].groups[g].getSubjectWeekLim(s) - subjectDemand));
                        
                        return;
                    }

                    if (x.parent != null)
                    {
                        int lim = maxLessons;
                        
                        if(x.GetType() == typeof(LimitationTreeNode))
                            lim = Math.Min(lim, dayState[day].groups[g].getLimGroupDayLim((x as LimitationTreeNode).lg));

                        int lessonsLeft = groups[g].subject2Teacher.Where(t => t.Item1.limGroups.Contains((x as LimitationTreeNode).lg)==true)
                                                                   .Sum(t => dayState[1].groups[g].getSubjectWeekLim(groups[g].findSubject(t.Item1)));

                        for(int d = day+1;d<=workDays;d++)
                        {
                            IEnumerable<Multilesson> important = multilessons[d].Where(m => m.g.Equals(groups[g]) == true
                                                                                && m.s.limGroups.Contains((x as LimitationTreeNode).lg)==true);

                            int rm = 0;
                            if (important.Any() == false)
                            {
                                rm = Math.Min(dayState[d].groups[g].getLimGroupDayLim((x as LimitationTreeNode).lg), dayState[d].groupLeftLessons[g]);
                            }
                            else
                            {
                                rm = important.Sum(m => m.val.r);
                            }

                            lessonsLeft -= rm;
                        }

                        G.addEdge(x.nodeCode, x.parent.nodeCode, Math.Max(0, lessonsLeft), lim);
                    }
                        
                    foreach (TreeNode y in x.children)
                        dfs(y);
                }

                dfs(groupHigharchy[g]);
            }

            //connecting groups to their higharchy trees
            for(int g = 0;g<groups.Count;g++)
            {
                int allLeft = dayState[day].groups[g].subject2Teacher.Sum(t => dayState[day].groups[g].getSubjectWeekLim(dayState[day].groups[g].findSubject(t.Item1)));
                for (int d = day + 1; d <= workDays; d++) allLeft -= dayState[d].groupLeftLessons[g];

                int fixatedLessons = multilessons[day].Where(r => r.g.Equals(groups[g])==true).Sum(r => r.val.l);

                int upperBound = dayState[day].groups[g].subject2Teacher.Sum(t => dayState[day].groups[g].getSubjectWeekLim(dayState[day].groups[g].findSubject(t.Item1)));
                for (int d = day + 1; d <= workDays; d++) upperBound -= Math.Min(dayState[d].groupLeftLessons[g], groups[g].minLessons);

                G.addEdge(groupHigharchy[g].nodeCode, groupNode[g], 
                          Math.Max(Math.Max(groups[g].minLessons - dayState[day].groups[g].curriculum.Count, allLeft), fixatedLessons), 
                          Math.Min(dayState[day].groupLeftLessons[g], upperBound));
            }

            //connecting groups to allTeachersNode
            for (int g = 0; g < groups.Count; g++)
            {
                G.addEdge(allTeachersNode, groupNode[g], 0, Math.Min(maxLessons-minLessons, maxLessons-dayState[day].groups[g].curriculum.Count));
            }

            G.eval();

            List <Tuple<int, int>>[] currConfig = new List<Tuple <int, int>>[groups.Count];
            for(int g = 0;g<groups.Count;g++)
            {
                //Console.WriteLine($"---- {groups[g].name} - {g} ---");

                int expectedCnt = dayState[day].groupLeftLessons[g];
                List<int> teacherInds = new List<int>();
                currConfig[g] = new List<Tuple <int, int>>();

                for (int s = 0; s < groups[g].subject2Teacher.Count; s++)
                {
                    if (groups[g].subject2Teacher[s].Item2 is null) continue;
                        
                    for (int i = 0; i < G.getEdge(groupSubjectEdge[g, s]); i++)
                    {
                        currConfig[g].Add(Tuple.Create(s, groupSubject2Teacher[g, s]));
                        teacherInds.Add(groupSubject2Teacher[g, s]);
                        //Console.WriteLine(teachers[groupSubject2Teacher[g, s]].name);

                        if (dayState[day].updateLimits(g, s, groupSubject2Teacher[g, s], +1) == false)
                        {
                            Console.WriteLine($"ebalo si maikata {g}");
                            return null;
                        }
                    }
                }

                if (dayState[day].groups[g].curriculum.Count < dayState[day].groups[g].minLessons
                    || dayState[day].groups[g].curriculum.Count > maxLessons)
                {
                    Console.WriteLine($"e de {dayState[day].groups[g].curriculum.Count} {g} || {day}");
                    return null;
                }
                       
            }

            Console.WriteLine("pak faida e ");
            ScheduleCompleter sc = new ScheduleCompleter(dayState[day].groups, teachers, supergroupMultilessons[day], maxLessons);
            DaySchedule ds = sc.gen(true);

            if(!(ds is null)) 
            {
                lastConfigSolution[day] = ds;
                lastConfig[day] = currConfig;
            }
            return ds;
        }

        public bool runDiagnostics()
        {
            for(int day = 1;day<=workDays;day++)
            {
                for (int t = 0; t < teachers.Count; t++)
                    if (dayState[day].teacherLeftLessons[t] < 0)
                        return false;
            }
            for (int g = 0; g < groups.Count; g++)
            {
                for(int s = 0;s<dayState[1].groups[g].subject2Teacher.Count;s++)
                {
                    if (dayState[1].groups[g].getWeekBottleneck(s) != 0)
                        return false;
                }

                for (int s = 0; s < dayState[1].groups[g].subject2Teacher.Count; s++)
                {
                    for(int day = 1;day<=workDays;day++)
                    {                        
                        if (dayState[day].groups[g].getDayBottleneck(s) < 0)
                            return false;
                    }   
                }
            }

            for(int g = 0;g<groups.Count;g++)
            {
                for(int day = 1;day<=workDays;day++)
                {
                    foreach(Multilesson m in multilessons[day].Where(x => (x.g.Equals(groups[g])==true)))
                    {
                        int cnt = dayState[day].groups[g].curriculum.Count(s => s.Equals(m.s)==true);
                        if (cnt < m.val.l || cnt > m.val.r) return false;
                    }
                }
            }

            return true;
        }   

        private bool checkPossible(int day)
        {
            for(int t = 0;t<teachers.Count;t++)
            {
                int demand = 0;
                for(int g = 0;g<dayState[1].groups.Count;g++)
                {
                    demand += dayState[1].groups[g].subject2Teacher.Where(tup => (!(tup.Item2 is null)) && tup.Item2.Equals(teachers[t])==true)
                    .Sum(x => dayState[1].groups[g].getSubjectWeekLim(dayState[1].groups[g].findSubject(x.Item1)));
                }

                for(int d = day;d<=workDays;d++) demand -= dayState[d].teacherLeftLessons[t];

                if(demand>0) return false;
            }

            for(int g = 0;g<dayState[1].groups.Count;g++)
            {
                for(int s = 0;s<dayState[1].groups[g].subject2Teacher.Count;s++)
                {
                    int demand = dayState[1].groups[g].getSubjectWeekLim(s);
                    for(int d = day;d<=workDays;d++) demand -= dayState[d].groups[g].getDayBottleneck(s);

                    if(demand>0) return false;
                }
            }
            
            return true;
        }

        static int callNum = 0, succsefullCallNum = 0;

        public WeekSchedule gen(int limDay, bool isFinal)
        {
            if (limDay == -1) limDay = workDays;
            WeekSchedule ws = new WeekSchedule(workDays);

            initNodes();
            initArrays();

            callNum++;
            Console.WriteLine($"callNum: {callNum} || {(double)succsefullCallNum/(double)callNum}");
            
            for (int day = 1; day <= limDay; day++)
            {
                foreach (var item in supergroupMultilessons[day])
                {
                    for(int iter = 0;iter<item.Item2;iter++)
                    {
                        foreach (Tuple<Group, Subject> tup in item.Item1.groups)
                        {                            
                            int g = groups.FindIndex(g => g.Equals(tup.Item1));
                            int s = groups[g].findSubject(tup.Item2);

                            if (dayState[day].updateLimitsNoTeacher(g, s, +1) == false)
                            {
                                return null;
                            }
                                   
                        }

                        foreach (Teacher t in item.Item1.teachers)
                        {
                            int tInd = teachers.FindIndex(x => x.Equals(t) == true);
                            if (dayState[day].updateTeacherLimits(tInd, +1) == false)
                            {
                                //Console.WriteLine("s tichar");
                                return null;
                            }
                                
                        }
                    }
                }
            }

            for (int day = 1;day<=limDay;day++)
            {
                if(checkPossible(day)==false) 
                {
                    System.Console.WriteLine("FAKKKKKKKKKKKK");
                    return null;
                }
                DaySchedule dayRes = generateDay(day);

                if (dayRes == null) return null;
                ws.days[day] = dayRes;
            }

            bool diagnosticsRes = runDiagnostics();
            if (diagnosticsRes == false && limDay==workDays && isFinal==true) return null;
            
            Console.WriteLine("davai volene");
            succsefullCallNum++;

            return ws;
        }
    }
}
