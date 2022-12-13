using System.Collections;
using System.Collections.Generic;

namespace ddg {
    public class Homology {
        public HeGeom geom;
        public Dictionary<Vert, Vert> vertParentList;
        public Dictionary<Face, Face> faceParentList;

        public Homology(HeGeom g) {
            geom = g;
            vertParentList = new Dictionary<Vert, Vert>();
            faceParentList = new Dictionary<Face, Face>();
        }

        public void BuildPrimalSpanningTree() {
            foreach (var v in geom.Verts) vertParentList.Add(v, v);
            var r = geom.Verts[0];
            var q = new Queue<Vert>();
            q.Enqueue(r);
            while (q.Count != 0) {
                var u = q.Dequeue();
                foreach (var v in geom.GetAdjacentVerts(u)) {
                    if (vertParentList[v].vid == v.vid && v.vid != r.vid) {
                        vertParentList[v] = u;
                        q.Enqueue(v);
                    }
                }
            }
        }

        void BuildDualSpanningCotree() {
            var q = new Queue<Face>();
            var visit = new Dictionary<Face, bool>();
            var f0 = geom.Faces[0];
            foreach (var f in geom.Faces) { visit.Add(f, false); }
            q.Enqueue(f0);
            while (q.Count > 0) {
                var ft = q.Dequeue();
                visit[ft] = true;
                foreach (var h in geom.GetAdjacentHalfedges(ft)) {
                    if (!InPrimalSpanningTree(h)) {
                        var f = h.twin.face;
                        if (!visit[f]) {
                            faceParentList[f] = ft;
                            q.Enqueue(f);
                        }
                    }
                }
            }
        }

        bool InPrimalSpanningTree(HalfEdge h) {
            var u = geom.Verts[h.vid];
            var v = geom.Verts[h.twin.vid];
            return vertParentList[u].vid == v.vid || vertParentList[v].vid == u.vid;
        }

        bool InDualSpanningTree(HalfEdge h) {
            var f = h.face;
            var g = h.twin.face;
            return faceParentList[f].fid == g.fid || faceParentList[g].fid == f.fid;
        }

        HalfEdge SharedHalfEdge(Face f, Face g) {
            foreach (var h in geom.GetAdjacentHalfedges(f)) {
                if (h.twin.face.fid == g.fid) return h;
            }
            throw new System.Exception("no halfedge shared");
        }

        public HalfEdge[] BuildGenerators() {
            BuildPrimalSpanningTree();
            BuildDualSpanningCotree();
            var gens = new List<HalfEdge>();

            foreach (var e in this.geom.Edges) {
                var h = geom.halfedges[e.hid]; 
                if (!InPrimalSpanningTree(h) && !InDualSpanningTree(h)) {
                    var tmp1 = new List<HalfEdge>();
                    var tmp2 = new List<HalfEdge>();
                    var f = h.face;
                    while(faceParentList[f].fid != f.fid){
                        var p = faceParentList[f];
                        tmp1.Add(SharedHalfEdge(f, p));
                        f = p;
                    }
                    f = h.twin.face;
                    while(faceParentList[f].fid != f.fid){
                        var p = faceParentList[f];
                        tmp2.Add(SharedHalfEdge(f, p));
                        f = p;
                    }
                    var m = tmp1.Count - 1;
				    var n = tmp2.Count - 1;
                    while (tmp1[m] == tmp2[n]) { m--; n--; }
                    var generator = new List<HalfEdge>() { h };
                    for (var i = 0; i <= m; i++) generator.Add(tmp1[i].twin);
                    for (var i = n; i >= 0; i--) generator.Add(tmp2[i]);
                    gens.AddRange(generator);
                }
            }
            return gens.ToArray();
        }
    }
}
