using System.Collections.Generic;

namespace VectorField {
    public class HomologyGenerator {
        private readonly HeGeom G;
        private readonly int[] vertParent;
        private readonly int[] faceParent;

        public HomologyGenerator(HeGeom g) {
            G = g;
            vertParent = new int[g.nVerts];
            faceParent = new int[g.nFaces];
        }

        void BuildPrimalSpanningTree() {
            foreach (var v in G.Verts) vertParent[v.vid] = v.vid;
            var rtid = G.Verts[10].vid;
            var queue = new Queue<int>();
            queue.Enqueue(rtid);
            while (queue.Count != 0) {
                var uid = queue.Dequeue();
                foreach (var v in G.GetAdjacentVerts(G.Verts[uid])) {
                    var vid = v.vid;
                    if (vertParent[vid] == vid && vid != rtid) {
                        vertParent[vid] = uid;
                        queue.Enqueue(vid);
                    }
                }
            }
        }

        void BuildDualSpanningCotree() {
            foreach (var f in G.Faces) faceParent[f.fid] = f.fid;
            var queue = new Queue<int>();
            var f0 = G.Faces[0];
            queue.Enqueue(f0.fid);
            while (queue.Count > 0) {
                var fid = queue.Dequeue();
                foreach (var h in G.GetAdjacentHalfedges(G.Faces[fid])) {
                    if (!InPrimalSpanningTree(h)) {
                        var gid = h.twin.face.fid;
                        if (faceParent[gid] == gid && gid != f0.fid) {
                            faceParent[gid] = fid;
                            queue.Enqueue(gid);
                        }
                    }
                }
            }
        }

        bool InPrimalSpanningTree(HalfEdge h) {
            var uid = h.vid;
            var vid = h.twin.vid;
            return vertParent[uid] == vid || vertParent[vid] == uid;
        }

        bool InDualSpanningTree(HalfEdge h) {
            var fid = h.face.fid;
            var gid = h.twin.face.fid;
            return faceParent[fid] == gid || faceParent[gid] == fid;
        }

        HalfEdge SharedHalfEdge(Face f, Face g) {
            foreach (var h in G.GetAdjacentHalfedges(f))
                if (h.twin.face.fid == g.fid) return h;
            throw new System.Exception("No half edges are shared");
        }

        public List<List<HalfEdge>> BuildGenerators() {
            BuildPrimalSpanningTree();
            BuildDualSpanningCotree();
            var gens = new List<List<HalfEdge>>();

            foreach (var e in G.Edges) {
                var h = G.halfedges[e.hid]; 
                if (!InPrimalSpanningTree(h) && !InDualSpanningTree(h)) {
                    var tmp1 = new List<HalfEdge>();
                    var tmp2 = new List<HalfEdge>();
                    var f = h.face;
                    var c1 = 0;
                    var c2 = 0;
                    while(faceParent[f.fid] != f.fid){
                        c1++;
                        var p = G.Faces[faceParent[f.fid]];
                        tmp1.Add(SharedHalfEdge(f, p));
                        f = p;
                    }

                    f = h.twin.face;
                    while(faceParent[f.fid] != f.fid){
                        c2++;
                        var p = G.Faces[faceParent[f.fid]];
                        tmp2.Add(SharedHalfEdge(f, p));
                        f = p;
                    }
                    var m = tmp1.Count - 1;
				    var n = tmp2.Count - 1;
                    while (tmp1[m] == tmp2[n]) { m--; n--; }
                    var g = new List<HalfEdge> { h };
                    for (var i = 0; i <= m; i++) g.Add(tmp1[i].twin);
                    for (var i = n; i >= 0; i--) g.Add(tmp2[i]);
                    gens.Add(g);
                }
            }
            return gens;
        }
    }
}
