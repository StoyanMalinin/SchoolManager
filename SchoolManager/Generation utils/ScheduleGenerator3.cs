using SchoolManager.MaxFlow;
using SchoolManager.ScheduleUtils;
using SchoolManager.School_Models;
using SchoolManager.School_Models.Higharchy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SchoolManager.Generation_utils
{
    class ScheduleGenerator3
    {
        const int workDays = 5;
        const int maxLessons = 5;

        private List<Group> groups;
        private List<Teacher> teachers;
        private List<Subject> subjects;
        private TreeNode higharchy;

        private List<Multilesson>[] multilessons;  

        public ScheduleGenerator3() { }
        public ScheduleGenerator3(List <Group> groups, List <Teacher> teachers, 
                                  List <Subject> subjects, TreeNode higharchy)
        {
            this.groups = groups;
            this.teachers = teachers;
            this.subjects = subjects;
            this.higharchy = higharchy;

            groupNode = new int[groups.Count];
            groupHigharchy = new TreeNode[groups.Count];

            teacherNode = new int[teachers.Count];
            groupSubjectNode = new int[groups.Count, subjects.Count];

            this.multilessons = new List<Multilesson>[workDays+1];
            for (int day = 1; day <= workDays; day++)
                this.multilessons[day] = new List<Multilesson>();
        }
        public ScheduleGenerator3(List<Group> groups, List<Teacher> teachers,
                                  List<Subject> subjects, TreeNode higharchy, List<Multilesson>[] multilessons) : this(groups, teachers, subjects, higharchy)
        {
            this.multilessons = multilessons;
        }

        private List<Group>[] dayState;

        private int[] teacherNode;
        private int allTeachersNode;
        private int[,] groupSubjectNode;

        private int[] groupNode; 
        private TreeNode[] groupHigharchy;

        private CirculationFlowGraph G;

        private int[,] teacherLeftLessons, groupLeftLessons;
        private List<int>[,] teacherGroup2Subjects;
        private int[,] groupSubject2Teacher;

        private int[,] groupSubjectEdge;

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

                //Console.WriteLine();
                //Console.WriteLine($"Group: {groups[g].name}");
                //groupHigharchy[g].printTree();
            }
            for(int g = 0;g<groups.Count;g++)
            {
                void findSubjects(TreeNode x)
                {
                    if(x.GetType()==typeof(SubjectTreeNode))
                    {
                        groupSubjectNode[g, groups[g].findSubject((x as SubjectTreeNode).s)] = x.nodeCode;
                        //Console.WriteLine($"{g} -> [{groups[g].findSubject((x as SubjectTreeNode).s)} <=> {x.nodeCode}]");

                        return;
                    }

                    foreach (TreeNode y in x.children)
                        findSubjects(y);
                }

                findSubjects(groupHigharchy[g]);
            }

            allTeachersNode = node;
            node++;

            G = new CirculationFlowGraph(node*maxLessons);
        }

        void initArrays()
        {
            groupSubjectEdge = new int[groups.Count, subjects.Count];

            teacherLeftLessons = new int[workDays + 1, teachers.Count];
            for (int day = 1; day <= workDays; day++)
                for (int t = 0; t < teachers.Count; t++)
                    teacherLeftLessons[day, t] = maxLessons;

            groupLeftLessons = new int[workDays + 1, groups.Count];
            for (int day = 1; day <= workDays; day++)
                for (int g = 0; g < groups.Count; g++)
                    groupLeftLessons[day, g] = maxLessons;

            dayState = new List<Group>[workDays + 1];
            dayState[1] = groups.Select(g => g.CloneFull()).ToList();
            for (int day = 2; day <= workDays; day++)
                dayState[day] = dayState[1].Select(g => g.ClonePartial(g.weekLims)).ToList();

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
        }
        private bool updateLimits(int d, int g, int s, int t, int sign)
        {
            groupLeftLessons[d, g] -= sign;
            teacherLeftLessons[d, t] -= sign;
            dayState[d][g].applySubject(s, sign);

            if (groupLeftLessons[d, g] < 0 || teacherLeftLessons[d, t] < 0 || dayState[d][g].getSubjectDayLim(s) < 0) return false;
            return true;
        }

        private bool generateDay(int day)
        {
            G.reset();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($" ----- {day} ----- ");

            // ----- setting node demands -----
            G.setDemand(allTeachersNode, -(groups.Count * maxLessons));//redo later, when we can have daily lessons, different than maxLessons
            for (int g = 0; g < groups.Count; g++) G.setDemand(groupNode[g], maxLessons);
        
            // ----- adding edges ----- 
            
            //connecting teachers to allTeachersNode
            for(int t = 0;t<teachers.Count;t++)
            {
                //Console.WriteLine($"Teacher: {teachers[t].name}");

                int requested = 0;
                for (int g = 0; g < groups.Count; g++)
                {
                    foreach (int s in teacherGroup2Subjects[t, g])
                    {
                        if (dayState[day][g].subject2Teacher[s].Item2.name == teachers[t].name)
                        {
                            requested += dayState[day][g].getSubjectWeekLim(s);
                        }
                    }
                }

                for (int d = day+1; d <= workDays; d++)
                    requested -= teacherLeftLessons[d, t];

                int u = allTeachersNode;
                int v = teacherNode[t];
                int l = Math.Max(requested, 0);
                int c = maxLessons;

                G.addEdge(u, v, l, c);
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

                       // if (important.Count > 0) 
                       //     Console.WriteLine($"{groups[g].name} {groups[g].subject2Teacher[s].Item1.name} -> {scheduledLessons.l} {scheduledLessons.r}");
                        
                        if(important.Count>0) G.addEdge(teacherNode[t], groupSubjectNode[g, s], scheduledLessons.l, scheduledLessons.r);
                        else G.addEdge(teacherNode[t], groupSubjectNode[g, s], 0, maxLessons);
                    }
                }
            }

            //connecting the actual higharcy structures
            for(int g = 0;g<groups.Count;g++)
            {
                //Console.WriteLine();
                //Console.WriteLine($"{groups[g].name}");

                void dfs(TreeNode x)
                {
                    if(x.GetType()==typeof(SubjectTreeNode))
                    {
                        int s = dayState[day][g].findSubject((x as SubjectTreeNode).s);
                        if (dayState[day][g].subject2Teacher[s].Item2 == null) return;
                        
                        int teacherInd = groupSubject2Teacher[g, s];
                        int lessonsLeft = dayState[day][g].getSubjectWeekLim(s);

                        for (int d = day+1; d <= workDays; d++)
                        {
                            List <Multilesson> important = multilessons[d].Where(m => m.g.Equals(groups[g])==true && m.s.Equals((x as SubjectTreeNode).s) == true).ToList();

                            int rm = 0;
                            if(important.Count==0)
                            {
                                rm = Math.Min(dayState[day][g].getSubjectDayLim(s), teacherLeftLessons[d, teacherInd]);
                                rm = Math.Min(rm, groupLeftLessons[d, g]);
                            }
                            else
                            {
                                rm = important.Sum(m => m.val.r);
                            }

                            lessonsLeft -= rm;
                        }

                        int subjectLim = Math.Min(dayState[day][g].getSubjectDayLim(s),
                                                  dayState[day][g].getSubjectWeekLim(s));

                        int subjectDemand = 0;
                        for(int d = day+1;d<=workDays;d++)
                        {
                            subjectDemand += multilessons[d].Where(r => r.g.Equals(groups[g]) == true && r.s.Equals((x as SubjectTreeNode).s))
                                                            .Sum(r => r.val.l);
                        }

                        groupSubjectEdge[g, s] = G.addEdge(x.nodeCode, x.parent.nodeCode, 
                                                           Math.Max(0, lessonsLeft), Math.Min(subjectLim, dayState[day][g].getSubjectWeekLim(s) - subjectDemand), 
                                                           true);
                        
                        return;
                    }

                    if (x.parent != null)
                        G.addEdge(x.nodeCode, x.parent.nodeCode, 0, maxLessons);

                    foreach (TreeNode y in x.children)
                        dfs(y);
                }

                dfs(groupHigharchy[g]);
            }

            //connecting students to their higharchy trees
            //for now the capacities will be simply maxLessons
            for(int g = 0;g<groups.Count;g++)
            {
                G.addEdge(groupHigharchy[g].nodeCode, groupNode[g], 0, maxLessons);
            }

            G.eval();

            for(int g = 0;g<groups.Count;g++)
            {
                List<int> teacherInds = new List<int>();
                for (int s = 0; s < groups[g].subject2Teacher.Count; s++)
                {
                    for (int i = 0; i < G.getEdge(groupSubjectEdge[g, s]); i++)
                    {
                        teacherInds.Add(groupSubject2Teacher[g, s]);
                        if (updateLimits(day, g, s, groupSubject2Teacher[g, s], + 1) == false) return false;
                    }
                }

                if (teacherInds.Count != maxLessons)
                    return false;   
            }

            ScheduleCompleter sc = new ScheduleCompleter(dayState[day], teachers, maxLessons);

            DaySchedule ds = sc.gen(true);
            if (ds is null) return false;

            ds.print();
            return true;
        }

        public bool runDiagnostics()
        {
            for(int day = 1;day<=workDays;day++)
            {
                for (int t = 0; t < teachers.Count; t++)
                    if (teacherLeftLessons[day, t] < 0)
                        return false;
            }
            for (int g = 0; g < groups.Count; g++)
            {
                for(int s = 0;s<dayState[1][g].subject2Teacher.Count;s++)
                {
                    if (dayState[1][g].getSubjectWeekLim(s) != 0)
                        return false;
                }

                for (int s = 0; s < dayState[1][g].subject2Teacher.Count; s++)
                {
                    for(int day = 1;day<=workDays;day++)
                    {
                        if (dayState[day][g].getSubjectDayLim(s) < 0)
                            return false;
                    }   
                }
            }

            return true;
        }

        public bool generate()
        {
            initNodes();
            initArrays();
            for(int day = 1;day<=workDays;day++)
            {
                bool dayRes = generateDay(day);
                if (dayRes == false) return false;
            }

            bool diagnosticsRes = runDiagnostics();
            if (diagnosticsRes == true) Console.WriteLine("Kein problem!");
            else Console.WriteLine("Es gibt ein problem!");

            if (diagnosticsRes == false) return false;

            return true;
        }
    }
}
