using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SchoolManager.MaxFlow
{
    class Edge
    {
        public int u { get; set; }
        public int v { get; set; }
        public int cap { get; set; }

        public Edge() { }
        public Edge(int u, int v, int cap)
        {
            this.u = u;
            this.v = v;
            this.cap = cap;
        }
    }

    class MaxFlowGraph
    {
        int source, sink;

        List<Edge> edges;
        List<List<int>> adj;

        List<int> startInd;
        public List<int> dist { get; set; }

        public MaxFlowGraph() 
        {
            this.dist = new List<int>();
            this.edges = new List<Edge>();
            this.startInd = new List<int>();
            this.adj = new List<List<int>>();
        }
        public MaxFlowGraph(int n, int s, int t) : this()
        {
            this.sink = t;
            this.source = s;

            for (int i = 0; i <= n; i++)
                this.adj.Add(new List<int>());
        }

        public void setSourceSink(int s, int t)
        {
            source = s;
            sink = t;
        }

        public void addEdge(int u, int v, int cap)
        {
            edges.Add(new Edge(u, v, cap));
            adj[u].Add(edges.Count-1);

            edges.Add(new Edge(u, v, cap));
            adj[u].Add(edges.Count-1);
        }

        public void bfs(int x)
        {
            for (int i = 0; i < dist.Count; i++) dist[i] = -1;
            Queue<int> q = new Queue<int>();

            q.Enqueue(x);
            dist[x] = 0;

            while(q.Count>0)
            {
                x = q.Peek();
                q.Dequeue();

                foreach(int eInd in adj[x])
                {
                    Edge e = edges[eInd];
                    if(dist[e.v]==-1 && e.cap>0)
                    {
                        dist[e.v] = dist[x] + 1;
                        q.Enqueue(e.v);
                    }
                }
            }
        }

        public int dfs(int x, int minVal)
        {
            if (x == sink) return minVal;

            foreach(int eInd in adj[x])
            {
                Edge e = edges[eInd];
                if(dist[e.v]==dist[x]+1 && e.cap>0)
                {
                    int flow = dfs(e.v, Math.Min(minVal, e.cap));
                    if(flow!=-1)
                    {
                        edges[eInd].cap -= flow;
                        edges[eInd^1].cap += flow;

                        return flow;
                    }
                }
            }

            return -1;
        }

        public int Dinic()
        {
            int maxFlow = 0;
            while(true)
            {
                for (int i = 0; i < startInd.Count; i++) startInd[i] = 0;

                bfs(source);
                if (dist[sink] == -1) break;

                while(true)
                {
                    int add = dfs(source, int.MaxValue);
                    if (add == -1) break;

                    maxFlow += add;
                }
            }

            return maxFlow;
        }
    }
}
