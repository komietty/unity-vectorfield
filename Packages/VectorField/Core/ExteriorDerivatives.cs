using System.Collections.Generic;
using Unity.Mathematics;

namespace VectorField {
    using Sprs = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using Vecd = MathNet.Numerics.LinearAlgebra.Vector<double>;

    public static class ExteriorDerivatives {

        public static Sprs BuildHodgeStar0Form(HeGeom g) {
            var n = g.nVerts;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var v = g.Verts[i];
                r[v.vid] = g.BarycentricDualArea(v);
            }
            return Sprs.OfDiagonalArray(r);
        }

        public static Sprs BuildHodgeStar1Form(HeGeom g) {
            var n = g.nEdges;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var e = g.Edges[i];
                var h = g.halfedges[e.hid];
                r[e.eid] = (g.Cotan(h) + g.Cotan(h.twin)) * 0.5;
            }
            return Sprs.OfDiagonalArray(r);
        }

        public static Sprs BuildHodgeStar2Form(HeGeom g) {
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
            return Sprs.OfDiagonalArray(r);
        }

        public static Sprs BuildExteriorDerivative0Form(HeGeom g) {
            var l = new List<(int, int, double)>();
            foreach (var e in g.Edges) {
                l.Add((e.eid, g.halfedges[e.hid].vid, -1));
                l.Add((e.eid, g.halfedges[e.hid].next.vid,  1));
            }
            return Sprs.OfIndexed(g.nEdges, g.nVerts, l);
        }

        public static Sprs BuildExteriorDerivative1Form(HeGeom g) {
            var l = new List<(int, int, double)>();
            foreach(var f in g.Faces) {
                foreach(var h in g.GetAdjacentHalfedges(f)){
                    var dir = h.id == h.edge.hid ? 1 : -1;
                    l.Add((f.fid, h.edge.eid, dir));
                }
            }
            return Sprs.OfIndexed(g.nFaces, g.nEdges, l);
        }
        
        /*
         * interpolation to compute intrinsic vector on face
         * see Keenans Lecture 8 for Whitney interpolation.
        */
        public static float3[] InterpolateWhitney(Vecd oneForm, HeGeom g) {
            var field = new float3[g.nFaces];
            for (var i = 0; i < g.nFaces; i++) {
                var f = g.Faces[i];
                var h = g.halfedges[f.hid];
                var pi = g.Pos[h.vid];
                var pj = g.Pos[h.next.vid];
                var pk = g.Pos[h.prev.vid];
                var eij = pj - pi;
                var ejk = pk - pj;
                var eki = pi - pk;
                var cij = oneForm[h.edge.eid];
                var cjk = oneForm[h.next.edge.eid];
                var cki = oneForm[h.prev.edge.eid];
                if (h.edge.hid != h.id) cij *= -1;
                if (h.next.edge.hid != h.next.id) cjk *= -1;
                if (h.prev.edge.hid != h.prev.id) cki *= -1;
                var A = g.Area(f);
                var N = g.FaceNormal(f).n;
                var a = (eki - ejk) * (float)cij;
                var b = (eij - eki) * (float)cjk;
                var c = (ejk - eij) * (float)cki;
                field[i] = math.cross(N, (a + b + c)) / (float)(6 * A);
            }
            return field;
        }
    }
}
