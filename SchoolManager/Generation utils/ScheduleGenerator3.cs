using SchoolManager.MaxFlow;
using SchoolManager.School_Models;
using SchoolManager.School_Models.Higharchy;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
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

        private int[] teacherNode;
        private int[,] groupSubjectNode;

        private int[] groupNode; 
        private TreeNode[] groupHigharchy;

        private void initNodes()
        {
            int node = 1;
            CirculationFlowGraph G = new CirculationFlowGraph();

            //initializing nodes
            for(int g = 0;g<groups.Count;g++)
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

                Console.WriteLine($"Group: {groups[g].name}");
                groupHigharchy[g].printTree();
            }
            for(int g = 0;g<groups.Count;g++)
            {
                void findSubjects(TreeNode x)
                {
                    if(x.GetType()==typeof(SubjectTreeNode))
                    {
                        for(int s = 0;s<groups[g].subject2Teacher.Count;s++)
                        {
                            if(groups[g].subject2Teacher[s].Item1.Equals((x as SubjectTreeNode).s)==true)
                            {
                                groupSubjectNode[g, s] = x.nodeCode;
                                Console.WriteLine($"{g} -> [{s} <=> {x.nodeCode}]");
                                
                                break;
                            }
                        }

                        return;
                    }

                    foreach (TreeNode y in x.children)
                        findSubjects(y);
                }

                findSubjects(groupHigharchy[g]);
            }

            //setting node demands
            for (int g = 0; g < groups.Count; g++) G.setDemand(groupNode[g], workDays);

        }

        private void generateDay(int day)
        {

        }

        public void generate()
        {
            initNodes();

            for(int day = 1;day<=workDays;day++)
            {

            }
        }
    }
}
