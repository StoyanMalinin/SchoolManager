using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.MaxFlow
{
    abstract class FlowGraph
    {
        public abstract int getEdge(int ind);
        public abstract int addEdge(int u, int v, int cap);
        public abstract int addEdge(int u, int v, int cap, long cost);
        public abstract long findFlow();
    }
}
