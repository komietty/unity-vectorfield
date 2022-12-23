using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using static Unity.Mathematics.math;

namespace ddg {
public static class ExteriorDerivatives {

        public static SparseMatrix BuildHodgeStar0Form(HeGeom g) {
            var n = g.nVerts;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var v = g.Verts[i];
                r[v.vid] = g.BarycentricDualArea(v);
            }
            return SparseMatrix.OfDiagonalArray(r);
        }

        public static SparseMatrix BuildHodgeStar1Form(HeGeom g) {
            var n = g.nEdges;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var e = g.Edges[i];
                var h = g.halfedges[e.hid];
                r[e.eid] = (g.Cotan(h) + g.Cotan(h.twin)) * 0.5;
            }
            return SparseMatrix.OfDiagonalArray(r);
        }

        public static SparseMatrix BuildHodgeStar2Form(HeGeom g) {
            var n = g.nFaces;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var f = g.Faces[i];
                var h = g.halfedges[f.hid];
                var la = g.Length(h);
                var lb = g.Length(h.prev);
                var lc = g.Length(h.next);
                var s = (la + lb + lc) * 0.5;
                r[f.fid] = 1.0 / sqrt(s * (s - la) * (s - lb) * (s - lc));
            }
            return SparseMatrix.OfDiagonalArray(r);
        }

        public static SparseMatrix BuildExteriorDerivative0Form(HeGeom g) {
            var l = new List<(int, int, double)>();
            foreach (var e in g.Edges) {
                l.Add((e.eid, g.halfedges[e.hid].vid, -1.0));
                l.Add((e.eid, g.halfedges[e.hid].next.vid,  1.0));
            }
            return SparseMatrix.OfIndexed(g.nEdges, g.nVerts, l);
        }

        public static SparseMatrix BuildExteriorDerivative1Form(HeGeom g) {
            var l = new List<(int, int, double)>();
            foreach(var f in g.Faces) {
                foreach(var h in g.GetAdjacentHalfedges(f)){
                    var dir = h.id == h.edge.hid ? 1.0 : -1.0;
                    l.Add((f.fid, h.edge.eid, dir));
                }
            }
            return SparseMatrix.OfIndexed(g.nFaces, g.nEdges, l);
        }
    }
}
