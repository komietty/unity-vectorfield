using System.Collections.Generic;
using Unity.Mathematics;
using System.Numerics;

namespace VectorField {
    using static math;
    using RSprs = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using CSprs = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;

    public static class Operator {
        /*
         * Generates Real Mass Matrix
        */
        public static RSprs Mass(HeGeom g){
            var n = g.nVerts;
            System.Span<double> a = stackalloc double[n];
            for (int i = 0; i < n; i++) a[i] = g.BarycentricDualArea(i);
            return RSprs.OfDiagonalArray(a.ToArray());
        }

        /*
         * Generates Complex Mass Matrix
        */
        public static CSprs MassComplex(HeGeom g){
            var n = g.nVerts;
            System.Span<Complex> a = new Complex[n];
            for (int i = 0; i < n; i++) a[i] = new Complex(g.BarycentricDualArea(i), 0);
            return CSprs.OfDiagonalArray(a.ToArray());
        }

        /*
         * Generates Real Laplace Matrix
        */
        public static RSprs Laplace(HeGeom g){
            var t = new List<(int, int, double)>();
            var n = g.nVerts;
            for (var i = 0; i < n; i++) {
                var v = g.Verts[i];
                var s = 0f;
                foreach (var h in g.GetAdjacentHalfedges(v)) {
                    var w = g.EdgeCotan(h.edge); 
                    t.Add((i, h.next.vid, -w));
                    s += w;
                }
                t.Add((i, i, s));
            }
            var M = RSprs.OfIndexed(n, n, t);
            var C = RSprs.CreateDiagonal(n, n, 1e-8);
            return M + C;
        }

        /*
         * Generates Connection Laplace Matrix
        */
        public static CSprs ConnectionLaplace(HeGeom g) {
            var t = new List<(int, int, Complex)>();
            var n = g.nVerts;
            var halfedgeVectorInVertex = new double2[g.halfedges.Length];
            var transportVectorAlongHe = new double2[g.halfedges.Length];

            for (var i = 0; i < n; i++) {
                var v = g.Verts[i];
                var a = 0f;
                // is isBoundary, rate is PI / geom.AngleSum(v);
                var rate = 2 * PI / g.AngleSum(v);
                foreach (var c in g.GetAdjacentConers(v)) {
                    var h = g.halfedges[c.hid].prev;
                    halfedgeVectorInVertex[h.id] = new double2(cos(a), sin(a)) * g.Length(h);
                    a += g.Angle(c) * rate;
                }
            }

            foreach (var e in g.Edges) {
                var hA = g.halfedges[e.hid];
                var hB = hA.twin;
                var vA = halfedgeVectorInVertex[hA.id];
                var vB = halfedgeVectorInVertex[hB.id];
                var rot = normalize(Divide(-vB, vA));
                transportVectorAlongHe[hA.id] = rot;
                transportVectorAlongHe[hB.id] = Divide(new float2(1, 0), rot);
            }

            var diag = new double[g.nVerts];
            foreach (var h in g.halfedges) {
                var iTail = h.vid;
                var iTop  = h.twin.vid;
                var w = g.EdgeCotan(h.edge);
                var r = transportVectorAlongHe[h.twin.id];
                var v = -w * r;
                diag[iTail] += w;
                t.Add((iTail, iTop, new Complex(v.x, v.y)));
            }

            for (int i = 0; i < diag.Length; i++)
                t.Add((i, i, new Complex(diag[i], 0)));

            var M = CSprs.OfIndexed(n, n, t);
            //var C = CSprs.CreateDiagonal(n, n, new Complex(1e-8f, 0));
            if (!M.IsHermitian()) UnityEngine.Debug.LogError("not hermitian");
            return M; // + C;
        }
        
        static double2 Divide(double2 u, double2 v) {
            var deno = v.x * v.x + v.y * v.y;
            return new double2(u.x * v.x + u.y * v.y, u.y * v.x - u.x * v.y) / deno;
        }
    }
}
