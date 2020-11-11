using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.School_Models.Higharcy
{
    class TreeNode
    {
        public string name;
        public int nodeCode = -1;
        public List<TreeNode> children;
    
        public void encodeTree(ref int nodeCnt)
        {
            nodeCode = nodeCnt;
            nodeCnt++;

            foreach (TreeNode x in children)
                x.encodeTree(ref nodeCnt);
        }

        public void printTree(int depth = 0)
        {
            for (int i = 0; i < depth; i++) Console.Write("  ");
            Console.WriteLine(name);

            foreach (TreeNode x in children)
                x.printTree(depth+1);
        }
    }

    class LimitationTreeNode : TreeNode
    {
        public LimitationGroup lg;

        public LimitationTreeNode() 
        {
            this.children = new List<TreeNode>();
        }
        public LimitationTreeNode(LimitationGroup lg) : this()
        {
            this.name = lg.name;
            this.lg = lg;
        }
        public LimitationTreeNode(string name, LimitationGroup lg) : this()
        {
            this.name = name;
            this.lg = lg;
        }

        public void addChild(TreeNode x)
        {
            children.Add(x);
        }     
    }

    class SubjectTreeNode : TreeNode
    {
        public Subject s;

        public SubjectTreeNode()
        {
            this.children = new List<TreeNode>();
        }
        public SubjectTreeNode(Subject s) : this()
        {
            this.name = s.name;
            this.s = s;
        }
        public SubjectTreeNode(string name, Subject s) : this()
        {
            this.name = name;
            this.s = s;
        }
    }
}
