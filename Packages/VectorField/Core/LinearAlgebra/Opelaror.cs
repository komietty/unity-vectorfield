using System.Collections.Generic;
using MathNet.Numerics;
using Unity.Mathematics;

namespace VectorField {
    using static math;
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

        static float2 Divider(float2 u, float2 v) {
            var denom = v.x * v.x + v.y * v.y;
            return new float2(u.x * v.x + u.y * v.y, u.y * v.x - u.x * v.y) / denom;
        }

        public static CSprs ConnectionLaplace(HeGeom geom){
            var triplets = new List<(int, int, Complex32)>();
            var n = geom.nVerts;
            var halfedgeVectorInVertex = new float2[geom.halfedges.Length];
            var transportVectorAlongHe = new float2[geom.halfedges.Length];

            for (var i = 0; i < n; i++) {
                var v = geom.Verts[i];
                var angle = 0f;
                // is isBoundary, rate is PI / geom.AngleSum(v);
                var rate = 2 * PI / geom.AngleSum(v);
                foreach (var c in geom.GetAdjacentConers(v)) {
                    var h = geom.halfedges[c.hid].prev;
                    halfedgeVectorInVertex[h.id] = new float2(cos(angle), sin(angle)) * geom.Length(h);
                    angle += geom.Angle(c) * rate;
                }
            }

            foreach (var e in geom.Edges) {
                var heA = geom.halfedges[e.hid];
                var heB = heA.twin;
                var vecA = halfedgeVectorInVertex[heA.id];
                var vecB = halfedgeVectorInVertex[heB.id];
                var rot = normalize(Divider(-vecB, vecA));
                transportVectorAlongHe[heA.id] = rot;
                transportVectorAlongHe[heB.id] = Divider(new float2(1, 0), rot);
            }

            var dupcheck = new List<int>();
            foreach (var h in geom.halfedges) {
                var iTail = h.vid;
                var iTop  = h.twin.vid;
                var weight = geom.EdgeCotan(h.edge);
                var rot = transportVectorAlongHe[h.twin.id];
                var val = -weight * rot;
                if(!dupcheck.Contains(iTail))
                    triplets.Add((iTail, iTail, new Complex32(weight, 0)));
                triplets.Add((iTail, iTop, new Complex32(val.x, val.y)));
                dupcheck.Add(iTail);
            }

            /*
            for (var i = 0; i < n; i++) {
                var v = geom.Verts[i];
                var s = 0f;
                foreach (var h in geom.GetAdjacentHalfedges(v)) {
                    var a = geom.Cotan(h);
                    var b = geom.Cotan(h.twin);
                    var c = (a + b) * 0.5f;
                    var angleDiff = d[h.twin.id] + PI - d[h.id];
                    var r = new Complex32(cos(angleDiff), sin(angleDiff));
                    UnityEngine.Debug.Log(r);
                    t.Add((i, h.next.vid, -c * r));
                    s += c;
                }
                t.Add((i, i, s));
            }
            */

            var M = CSprs.OfIndexed(n, n, triplets);
            var C = CSprs.CreateDiagonal(n, n, new Complex32(1e-8f, 0));
            return M;// + C;
        }

        public static Complex32 TransportNoRotationComplex(HeGeom g, Vert v, HalfEdge h) {
            var (e1, e2) = g.OrthonormalBasis(g.Verts[h.vid]);
            var (f1, f2) = g.OrthonormalBasis(g.Verts[h.twin.vid]);
            var u1 = g.Vector(g.halfedges[h.edge.hid]);
            var u2 = g.Vector(g.halfedges[h.edge.hid].twin);
            var thetaIJ = atan2(dot(u1, e2), dot(u1, e1));
            var thetaJI = atan2(dot(u2, f2), dot(u2, f1));
            var o = (thetaJI + PI - thetaIJ) % PI;
            if(o >  PI / 2) o = -(PI - o); 
            if(o < -PI / 2) o =  (PI + o); 
            return new Complex32(cos(o), sin(o));
        }
    }
}
