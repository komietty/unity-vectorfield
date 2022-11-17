using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using System.Runtime.InteropServices;
using System.Linq;
using System;

namespace ddg {
    public static class Operator {

        public static SparseMatrix Mass(HalfEdgeGeom geom){
            var n = geom.nVerts;
            Span<double> a = stackalloc double[n];
            for (int i = 0; i < n; i++) a[i] = geom.BarycentricDualArea(geom.Verts[i]);
            return SparseMatrix.OfDiagonalArray(a.ToArray());
        }

        public static SparseMatrix MassInv(HalfEdgeGeom geom){
            var n = geom.nVerts;
            Span<double> a = stackalloc double[n];
            for (int i = 0; i < n; i++) a[i] = 1 / geom.BarycentricDualArea(geom.Verts[i]);
            return SparseMatrix.OfDiagonalArray(a.ToArray());
        }

        public static SparseMatrix Laplace(HalfEdgeGeom geom){
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
            var M = SparseMatrix.OfIndexed(n, n, t);
            var C = SparseMatrix.CreateDiagonal(n, n, 1e-8d);
            return M + C;
        }
    }
}
