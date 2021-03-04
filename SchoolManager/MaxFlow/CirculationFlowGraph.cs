using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SchoolManager.MaxFlow
{
    class CirculationFlowGraph
    {
        class CirculationEdge
        {
            public int u, v;
            public int l, c;
            public bool progressiveCost;

            public CirculationEdge() 
            {
                this.progressiveCost = false;
            }
            public CirculationEdge(int u, int v, int l, int c) : this()
            {
                this.u = u;
                this.v = v;
                this.l = l;
                this.c = c;
            }
            public CirculationEdge(int u, int v, int l, int c, bool progressiveCost) : this(u, v, l, c)
            {
                this.progressiveCost = progressiveCost;
            }
        }

        private List<int> edgeInd = new List<int>();
        private FlowGraph G;

        private List<CirculationEdge> edges;
        private int[] demand;
        private int s, t;

        private int auxNode;

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

            this.auxNode = demand.Length - 3;
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

        public int addEdge(int u, int v, int l, int c, bool progressiveCost)
        {
            if (l == c) progressiveCost = false;

            edges.Add(new CirculationEdge(u, v, l, c, progressiveCost));
            return edges.Count - 1;
        }

        public int getEdge(int ind)
        {
            return G.getEdge(edgeInd[ind]) + edges[ind].l;
        }

        public int eval()
        {
            bool hasProgessiveCost = edges.Any(e => e.progressiveCost==true);

            //getting rid of the lower bounds
            foreach(CirculationEdge e in edges)
            {
                demand[e.v] -= e.l;   
                demand[e.u] += e.l;   
            }

            //building the MaxFlowGraph
            if (hasProgessiveCost == true) G = new MinCostMaxFlowGraph(demand.Length, s, t);
            else G = new DinicMaxFlowGraph(demand.Length, s, t);

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
            {
                if (e.progressiveCost == false)
                    edgeInd.Add(G.addEdge(e.u, e.v, e.c - e.l));
                else
                {
                    edgeInd.Add(G.addEdge(auxNode, e.v, e.c - e.l));
                    for(int f = 1;f<=e.c-e.l;f++)
                        G.addEdge(e.u, auxNode, 1, (((long)1)<<f));

                    auxNode--;
                }
            }

            int maxFlow = (int)G.findFlow();
            Console.WriteLine($"maxFlow = {maxFlow}");

            return 69;
        }
    }
}
