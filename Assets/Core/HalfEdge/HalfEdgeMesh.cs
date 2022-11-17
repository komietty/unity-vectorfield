using System.Collections.Generic;
using UnityEngine;
using System;

namespace ddg {
    public class HalfEdgeMesh {
        public HalfEdge[] halfedges     { get; private set; }
        public int eulerCharactaristics { get; private set; }
        public ReadOnlySpan<Vert> Verts => verts.AsSpan();
        public ReadOnlySpan<Face> Faces => faces.AsSpan();
        public Span<Vector3> Pos => pos.AsSpan();
        public int nVerts { get; private set; }
        public int nFaces { get; private set; }
        Vert[] verts;
        Face[] faces;
        Face[] bunds;
        Corner[] corners;
        public Vector3[] pos;

        public HalfEdgeMesh(Mesh mesh) {
            pos = mesh.vertices;
            var idxs = new ReadOnlySpan<int>(mesh.triangles);
            Preallocate(new ReadOnlySpan<Vector3>(pos), idxs);

            var alones = new List<HalfEdge>(3);
            var tripls = new List<HalfEdge>(3);

            for (var i = 0; i < idxs.Length; i += 3) {
                tripls.Clear();
                var n = i + 3;
                var f = new Face(i + 2);
                faces[i / 3] = f;

                for (int j = i; j < n; j++) { tripls.Add(new HalfEdge(j, idxs[j])); }
                for (int j = 0; j < 3; j++) {
                    tripls[j].next = tripls[(j + 1) % 3];
                    tripls[j].prev = tripls[(j + 2) % 3];
                    tripls[j].face = f;
                }
                for (int k = 0; k < 3; k++) {
                    var fg = false;
                    var hb = tripls[k];
                    for (var j = 0; j < alones.Count; j++) {
                        var ha = alones[j];
                        if (ha.vid == hb.next.vid && ha.next.vid == hb.vid) {
                            ha.twin = hb;
                            hb.twin = ha;
                            alones.Remove(ha);
                            fg = true;
                            break;
                        }
                    }
                    if(!fg) alones.Add(hb);
                }

                for (int j = 0; j < 3; j++) { halfedges[i + j] = tripls[j]; }
                for (int j = i; j < n; j++) { verts[idxs[j]] = new Vert(j, idxs[j]); }
            }

            var hasTwins = new List<int>();
            for (var i = 0; i < idxs.Length; i++) {
                var h = halfedges[i];
                if (h.twin != null) hasTwins.Add(h.id);
            }

            var itr = 0;
            var cid = 0;
            var len = idxs.Length;
            var cyc = new List<HalfEdge>();

            for (var i = 0; i < idxs.Length; i++) {
                var tgt = halfedges[i];
                if (tgt.twin == null) {
                    Face f;
                    cyc.Clear();
                    var curr = tgt;
                    do {
                        var bh = new HalfEdge(len);
                        var next = curr.next;
                        while (hasTwins.Contains(next.id)) { next = next.twin.next; }
                        f = new Face(i);
                        bh.vid = next.vid;
                        bh.face = f;
                        bh.onBoundary = true;
                        bh.twin = curr;
                        curr.twin = bh;
                        cyc.Add(bh);
                        halfedges[len] = bh;
                        curr = next;
                        len++;
                    } while (curr != tgt);

                    var n = cyc.Count;
                    for(var j = 0; j < n; j++) {
                        var h = cyc[j];
                        h.next = cyc[(j + n - 1) % n];
                        h.prev = cyc[(j + 1) % n];
                    }
                    this.bunds[itr++] = f; 
                }

                if (!tgt.onBoundary) {
                    var c = new Corner(i);
                    halfedges[i].corner = c;
                    this.corners[cid++] = c;
                }
            }
        }

        void Preallocate(ReadOnlySpan<Vector3> vrts, ReadOnlySpan<int> idcs){
            var sortedEdges = new HashSet<(int, int)>();
            var nBoundaryHe = 0;
            for (var I = 0; I < idcs.Length; I += 3) {
                for (var J = 0; J < 3; J++) {
                    var K = (J + 1) % 3;
                    var i = idcs[I + J];
                    var j = idcs[I + K];
                    if (i > j) { var k = j; j = i; i = k; }
                    if (sortedEdges.Contains((i, j))) { nBoundaryHe--; }
                    else { sortedEdges.Add((i, j)); nBoundaryHe++; }
                }
            }

            nVerts = vrts.Length;
            nFaces = idcs.Length / 3;
            var nEdges = sortedEdges.Count;
            var nHalfedges = 2 * nEdges;
            var nInteriorHalfedges = nHalfedges - nBoundaryHe;
            this.halfedges = new HalfEdge[nHalfedges];
            this.corners = new Corner[nInteriorHalfedges];
            this.verts = new Vert[nVerts];
            this.bunds = new Face[nBoundaryHe];
            this.faces = new Face[nFaces];
            eulerCharactaristics = nFaces - nEdges + nVerts;
        }
    }
}
