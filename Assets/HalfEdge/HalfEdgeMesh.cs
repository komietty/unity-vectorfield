using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ddg {
    public class HalfEdgeMesh : MonoBehaviour {
        public HalfEdge[] halfedges;
        public Vert[] verts;
        public Edge[] edges;
        public Face[] faces;
        public Coner[] coners;
        public Face[] boundaries;

        void Start() {
            var m = GetComponentInChildren<MeshFilter>().sharedMesh;
            var vrts = m.vertices;
            var idxs = m.GetIndices(0);
            var idxl = idxs.Length;
            var keys = (Span<Key>)(stackalloc Key[idxs.Length]);
            var hes  = new List<HalfEdge>(idxs.Length);

            for (var i = 0; i < idxl; i += 3) {
                var i0 = i + 0;
                var i1 = i + 1;
                var i2 = i + 2;

                var h0 = new HalfEdge();
                var h1 = new HalfEdge();
                var h2 = new HalfEdge();

                h0.vert = new Vert(i0, idxs[i0]);
                h1.vert = new Vert(i1, idxs[i1]);
                h2.vert = new Vert(i2, idxs[i2]);

                h0.nextid = i1; h0.previd = i2; h0.twinid = -1; h0.id = i0;
                h1.nextid = i2; h1.previd = i0; h1.twinid = -1; h1.id = i1;
                h2.nextid = i0; h2.previd = i1; h2.twinid = -1; h2.id = i2;

                keys[i0] = new Key(idxs[i0], idxs[i1]);
                keys[i1] = new Key(idxs[i1], idxs[i2]);
                keys[i2] = new Key(idxs[i2], idxs[i0]);

                for (var j = 0; j < i; j++) {
                    var k = keys[j];
                    var h = hes[j];
                    if      (k.Twin(keys[i0])) { h.twinid = i0; h0.twinid = j; }
                    else if (k.Twin(keys[i1])) { h.twinid = i1; h1.twinid = j; }
                    else if (k.Twin(keys[i2])) { h.twinid = i2; h2.twinid = j; }
                    hes[j] = h;
                }

                hes.Add(h0); 
                hes.Add(h1);
                hes.Add(h2);
            }

            var boundaries = new List<Face>();
            var count = idxl;
            var hasTwinHes = new List<int>();
            foreach (var h in hes) { if (h.twinid != -1) hasTwinHes.Add(h.id); }

            for (var i = 0; i < idxl; i++) {
                var tgt = hes[i];
                if (tgt.twinid == -1) {
                    var f = new Face();
                    var cycl = new List<HalfEdge>();
                    var curr = tgt;
                    do {
                        var bh = new HalfEdge();
                        var next = hes[curr.nextid];
                        while(hasTwinHes.Contains(next.id)) {
                            var twin = hes[next.twinid];
                            next = hes[twin.nextid];
                        }
                        f.hid = i;
                        bh.vert = next.vert;
                        bh.edge = curr.edge;
                        bh.face = f;
                        bh.onBoundary = true;
                        bh.twinid = i;
                        curr.twinid = count;
                        hes[curr.id] = curr;
                        cycl.Add(bh);
                        hes.Add(bh);
                        count++;
                        curr = next;
                    } while (curr.id != tgt.id);

                    var n = cycl.Count;
                    Debug.Log(n);
                    for(var j = 0; j < n; j++) {
                        var _h = cycl[j];
                        _h.nextid = (j + n - 1) % n;
                        _h.previd = (j + 1) % n;
                        cycl[j] = _h;
                    }

                    boundaries.Add(f);
                }
            }

            this.boundaries = boundaries.ToArray();
            this.halfedges = hes.ToArray();
        }

        public struct Key {
            public int fr;
            public int to;
            public Key(int fr, int to){ this.fr = fr; this.to = to; }
            public bool Twin(Key that) => this.fr == that.to && this.to == that.fr;
        }
    }
}
