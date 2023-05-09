using System.Collections.Generic;
using Unity.Mathematics;
using System.Numerics;

namespace VectorField {
    using static math;
    using d2 = double2;
    using RSparse = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using CSparse = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;

    public static class Operator {
        /*
         * Generates Real Mass Matrix
        */
        public static RSparse Mass(HeGeom g){
            var n = g.nVerts;
            System.Span<double> a = stackalloc double[n];
            for (var i = 0; i < n; i++) a[i] = g.BarycentricDualArea(i);
            return RSparse.OfDiagonalArray(a.ToArray());
        }

        /*
         * Generates Complex Mass Matrix
        */
        public static CSparse MassComplex(HeGeom g){
            var n = g.nVerts;
            System.Span<Complex> a = stackalloc Complex[n];
            for (var i = 0; i < n; i++) a[i] = new Complex(g.BarycentricDualArea(i), 0);
            return CSparse.OfDiagonalArray(a.ToArray());
        }

        /*
         * Generates Real Laplace Matrix
        */
        public static RSparse Laplace(HeGeom g){
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
            var M = RSparse.OfIndexed(n, n, t);
            var C = RSparse.CreateDiagonal(n, n, 1e-8);
            return M + C;
        }

        /*
         * A connection laplace matrix.
         * If a vert is on boundary, proj2plane is PI/g.AngleSum(v).
         * Because the halfEdges on HeGeom are oriented CW order,
         * regular g.Angle(c) func does not calc angle in CCW way.
         * Here I instead calc angles manually using AdjacentHes.
         * M should be Hermitian Matrix.
        */
        public static CSparse ConnectionLaplace(HeGeom g) {
            var t = new List<(int, int, Complex)>();
            var halfedgeVectorInVertex = new d2[g.halfedges.Length];
            var transportVectorAlongHe = new d2[g.halfedges.Length];

            foreach (var v in g.Verts) {
                var angle = 0f;
                var proj2plane = 2 * PI / g.AngleSum(v);
                foreach (var h in g.GetAdjacentHalfedges(v)) {
                    halfedgeVectorInVertex[h.id] = new d2(cos(angle), sin(angle)) * g.Length(h);
                    var v1 = normalize(g.Vector(h));
                    var v2 = normalize(g.Vector(h.twin.next));
                    angle += acos(dot(v1, v2)) * proj2plane;
                }
            }

            foreach (var e in g.Edges) {
                var hA = g.halfedges[e.hid];
                var hB = hA.twin;
                var vA = halfedgeVectorInVertex[hA.id];
                var vB = halfedgeVectorInVertex[hB.id];
                var rot = normalize(Utility.Div(-vB, vA));
                transportVectorAlongHe[hA.id] = rot;
                transportVectorAlongHe[hB.id] = Utility.Div(new d2(1, 0), rot);
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

            var n = g.nVerts;
            var M = CSparse.OfIndexed(n, n, t);
            var C = CSparse.CreateDiagonal(n, n, new Complex(1e-8f, 0));
            return M + C;
        }
    }
}
