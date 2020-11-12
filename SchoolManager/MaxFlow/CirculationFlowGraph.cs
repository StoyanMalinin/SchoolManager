using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SchoolManager.MaxFlow
{
    //this assumes that we have only one source and sink
    class CirculationFlowGraph
    {
        class CirculationEdge
        {
            public int u, v;
            public int l, c;

            public CirculationEdge() { }
            public CirculationEdge(int u, int v, int l, int c)
            {
                this.u = u;
                this.v = v;
                this.l = l;
                this.c = c;
            }
        }

        private List<int> edgeInd = new List<int>();
        private MaxFlowGraph G;

        private List<CirculationEdge> edges;
        private int[] demand;
        private int s, t;

        public CirculationFlowGraph() 
        {
            this.edgeInd = new List<int>();
            this.edges = new List<CirculationEdge>();
        }
        public CirculationFlowGraph(int n) : this()
        {
            this.demand = new int[n + 15];
            for (int x = 0; x < demand.Length; x++) demand[x] = 0;

            s = demand.Length - 2;
            t = demand.Length - 1;
        }
       
        public void reset()
        {
            this.edges = new List<CirculationEdge>();
            this.edgeInd = new List<int>();
            for (int x = 0; x < demand.Length; x++) demand[x] = 0;
        }

        public void setDemand(int x, int newDemand)
        {
            demand[x] = newDemand;
        }

        public int addEdge(int u, int v, int l, int c)
        {
            edges.Add(new CirculationEdge(u, v, l, c));
            return edges.Count - 1;
        }

        public int getEdge(int ind)
        {
            return G.getEdge(edgeInd[ind]) + edges[ind].l;
        }

        public int eval()
        {
            //getting rid of the lower bounds
            foreach(CirculationEdge e in edges)
            {
                demand[e.v] -= e.l;   
                demand[e.u] += e.l;   
            }

            //building the MaxFlowGraph
            G = new MaxFlowGraph(demand.Length, s, t);

            //connecting demand/supply nodes
            for(int x = 0;x<demand.Length;x++)
            {
                if (s == x) continue;
                if (t == x) continue;

                if (demand[x] < 0) G.addEdge(s, x, -demand[x]);
                else if (demand[x] > 0) G.addEdge(x, t, demand[x]);
            }

            //doing the actual edges
            foreach (CirculationEdge e in edges)
                edgeInd.Add(G.addEdge(e.u, e.v, e.c-e.l));

            int maxFlow = (int)G.Dinic();
            Console.WriteLine($"maxFlow = {maxFlow}");

            return 69;
        }
    }
}
