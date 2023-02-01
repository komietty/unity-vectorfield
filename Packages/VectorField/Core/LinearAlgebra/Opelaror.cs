using System.Collections.Generic;
using MathNet.Numerics;

namespace VectorField {
    using static Unity.Mathematics.math;
    using RS = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using CS = MathNet.Numerics.LinearAlgebra.Complex32.SparseMatrix;

    public static class Operator {
        /*
         * Generates Real Mass Matrix
        */
        public static RS Mass(HeGeom geom){
            var n = geom.nVerts;
            System.Span<double> a = stackalloc double[n];
            for (int i = 0; i < n; i++) a[i] = geom.BarycentricDualArea(geom.Verts[i]);
            return RS.OfDiagonalArray(a.ToArray());
        }

        /*
         * Generates Complex Mass Matrix
        */
        public static CS MassComplex(HeGeom geom){
            var n = geom.nVerts;
            System.Span<Complex32> a = new Complex32[n];
            for (int i = 0; i < n; i++)
                a[i] = new Complex32((float)geom.BarycentricDualArea(geom.Verts[i]), 0);
            return CS.OfDiagonalArray(a.ToArray());
        }

        /*
         * Generates Real Laplace Matrix
        */
        public static RS Laplace(HeGeom geom){
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
            var M = RS.OfIndexed(n, n, t);
            var C = RS.CreateDiagonal(n, n, 1e-8d);
            return M + C;
        }

        /*
         * Generates Connection Laplace Matrix
         * https://numerics.mathdotnet.com/api/MathNet.Numerics/Complex32.html
        */

        public static CS ConnectionLaplace(HeGeom geom){
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
                    t.Add((i, h.next.vid, r * new Complex32(-c, 0)));
                    s += c;
                }
                t.Add((i, i, s));
            }
            var M = CS.OfIndexed(n, n, t);
            var C = CS.CreateDiagonal(n, n, new Complex32(1e-8f, 0));
            return M + C;
        }

        static Complex32 TransportNoRotationComplex(HeGeom g, HalfEdge h) {
            var u = g.Vector(h);
            var (e1, e2) = g.OrthonormalBasis(g.Verts[h.vid]);
            var (f1, f2) = g.OrthonormalBasis(g.Verts[h.twin.vid]);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            return new Complex32(1, -thetaIJ + thetaJI);
        }
    }
}
