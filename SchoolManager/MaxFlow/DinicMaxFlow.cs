using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SchoolManager.MaxFlow
{
    class DinicMaxFlowGraph : FlowGraph
    {
        int source, sink;

        List<Edge> edges;
        List<List<int>> adj;

        List<int> startInd;
        public List<int> dist { get; set; }

        public DinicMaxFlowGraph()
        {
            this.dist = new List<int>();
            this.edges = new List<Edge>();
            this.startInd = new List<int>();
            this.adj = new List<List<int>>();
        }
        public DinicMaxFlowGraph(int n, int s, int t) : this()
        {
            this.sink = t;
            this.source = s;

            for (int i = 0; i <= n+5; i++)
            {
                this.adj.Add(new List<int>());
                this.startInd.Add(0);
                this.dist.Add(0);
            }
        }

        public void setSourceSink(int s, int t)
        {
            source = s;
            sink = t;
        }

        public override int addEdge(int u, int v, int cap)
        {
            edges.Add(new Edge(u, v, cap));
            adj[u].Add(edges.Count - 1);

            edges.Add(new Edge(v, u, 0));
            adj[v].Add(edges.Count - 1);

            return edges.Count - 2;
        }

        public override int getEdge(int ind)
        {
            return edges[ind^1].cap;
        }

        public void bfs(int x)
        {
            for (int i = 0; i < dist.Count; i++) dist[i] = -1;
            Queue<int> q = new Queue<int>();

            q.Enqueue(x);
            dist[x] = 0;

            while (q.Count > 0)
            {
                x = q.Peek();
                q.Dequeue();

                foreach (int eInd in adj[x])
                {
                    Edge e = edges[eInd];
                    if (dist[e.v] == -1 && e.cap > 0)
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

            for (int i = startInd[x]; i < adj[x].Count; i++)
            {
                int eInd = adj[x][i];
                Edge e = edges[eInd];

                if (dist[e.v] == dist[x] + 1 && e.cap > 0)
                {
                    int flow = dfs(e.v, Math.Min(minVal, e.cap));
                    if (flow != -1)
                    {
                        edges[eInd].cap -= flow;
                        edges[eInd ^ 1].cap += flow;

                        return flow;
                    }
                }

                startInd[x]++;
            }

            return -1;
        }

        public override long findFlow()
        {
            long maxFlow = 0;
            while (true)
            {
                for (int i = 0; i < startInd.Count; i++) startInd[i] = 0;

                bfs(source);
                if (dist[sink] == -1) break;

                while (true)
                {
                    int add = dfs(source, 1);
                    if (add == -1) break;

                    maxFlow += add;
                }
            }

            return maxFlow;
        }

        public override int addEdge(int u, int v, int cap, long cost)
        {
            throw new NotImplementedException();
        }
    }
}
