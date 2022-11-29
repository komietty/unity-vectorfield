using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using static Unity.Mathematics.math;

namespace ddg {
public static class ExteriorDerivatives {

        public static SparseMatrix BuildHodgeStar0Form(HalfEdgeGeom geom) {
            var n = geom.nVerts;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var v = geom.Verts[i];
                r[v.vid] = geom.BarycentricDualArea(v);
            }
            return SparseMatrix.OfDiagonalArray(r);
        }

        public static SparseMatrix BuildHodgeStar1Form(HalfEdgeGeom geom) {
            var n = geom.nEdges;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var e = geom.Edges[i];
                var h = geom.halfedges[e.hid];
                r[e.eid] = (geom.Cotan(h) + geom.Cotan(h.twin)) * 0.5;
            }
            return SparseMatrix.OfDiagonalArray(r);
        }

        public static SparseMatrix BuildHodgeStar2Form(HalfEdgeGeom geom) {
            var n = geom.nFaces;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var f = geom.Faces[i];
                var h = geom.halfedges[f.hid];
                var la = geom.Length(h);
                var lb = geom.Length(h.prev);
                var lc = geom.Length(h.next);
                var s = (la + lb + lc) * 0.5;
                r[f.fid] = 1.0 / sqrt(s * (s - la) * (s - lb) * (s - lc));
            }
            return SparseMatrix.OfDiagonalArray(r);
        }

        public static SparseMatrix BuildExteriorDerivative0Form(HalfEdgeGeom geom) {
            var l = new List<(int, int, double)>();
            foreach(var e in geom.Edges) {
                l.Add((e.eid, geom.halfedges[e.hid].vid, -1.0));
                l.Add((e.eid, geom.halfedges[e.hid].next.vid,  1.0));
            }
            return SparseMatrix.OfIndexed(geom.nEdges, geom.nVerts, l);
        }

        public static SparseMatrix BuildExteriorDerivative1Form(HalfEdgeGeom geom) {
            var l = new List<(int, int, double)>();
            foreach(var f in geom.Faces) {
                foreach(var h in geom.GetAdjacentHalfedges(f)){
                    var dir = h.id == h.edge.hid ? 1.0 : -1.0;
                    l.Add((f.fid, h.edge.eid, dir));
                }
            }
            return SparseMatrix.OfIndexed(geom.nFaces, geom.nEdges, l);
        }
    }
}
