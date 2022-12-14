using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            foreach (var f in geom.Faces) faceParentList.Add(f.fid, f.fid);
            var queue = new Queue<int>();
            var f0 = geom.Faces[0];
            queue.Enqueue(f0.fid);
            while (queue.Count > 0) {
                var fid = queue.Dequeue();
                foreach (var h in geom.GetAdjacentHalfedges(geom.Faces[fid])) {
                    if (!InPrimalSpanningTree(h)) {
                        var gid = h.twin.face.fid;
                        if (faceParentList[gid] == gid && gid != f0.fid) {
                            faceParentList[gid] = fid;
                            queue.Enqueue(gid);
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

        bool InDualSpanningTree(HalfEdge h) {
            var fid = h.face.fid;
            var gid = h.twin.face.fid;
            return faceParentList[fid] == gid || faceParentList[gid] == fid;
        }

        HalfEdge SharedHalfEdge(Face f, Face g) {
            foreach (var h in geom.GetAdjacentHalfedges(f)) {
                if (h.twin.face.fid == g.fid) return h;
            }
            throw new System.Exception("no halfedge shared");
        }

        public List<List<HalfEdge>> BuildGenerators() {
            BuildPrimalSpanningTree();
            BuildDualSpanningCotree();
            var gens = new List<List<HalfEdge>>();

            foreach (var e in this.geom.Edges) {
                var h = geom.halfedges[e.hid]; 
                if (!InPrimalSpanningTree(h) && !InDualSpanningTree(h)) {
                    var tmp1 = new List<HalfEdge>();
                    var tmp2 = new List<HalfEdge>();
                    var f = h.face;
                    var c1 = 0;
                    var c2 = 0;
                    while(faceParentList[f.fid] != f.fid){
                        c1++;
                        var p = geom.Faces[faceParentList[f.fid]];
                        tmp1.Add(SharedHalfEdge(f, p));
                        f = p;
                    }

                    f = h.twin.face;
                    while(faceParentList[f.fid] != f.fid){
                        c2++;
                        var p = geom.Faces[faceParentList[f.fid]];
                        tmp2.Add(SharedHalfEdge(f, p));
                        f = p;
                    }
                    var m = tmp1.Count - 1;
				    var n = tmp2.Count - 1;
                    while (tmp1[m] == tmp2[n]) { m--; n--; }
                    var generator = new List<HalfEdge>() { h };
                    for (var i = 0; i <= m; i++) generator.Add(tmp1[i].twin);
                    for (var i = n; i >= 0; i--) generator.Add(tmp2[i]);
                    gens.Add(generator);
                }
            }
            return gens;
        }
    }
}
