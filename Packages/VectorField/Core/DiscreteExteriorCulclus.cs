using Unity.Mathematics;
using System.Numerics;
using System.Collections.Generic;
using MathNet.Numerics;
using UnityEngine;
using f3 = Unity.Mathematics.float3;
using d2 = Unity.Mathematics.double2;
using RS = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
using CS = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
using RV = MathNet.Numerics.LinearAlgebra.Vector<double>;
using static Unity.Mathematics.math;

namespace VectorField {
    public static class DEC {

        public static RS BuildHodgeStar0Form(HeGeom g) {
            var n = g.nVerts;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var v = g.Verts[i];
                r[v.vid] = g.BarycentricDualArea(v);
            }
            return RS.OfDiagonalArray(r);
        }

        public static RS BuildHodgeStar1Form(HeGeom g) {
            var n = g.nEdges;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var e = g.Edges[i];
                var h = g.halfedges[e.hid];
                r[e.eid] = (g.Cotan(h) + g.Cotan(h.twin)) * 0.5;
            }
            return RS.OfDiagonalArray(r);
        }

        public static RS BuildHodgeStar2Form(HeGeom g) {
            var n = g.nFaces;
            var r = new double[n];
            for (var i = 0; i < n; i++) {
                var f = g.Faces[i];
                var h = g.halfedges[f.hid];
                var la = g.Length(h);
                var lb = g.Length(h.prev);
                var lc = g.Length(h.next);
                var s = (la + lb + lc) * 0.5;
                r[f.fid] = 1 / sqrt(s * (s - la) * (s - lb) * (s - lc));
            }
            return RS.OfDiagonalArray(r);
        }

        public static RS BuildExteriorDerivative0Form(HeGeom g) {
            var l = new List<(int, int, double)>();
            foreach (var e in g.Edges) {
                l.Add((e.eid, g.halfedges[e.hid].vid, -1));
                l.Add((e.eid, g.halfedges[e.hid].next.vid,  1));
            }
            return RS.OfIndexed(g.nEdges, g.nVerts, l);
        }

        public static RS BuildExteriorDerivative1Form(HeGeom g) {
            var l = new List<(int, int, double)>();
            foreach(var f in g.Faces) {
                foreach(var h in g.GetAdjacentHalfedges(f)){
                    var dir = h.id == h.edge.hid ? 1 : -1;
                    l.Add((f.fid, h.edge.eid, dir));
                }
            }
            return RS.OfIndexed(g.nFaces, g.nEdges, l);
        }
        
        /*
         * Generates Real Mass Matrix
        */
        public static RS Mass(HeGeom g){
            var n = g.nVerts;
            System.Span<double> a = stackalloc double[n];
            for (var i = 0; i < n; i++)
                a[i] = g.BarycentricDualArea(i);
            return RS.OfDiagonalArray(a.ToArray());
        }

        /*
         * Generates Complex Mass Matrix
        */
        public static CS MassComplex(HeGeom g){
            var n = g.nVerts;
            System.Span<Complex> a = stackalloc Complex[n];
            for (var i = 0; i < n; i++)
                a[i] = g.BarycentricDualArea(i); // TODO: Check cast works
            return CS.OfDiagonalArray(a.ToArray());
        }

        /*
         * Generates Real Laplace Matrix
        */
        public static RS Laplace(HeGeom g){
            var n = g.nVerts;
            var T = new List<(int, int, double)>();
            for (var i = 0; i < n; i++) {
                var v = g.Verts[i];
                var s = 0f;
                foreach (var h in g.GetAdjacentHalfedges(v)) {
                    var w = g.EdgeCotan(h.edge); 
                    T.Add((i, h.next.vid, -w));
                    s += w;
                }
                T.Add((i, i, s));
            }
            var M = RS.OfIndexed(n, n, T);
            var C = RS.CreateDiagonal(n, n, 1e-8);
            return M + C;
        }
        
        /*
         * Generates Complex Laplace Matrix
        */
        public static CS LaplaceComplex(HeGeom g){
            var n = g.nVerts;
            var T = new List<(int, int, Complex)>();
            for (var i = 0; i < n; i++) {
                var v = g.Verts[i];
                var s = 0d;
                foreach (var h in g.GetAdjacentHalfedges(v)) {
                    var j = h.twin.vid;
                    //var w = g.EdgeCotan(h.edge); 
                    var w = (g.Cotan(h) + g.Cotan(h.twin)) * 0.5; 
                    T.Add((i, j, -w));
                    //Debug.Log("i:" + i + ", j:" + j + ", w:" + w);
                    s += w;
                }
                T.Add((i, i, s));
            }
            var M = CS.OfIndexed(n, n, T);
            var C = CS.CreateDiagonal(n, n, 1e-8);
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
        public static CS ConnectionLaplace(HeGeom g) {
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
            var M = CS.OfIndexed(n, n, t);
            var C = CS.CreateDiagonal(n, n, new Complex(1e-8f, 0));
            return M + C;
        }
        
        /*
         * Interpolation to compute intrinsic vector on faces.
         * See Keenan's lecture 8 for Whitney interpolation theory.
        */
        public static f3[] InterpolateWhitney(RV oneForm, HeGeom g) {
            var X = new float3[g.nFaces];
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
                X[i] = cross(N, a + b + c) / (float)(6 * A);
            }
            return X;
        }
        
    }
}
