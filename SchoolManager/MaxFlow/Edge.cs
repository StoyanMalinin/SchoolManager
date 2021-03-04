using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManager.MaxFlow
{
    class Edge
    {
        public int u;
        public int v;
        public int cap;
        public long cost;

        public Edge() { }
        public Edge(int u, int v, int cap)
        {
            this.u = u;
            this.v = v;
            this.cap = cap;
        }
        public Edge(int u, int v, int cap, long cost) : this(u, v, cap)
        {
            this.cost = cost;
        }
    }
}
