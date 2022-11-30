using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ddg {
    public static class Operator {

        /*
        */
        public static SparseMatrix Mass(HeGeom g){
            var n = g.nVerts;
            System.Span<double> a = stackalloc double[n];
            for (int i = 0; i < n; i++) {
                a[i] = g.BarycentricDualArea(g.Verts[i]);
            }
            return SparseMatrix.OfDiagonalArray(a.ToArray());
        }

        public static SparseMatrix MassInv(HeGeom g){
            var n = g.nVerts;
            System.Span<double> a = stackalloc double[n];
            for (int i = 0; i < n; i++) a[i] = 1 / g.BarycentricDualArea(g.Verts[i]);
            return SparseMatrix.OfDiagonalArray(a.ToArray());
        }

        public static SparseMatrix Laplace(HeGeom g){
            var t = new List<(int, int, double)>();
            var n = g.nVerts;
            for (var i = 0; i < n; i++) {
                var v = g.Verts[i];
                var s = 0f;
                foreach (var h in g.GetAdjacentHalfedges(v)) {
                    var a = g.Cotan(h);
                    var b = g.Cotan(h.twin);
                    var c = (a + b) * 0.5f;
                    t.Add((i, h.next.vid, -c));
                    s += c;
                }
                t.Add((i, i, s));
            }
            var M = SparseMatrix.OfIndexed(n, n, t);
            var C = SparseMatrix.CreateDiagonal(n, n, 1e-8d);
            return M + C;
        }
    }
}
