using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace VectorField {
    using S = SparseMatrix;

    public static class Operator {
        /*
         * Generates Mass Matrix
        */
        public static S Mass(HeGeom geom){
            var n = geom.nVerts;
            System.Span<double> a = stackalloc double[n];
            for (int i = 0; i < n; i++) a[i] = geom.BarycentricDualArea(geom.Verts[i]);
            return S.OfDiagonalArray(a.ToArray());
        }

        /*
         * Generates Laplace Matrix
        */
        public static S Laplace(HeGeom geom){
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
            var M = S.OfIndexed(n, n, t);
            var C = S.CreateDiagonal(n, n, 1e-8d);
            return M + C;
        }
    }
}
