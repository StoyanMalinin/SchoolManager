using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;

namespace SchoolManager.MaxFlow
{
    class MinCostMaxFlowGraph : FlowGraph
    {
        const int inf = (int)1e9 + 5;
        const long infLL = (long)1e18 + 5;

        int n;
        int source, sink;

        List<Edge> edges;
        List<List<int>> adj;

        int[] lastEdge;
        long[] dist;

        public MinCostMaxFlowGraph()
        {
            this.edges = new List<Edge>();
            this.adj = new List<List<int>>();
        }
        public MinCostMaxFlowGraph(int n, int s, int t) : this()
        {
            this.n = n;
            this.sink = t;
            this.source = s;

            this.dist = new long[n + 5];
            this.lastEdge = new int[n + 5];
            for (int i = 0; i <= n + 5; i++)
            {
                this.adj.Add(new List<int>());
            }
        }

        public override int addEdge(int u, int v, int cap)
        {
            return addEdge(u, v, cap, 0);
        }

        class DijkstraNodeState : IComparable<DijkstraNodeState>
        {
            public int node;
            public long dist;

            public DijkstraNodeState() { }
            public DijkstraNodeState(int node, long dist)
            {
                this.node = node;
                this.dist = dist;
            }

            public int CompareTo([AllowNull] DijkstraNodeState other)
            {
                if(dist!=other.dist) return dist.CompareTo(other.dist);
                return node.CompareTo(other.node);
            }
        }

        private void useNode(int x, SortedSet <DijkstraNodeState> pq)
        {
            foreach(int e in adj[x])
            {
                if(dist[edges[e].v] > dist[x] + edges[e].cost && edges[e].cap>0)
                {
                    dist[edges[e].v] = dist[x] + edges[e].cost;
                    lastEdge[edges[e].v] = e^1;

                    pq.Add(new DijkstraNodeState(edges[e].v, dist[edges[e].v]));
                }
            }
        }

        private long Dijkstra()
        {
            for(int x = 0;x<dist.Length;x++)
            {
                dist[x] = infLL;
            }

            lastEdge[source] = -1;
            dist[source] = 0;

            SortedSet<DijkstraNodeState> pq = new SortedSet<DijkstraNodeState>();
            pq.Add(new DijkstraNodeState(source, dist[source]));

            while(pq.Count>0)
            {
                DijkstraNodeState x = pq.Min;
                pq.Remove(x);

                if (x.dist != dist[x.node]) continue;
                useNode(x.node, pq);
            }

            return dist[sink];
        }

        public void test()
        {
            SortedSet<DijkstraNodeState> pq = new SortedSet<DijkstraNodeState>();

            pq.Add(new DijkstraNodeState(1, 1));
            pq.Add(new DijkstraNodeState(4, 4));
            pq.Add(new DijkstraNodeState(3, 3));
            pq.Add(new DijkstraNodeState(3, 3));
            pq.Add(new DijkstraNodeState(2, 2));

            Console.WriteLine(pq.Min.dist);
            pq.Remove(pq.Min);
            Console.WriteLine(pq.Min.dist);
        }

        public override long findFlow()
        {
            long flow = 0;
            long flowCost = 0;

            while(true)    
            {
                long d = Dijkstra();
                if (d == infLL) break;

                int bottleneck = inf;

                int x = sink;
                while(x!=source)
                {
                    bottleneck = Math.Min(bottleneck, edges[lastEdge[x]^1].cap);
                    x = edges[lastEdge[x]].v;
                }

                x = sink;
                while(x!=source)
                {
                    edges[lastEdge[x]].cap += bottleneck;
                    edges[lastEdge[x]^1].cap -= bottleneck;

                    x = edges[lastEdge[x]].v;
                }

                flow += bottleneck;
                flowCost += bottleneck*d;
            }

            return flow;
        }

        public override int getEdge(int ind)
        {
            return edges[ind ^ 1].cap;
        }

        public override int addEdge(int u, int v, int cap, long cost)
        {
            edges.Add(new Edge(u, v, cap, cost));
            adj[u].Add(edges.Count - 1);

            edges.Add(new Edge(v, u, 0, cost));
            adj[v].Add(edges.Count - 1);

            return edges.Count - 2;
        }
    }
}
