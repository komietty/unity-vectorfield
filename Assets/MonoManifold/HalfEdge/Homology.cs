using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ddg {
    public class Homology {
        public HeGeom geom;
        public Dictionary<int, int> vertParentList;
        public Dictionary<int, int> faceParentList;

        public Homology(HeGeom g) {
            geom = g;
            vertParentList = new Dictionary<int, int>();
            faceParentList = new Dictionary<int, int>();
        }

        public void BuildPrimalSpanningTree() {
            foreach (var v in geom.Verts) vertParentList.Add(v.vid, v.vid);
            var rid = geom.Verts[0].vid;
            var queue = new Queue<int>();
            queue.Enqueue(rid);
            while (queue.Count != 0) {
                var uid = queue.Dequeue();
                foreach (var v in geom.GetAdjacentVerts(geom.Verts[uid])) {
                    var vid = v.vid;
                    if (vertParentList[vid] == vid && vid != rid) {
                        vertParentList[vid] = uid;
                        queue.Enqueue(vid);
                    }
                }
            }
        }

        public void BuildDualSpanningCotree() {
            var queue = new Queue<int>();
            var visit = new Dictionary<int, bool>();
            var f0 = geom.Faces[0];
            foreach (var f in geom.Faces) visit.Add(f.fid, false);
            queue.Enqueue(f0.fid);
            while (queue.Count > 0) {
                var tid = queue.Dequeue();
                visit[tid] = true;
                foreach (var h in geom.GetAdjacentHalfedges(geom.Faces[tid])) {
                    if (!InPrimalSpanningTree(h)) {
                        var fid = h.twin.face.fid;
                        if (!visit[fid]) {
                            faceParentList[fid] = tid;
                            queue.Enqueue(fid);
                        }
                    }
                }
            }
        }

        bool InPrimalSpanningTree(HalfEdge h) {
            var uid = h.vid;
            var vid = h.twin.vid;
            return vertParentList[uid] == vid || vertParentList[vid] == uid;
        }

/*
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
*/
    }
}
