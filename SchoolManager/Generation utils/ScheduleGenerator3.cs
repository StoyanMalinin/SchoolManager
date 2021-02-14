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

            G = new CirculationFlowGraph(node);
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

        private void updateLimits(int d, int g, int s, int sign)
        {
            groupLeftLessons[d, g] -= sign;
            teacherLeftLessons[d, g] -= sign;
            dayState[d][g].applySubject(s, sign);
        }

        private void generateDay(int day)
        {
            G.reset();
            //G = new CirculationFlowGraph(150);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($" ----- {day} ----- ");

            //setting node demands
            G.setDemand(allTeachersNode, -(groups.Count * maxLessons));//redo later, when we can have daily lessons, different than maxLessons
            for (int g = 0; g < groups.Count; g++) G.setDemand(groupNode[g], maxLessons);
        
            //adding edges
            
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

                //Console.WriteLine($"requested = {requested}");

                for (int d = day+1; d <= workDays; d++)
                    requested -= teacherLeftLessons[d, t];

                int u = allTeachersNode;
                int v = teacherNode[t];
                int l = Math.Max(requested, 0);
                int c = maxLessons;

                //Console.WriteLine($"lowerBound = {l}");

                G.addEdge(u, v, l, c);
            }

            //connecting teachers to possible lessons
            //the capacities of the edges will be maxLessons,
            //since the actual limitations will be added later, when creating the higharchy tree
            for(int t = 0;t<teachers.Count;t++)
            {
                for(int g = 0;g<groups.Count;g++)
                {
                    foreach (int s in teacherGroup2Subjects[t, g])
                        G.addEdge(teacherNode[t], groupSubjectNode[g, s], 0, maxLessons);
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

                        //Console.WriteLine($"{groups[g].subject2Teacher[s].Item1.name} -> {lessonsLeft}");
                        for (int d = day+1; d <= workDays; d++)
                        {
                            int rm = Math.Min(dayState[day][g].getSubjectDayLim(s), teacherLeftLessons[d, teacherInd]);
                            rm = Math.Min(rm, groupLeftLessons[d, g]);

                            //Console.WriteLine($"{dayState[day][g].getSubjectDayLim(s)} {teacherLeftLessons[d, teacherInd]}");

                            lessonsLeft -= rm;
                        }
                        //Console.WriteLine(lessonsLeft);

                        int subjectLim = Math.Min(dayState[day][g].getSubjectDayLim(s),
                                                  dayState[day][g].getSubjectWeekLim(s));
                        groupSubjectEdge[g, s] = G.addEdge(x.nodeCode, x.parent.nodeCode, 
                                                           Math.Max(0, lessonsLeft),
                                                           subjectLim);
                        
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
                //for (int s = 0; s < groups[g].subject2Teacher.Count; s++)
                //    Console.WriteLine($"{groups[g].subject2Teacher[s].Item1.name} {G.getEdge(groupSubjectEdge[g, s])}");

                
                List<int> teacherInds = new List<int>();
                for (int s = 0; s < groups[g].subject2Teacher.Count; s++)
                {
                    for (int i = 0; i < G.getEdge(groupSubjectEdge[g, s]); i++)
                    {
                        teacherInds.Add(groupSubject2Teacher[g, s]);
                        updateLimits(day, g, s, +1);
                    }
                }

                
                //Console.Write($"{groups[g].name} => ");
                //Console.WriteLine(string.Join(" ", teacherInds.Select(t => teachers[t].name)));    
            }

            ScheduleCompleter sc = new ScheduleCompleter(dayState[day], teachers, maxLessons);
            
            DaySchedule ds = sc.gen(true);
            if (ds is null) ds = sc.gen(false);

            ds.print();
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

        public void generate()
        {
            initNodes();
            initArrays();

            for(int day = 1;day<=workDays;day++)
            {
                generateDay(day);
            }

            bool res = runDiagnostics();
            if (res == true) Console.WriteLine("Kein problem!");
            else Console.WriteLine("Es gibt ein problem!");
        }
    }
}
