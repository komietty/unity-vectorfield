using System.Collections.Generic;
using MathNet.Numerics;

namespace VectorField {
    using static Unity.Mathematics.math;
    using RSprs = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using CSprs = MathNet.Numerics.LinearAlgebra.Complex32.SparseMatrix;

    public static class Operator {
        /*
         * Generates Real Mass Matrix
        */
        public static RSprs Mass(HeGeom geom){
            var n = geom.nVerts;
            System.Span<double> a = stackalloc double[n];
            for (int i = 0; i < n; i++) a[i] = geom.BarycentricDualArea(i);
            return RSprs.OfDiagonalArray(a.ToArray());
        }

        /*
         * Generates Complex Mass Matrix
        */
        public static CSprs MassComplex(HeGeom g){
            var n = g.nVerts;
            System.Span<Complex32> a = new Complex32[n];
            for (int i = 0; i < n; i++) a[i] = new Complex32((float)g.BarycentricDualArea(i), 0);
            return CSprs.OfDiagonalArray(a.ToArray());
        }

        /*
         * Generates Real Laplace Matrix
        */
        public static RSprs Laplace(HeGeom geom){
            var t = new List<(int, int, double)>();
            var n = geom.nVerts;
            for (var i = 0; i < n; i++) {
                var v = geom.Verts[i];
                var s = 0f;
                foreach (var h in geom.GetAdjacentHalfedges(v)) {
                    var a = geom.Cotan(h);
                    var b = geom.Cotan(h.twin);
                    var c = (a + b) * 0.5f;
                    t.Add((i, h.next.vid, -c));
                    s += c;
                }
                t.Add((i, i, s));
            }
            var M = RSprs.OfIndexed(n, n, t);
            var C = RSprs.CreateDiagonal(n, n, 1e-8d);
            return M + C;
        }

        /*
         * Generates Connection Laplace Matrix
         * https://numerics.mathdotnet.com/api/MathNet.Numerics/Complex32.html
        */

        public static CSprs ConnectionLaplace(HeGeom geom){
            var t = new List<(int, int, Complex32)>();
            var n = geom.nVerts;
            for (var i = 0; i < n; i++) {
                var v = geom.Verts[i];
                var s = 0f;
                foreach (var h in geom.GetAdjacentHalfedges(v)) {
                    var a = geom.Cotan(h);
                    var b = geom.Cotan(h.twin);
                    var c = (a + b) * 0.5f;
                    var r = TransportNoRotationComplex(geom, h);
                    t.Add((i, h.next.vid, new Complex32(-c, r.Imaginary)));
                    s += c;
                }
                t.Add((i, i, s));
            }
            var M = CSprs.OfIndexed(n, n, t);
            UnityEngine.Debug.Log(M);
            var C = CSprs.CreateDiagonal(n, n, new Complex32(1e-8f, 0));
            return M + C;
        }

        public static Complex32 TransportNoRotationComplex(HeGeom g, HalfEdge h) {
            var u = g.Vector(g.halfedges[h.edge.hid]);
            var (e1, e2) = g.OrthonormalBasis(g.Verts[h.vid]);
            var (f1, f2) = g.OrthonormalBasis(g.Verts[h.twin.vid]);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            var o = -thetaIJ + thetaJI;
            var s = sign(o);
            return new Complex32(1, s * (abs(o) % PI));
        }
    }
}
