using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;

namespace VectorField {
    using V = Vector<double>;
    using R = UnityEngine.Random;

    public static class TangentField {

        public static (V oneForm, int[] exactIds, int[] coexactIds) GenRandomOneForm(HeGeom g) {
            var nv = g.nVerts;
            var r = math.max(2, (int)(nv / 1000f));
            var rho1 = DenseVector.Create(nv, 0);
            var rho2 = DenseVector.Create(nv, 0);
            var exactIds   = new int[r];
            var coexactIds = new int[r];
            for (var i = 0; i < r; i++) {
                var j1 = (int)(R.value * nv);
                var j2 = (int)(R.value * nv);
                exactIds[i] = j1;
                coexactIds[i] = j2;
                rho1[j1] = R.Range(-2500f, 2500f);
                rho2[j2] = R.Range(-2500f, 2500f);
            }
            var scalarPotential = Solver.ScalarPoissonProblem(g, rho1);
            var vectorPotential = Solver.ScalarPoissonProblem(g, rho2);

            var field = new Dictionary<Face, float3>();
            for (var i = 0; i < g.nFaces; i++) {
                var f = g.Faces[i];
                var v = new float3();
                var a = g.Area(f);
                var n = g.FaceNormal(f).n;
                foreach (var h in g.GetAdjacentHalfedges(f)) {
                    var j = h.prev.vid;
                    var e  = g.Vector(h);
                    var eT = math.cross(n, e);
                    v += eT * (float)(scalarPotential[j] / (2 * a));
                    v += e  * (float)(vectorPotential[j] / (2 * a));
                }
                var c = g.Centroid(f);
                var u = new float3(-c.z, 0, c.x);
                u -= n * math.dot(u, n);
                u = math.normalize(u);
                v += u * 0.3f;
                field[f] = v;
            }
            var omega = new double[g.nEdges];
            for (var i = 0; i < g.nEdges; i++) {
                var e = g.Edges[i];
                var h = g.halfedges[e.hid];
                var f1 = h.onBoundary ? new float3() : field[h.face];
                var f2 = h.twin.onBoundary ? new float3() : field[h.twin.face];
                omega[i] = math.dot(f1 + f2, g.Vector(h));
            }
            return (DenseVector.OfArray(omega), exactIds, coexactIds);
        }

        /*
         * interpolation to compute intrinsic vector on face
         * see Keenans Lecture 8 for Whitney interpolation.
        */
        public static float3[] InterpolateWhitney(V oneForm, HeGeom g) {
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