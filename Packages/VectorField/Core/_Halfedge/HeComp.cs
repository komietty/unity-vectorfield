using System.Collections.Generic;
using System;
using System.Linq;

namespace VectorField {
    using M = UnityEngine.Mesh;

    /*
     * HalfEdge Complex
    */
    public class HeComp {
        public HalfEdge[] halfedges     { get; private set; }
        public bool isManifold          { get; private set; }
        public int eulerCharactaristics { get; private set; }
        public ReadOnlySpan<Vert> Verts => verts.AsSpan();
        public ReadOnlySpan<Edge> Edges => edges.AsSpan();
        public ReadOnlySpan<Face> Faces => faces.AsSpan();
        public int nVerts { get; private set; }
        public int nEdges { get; private set; }
        public int nFaces { get; private set; }
        Vert[] verts;
        Edge[] edges;
        Face[] faces;
        Face[] bunds;
        Corner[] corners;

        public HeComp(M mesh) {
            var idxs = new ReadOnlySpan<int>(mesh.triangles);
            Preallocate(idxs, mesh.vertexCount);

            var alones = new List<HalfEdge>(3);
            var tripls = new List<HalfEdge>(3);
            var ecount = 0;

            /*
             * Allocate halfedges inside a triange.
             * Unity is left hand axis, so mesh ids are stored CW order.
             * That means halfedges are stored CW order as well.
             */
            for (var i = 0; i < idxs.Length; i += 3) {
                tripls.Clear();
                var n  = i + 3;
                var ii = i / 3;
                var f = new Face(i + 2, ii);
                faces[ii] = f;

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
                            hb.edge = ha.edge;
                            alones.Remove(ha);
                            fg = true;
                            break;
                        }
                    }
                    if (!fg) {
                        alones.Add(hb);
                        var e = new Edge(hb.id, ecount);
                        edges[ecount] = e;
                        hb.edge = e;
                        ecount++;
                    }
                }

                for (int j = 0; j < 3; j++) halfedges[i + j] = tripls[j];
                for (int j = i; j < n; j++) verts[idxs[j]] = new Vert(j, idxs[j]);
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
                        while (hasTwins.Contains(next.id)) next = next.twin.next;
                        f = new Face(i, -1);
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

        void Preallocate(ReadOnlySpan<int> idcs, int nv){
            var sortedEdges = new HashSet<(int, int)>();
            var nBoundaryHe = 0;
            for (var I = 0; I < idcs.Length; I += 3) {
                for (var J = 0; J < 3; J++) {
                    var K = (J + 1) % 3;
                    var i = idcs[I + J];
                    var j = idcs[I + K];
                    if (i > j) (i, j) = (j, i); 
                    if (sortedEdges.Contains((i, j))) { nBoundaryHe--; }
                    else { sortedEdges.Add((i, j)); nBoundaryHe++; }
                }
            }
            nVerts = nv;
            nFaces = idcs.Length / 3;
            nEdges = sortedEdges.Count;
            var nHalfedges = 2 * nEdges;
            var nInteriorHalfedges = nHalfedges - nBoundaryHe;
            this.halfedges = new HalfEdge[nHalfedges];
            this.corners = new Corner[nInteriorHalfedges];
            this.verts = new Vert[nVerts];
            this.edges = new Edge[nEdges];
            this.faces = new Face[nFaces];
            this.bunds = new Face[nBoundaryHe];
            eulerCharactaristics = nFaces - nEdges + nVerts;
        }

        /*
         * Weld vertices which are apparently same but structualy disconnected.
        */
        public static M Weld(M original, bool normalize = true) {
            var ogl_vrts = original.vertices;
            var ogl_idcs = original.triangles;
            var alt_mesh = new M();
            var alt_vrts = ogl_vrts.Distinct().ToArray();
            var alt_idcs = new int[ogl_idcs.Length];
            var vrt_rplc = new int[ogl_vrts.Length];
            for (var i = 0; i < ogl_vrts.Length; i++) {
                var o = -1;
                for (var j = 0; j < alt_vrts.Length; j++) 
                    if (alt_vrts[j] == ogl_vrts[i]) { o = j; break; }
                vrt_rplc[i] = o;
            }
            for (var i = 0; i < alt_idcs.Length; i++) alt_idcs[i] = vrt_rplc[ogl_idcs[i]];
            alt_mesh.SetVertices(alt_vrts);
            alt_mesh.SetTriangles(alt_idcs, 0);
            alt_mesh.RecalculateNormals();
            alt_mesh.RecalculateBounds();
            return alt_mesh;
        }
    }
}
