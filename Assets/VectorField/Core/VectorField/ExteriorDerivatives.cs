using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;

namespace VFD {
    using S = SparseMatrix;

    public static class ExteriorDerivatives {

        public static S BuildHodgeStar0Form(HeGeom g) {
            var n = g.nVerts;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var v = g.Verts[i];
                r[v.vid] = g.BarycentricDualArea(v);
            }
            return S.OfDiagonalArray(r);
        }

        public static S BuildHodgeStar1Form(HeGeom g) {
            var n = g.nEdges;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var e = g.Edges[i];
                var h = g.halfedges[e.hid];
                r[e.eid] = (g.Cotan(h) + g.Cotan(h.twin)) * 0.5;
            }
            return S.OfDiagonalArray(r);
        }

        public static S BuildHodgeStar2Form(HeGeom g) {
            var n = g.nFaces;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var f = g.Faces[i];
                var h = g.halfedges[f.hid];
                var la = g.Length(h);
                var lb = g.Length(h.prev);
                var lc = g.Length(h.next);
                var s = (la + lb + lc) * 0.5;
                r[f.fid] = 1 / math.sqrt(s * (s - la) * (s - lb) * (s - lc));
            }
            return S.OfDiagonalArray(r);
        }

        public static S BuildExteriorDerivative0Form(HeGeom g) {
            var l = new List<(int, int, double)>();
            foreach (var e in g.Edges) {
                l.Add((e.eid, g.halfedges[e.hid].vid, -1));
                l.Add((e.eid, g.halfedges[e.hid].next.vid,  1));
            }
            return S.OfIndexed(g.nEdges, g.nVerts, l);
        }

        public static S BuildExteriorDerivative1Form(HeGeom g) {
            var l = new List<(int, int, double)>();
            foreach(var f in g.Faces) {
                foreach(var h in g.GetAdjacentHalfedges(f)){
                    var dir = h.id == h.edge.hid ? 1 : -1;
                    l.Add((f.fid, h.edge.eid, dir));
                }
            }
            return S.OfIndexed(g.nFaces, g.nEdges, l);
        }
    }
}
