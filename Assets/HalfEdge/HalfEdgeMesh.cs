using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ddg {
    public class HalfEdgeMesh {
        public HalfEdge[] halfedges     { get; protected set; }
        public Corner[] corners         { get; protected set; }
        public Vert[] verts             { get; protected set; }
        public Face[] faces             { get; protected set; }
        public Face[] boundaries        { get; protected set; }
        public int eulerCharactaristics { get; protected set; }

        public HalfEdgeMesh(Mesh mesh) {
            var vrts = mesh.vertices;
            var idxs = mesh.GetIndices(0);
            var keys = (Span<Key>)(stackalloc Key[idxs.Length]);
            PreallocateElements(vrts, idxs);

            for (var i = 0; i < idxs.Length; i += 3) {
                var i0 = i + 0;
                var i1 = i + 1;
                var i2 = i + 2;
                var h0 = new HalfEdge(i0);
                var h1 = new HalfEdge(i1);
                var h2 = new HalfEdge(i2);
                h0.vid = idxs[i0];
                h1.vid = idxs[i1];
                h2.vid = idxs[i2];
                //h0.vert = new Vert(i0, idxs[i0], vrts[idxs[i0]]);
                //h1.vert = new Vert(i1, idxs[i1], vrts[idxs[i1]]);
                //h2.vert = new Vert(i2, idxs[i2], vrts[idxs[i2]]);

                var f = new Face();
                f.hid = i2;
                faces[i / 3] = f;

                h0.next = h1; h0.prev = h2; h0.face = f;
                h1.next = h2; h1.prev = h0; h1.face = f;
                h2.next = h0; h2.prev = h1; h2.face = f;

                keys[i0] = new Key(idxs[i0], idxs[i1]);
                keys[i1] = new Key(idxs[i1], idxs[i2]);
                keys[i2] = new Key(idxs[i2], idxs[i0]);
                bool f0 = false;
                bool f1 = false;
                bool f2 = false;
                for (var j = 0; j < i; j++) {
                    var k = keys[j];
                    var h = halfedges[j];
                    if (!f0 && k.Twin(keys[i0])) { h.twin = h0; h0.twin = h; f0 = true; }
                    if (!f1 && k.Twin(keys[i1])) { h.twin = h1; h1.twin = h; f1 = true; }
                    if (!f2 && k.Twin(keys[i2])) { h.twin = h2; h2.twin = h; f2 = true; }
                    if (f0 && f1 && f2) break;
                }
                halfedges[i0] = h0;
                halfedges[i1] = h1;
                halfedges[i2] = h2;
                //verts[idxs[i0]] = h0.vert;
                //verts[idxs[i1]] = h1.vert;
                //verts[idxs[i2]] = h2.vert;
                verts[idxs[i0]] = new Vert(i0, idxs[i0], vrts[idxs[i0]]);
                verts[idxs[i1]] = new Vert(i1, idxs[i1], vrts[idxs[i1]]);
                verts[idxs[i2]] = new Vert(i2, idxs[i2], vrts[idxs[i2]]);
            }

            var hasTwinHes = new List<int>();
            for (var i = 0; i < idxs.Length; i++) {
                var h = halfedges[i];
                if (h.twin != null) hasTwinHes.Add(h.id);
            }

            var itr = 0;
            var cid = 0;
            var len = idxs.Length;
            //var hasTwinHes = halfedges.Where(h => h != null && h.twin!= null).Select(h => h.id);

            for (var i = 0; i < idxs.Length; i++) {
                var tgt = halfedges[i];
                if (tgt.twin == null) {
                    var f = new Face();
                    var cycl = new List<HalfEdge>();
                    var curr = tgt;
                    do {
                        var bh = new HalfEdge(len);
                        var next = curr.next;
                        while(hasTwinHes.Contains(next.id)) {
                            var twin = next.twin;
                            next = twin.next;
                        }
                        f.hid = i;
                        //bh.vert = next.vert;
                        bh.vid = next.vid;
                        bh.face = f;
                        bh.onBoundary = true;
                        bh.twin = curr;
                        curr.twin = bh;
                        cycl.Add(bh);
                        halfedges[len] = bh;
                        curr = next;
                        len++;
                    } while (curr != tgt);

                    var n = cycl.Count;
                    for(var j = 0; j < n; j++) {
                        var h = cycl[j];
                        h.next = cycl[(j + n - 1) % n];
                        h.prev = cycl[(j + 1) % n];
                    }
                    this.boundaries[itr++] = f; 
                }

                if (!tgt.onBoundary) {
                    var c = new Corner();
                    c.hid = i;
                    halfedges[i].corner = c;
                    this.corners[cid++] = c;
                }
            }
        }

        void PreallocateElements(Vector3[] vrts, int[] idcs){
            var sortedEdges = new HashSet<Key>();
            var nBoundaryHe = 0;
            for (var I = 0; I < idcs.Length; I += 3) {
                for (var J = 0; J < 3; J++) {
                    var K = (J + 1) % 3;
                    var i = idcs[I + J];
                    var j = idcs[I + K];
                    if (i > j) { var k = j; j = i; i = k; }
                    var key = new Key(i, j);
                    if (sortedEdges.Contains(key)) { nBoundaryHe--; }
                    else { sortedEdges.Add(key); nBoundaryHe++; }
                }
            }
            var nEdges = sortedEdges.Count;
            var nHalfedges = 2 * nEdges;
            var nInteriorHalfedges = nHalfedges - nBoundaryHe;
            this.halfedges = new HalfEdge[nHalfedges];
            this.corners = new Corner[nInteriorHalfedges];
            this.boundaries = new Face[nBoundaryHe];
            this.verts = new Vert[vrts.Length];
            this.faces = new Face[idcs.Length / 3];
            eulerCharactaristics = faces.Length - nEdges + verts.Length;
        }

        public struct Key {
            public int fr;
            public int to;
            public Key(int fr, int to){ this.fr = fr; this.to = to; }
            public bool Twin(Key that) => this.fr == that.to && this.to == that.fr;
        }
    }
}
