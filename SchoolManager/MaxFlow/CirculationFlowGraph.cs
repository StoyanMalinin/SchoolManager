﻿using System;
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

        List<CirculationEdge> edges;
        int[] demand;

        public CirculationFlowGraph() 
        {
            this.edges = new List<CirculationEdge>();
        }
        public CirculationFlowGraph(int n) : this()
        {
            this.demand = new int[n + 5];
            for (int x = 0; x < demand.Length; x++) demand[x] = 0;
        }
       
        public void reset()
        {
            this.edges = new List<CirculationEdge>();
            for (int x = 0; x < demand.Length; x++) demand[x] = 0;
        }

        public void setDemand(int x, int newDemand)
        {
            demand[x] = newDemand;
        }

        public void addEdge(int u, int v, int l, int c)
        {
            edges.Add(new CirculationEdge(u, v, l, c));
        }

        public int eval()
        {
            foreach(CirculationEdge e in edges)
            {
                demand[e.v] -= e.l;   
                demand[e.u] += e.l;   
            }

            foreach (CirculationEdge e in edges)
            {
                e.c -= e.l;
                e.l = 0;
            }

            

            return 69;
        }
    }
}