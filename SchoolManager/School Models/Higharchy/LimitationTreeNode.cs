using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SchoolManager.School_Models.Higharchy
{
    class TreeNode
    {
        public string name;
        public int nodeCode;

        public TreeNode parent;
        public List<TreeNode> children;

        protected TreeNode()
        {
            this.name = "";
            this.nodeCode = -1;
            this.parent = null;
            this.children = new List<TreeNode>();
        }
        protected TreeNode(TreeNode other)
        {
            this.name = other.name;
            this.parent = other.parent;
            this.nodeCode = other.nodeCode;

            this.children = new List<TreeNode>();
            foreach(TreeNode x in other.children)
            {
                this.children.Add(x.Clone());
            }
        }

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
            Console.WriteLine($"{name} {nodeCode}");

            foreach (TreeNode x in children)
                x.printTree(depth+1);
        }

        public virtual TreeNode Clone()
        {
            return new TreeNode(this);
        }
    }

    class LimitationTreeNode : TreeNode
    {
        public LimitationGroup lg;

        public LimitationTreeNode() : base()
        {

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
        public LimitationTreeNode(LimitationTreeNode other) : base(other)
        {
            this.lg = other.lg;
        }

        public void addChild(TreeNode x)
        {
            children.Add(x);
            x.parent = this;
        }

        public override TreeNode Clone()
        {
            return new LimitationTreeNode(this);
        }
    }

    class SubjectTreeNode : TreeNode
    {
        public Subject s;

        public SubjectTreeNode() : base()
        {

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
        public SubjectTreeNode(SubjectTreeNode other) : base(other)
        {
            this.s = other.s;
        }

        public override TreeNode Clone()
        {
            return new SubjectTreeNode(this);
        }
    }
}
