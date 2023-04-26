using System.Collections.Generic;
using Unity.Mathematics;

namespace VectorField {
    using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

    public static class TangentField {
        /*
         * Computes the solution of the poisson problem Ax = -M(rho - rhoBar).
         * A: the positive definite laplace matrix and M is the mass matrix.
         * rho: a scalar density of vertices of the input mesh.
        */
        public static Vector ScalarPoissonProblem(HeGeom g, Vector rho){
            var M = Operator.Mass(g);
            var A = Operator.Laplace(g);
            var T = g.TotalArea();
            var rhoSum = (M * rho).Sum();
            var rhoBar = Vector.Build.Dense(M.RowCount, rhoSum / T);
            return Solver.Cholesky(A, -M * (rho - rhoBar));
        }

        public static (Vector oneForm, int[] exactIds, int[] coexactIds) GenRandomOneForm(HeGeom g) {
            var nv = g.nVerts;
            var r = math.max(2, (int)(nv / 1000f));
            var rho1 = Vector.Build.Dense(nv, 0);
            var rho2 = Vector.Build.Dense(nv, 0);
            var exactIds   = new int[r];
            var coexactIds = new int[r];
            for (var i = 0; i < r; i++) {
                var j1 = (int)(UnityEngine.Random.value * nv);
                var j2 = (int)(UnityEngine.Random.value * nv);
                exactIds[i] = j1;
                coexactIds[i] = j2;
                rho1[j1] = UnityEngine.Random.Range(-2500f, 2500f);
                rho2[j2] = UnityEngine.Random.Range(-2500f, 2500f);
            }
            var scalarPotential = ScalarPoissonProblem(g, rho1);
            var vectorPotential = ScalarPoissonProblem(g, rho2);

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
            var omega = Vector.Build.Dense(g.nEdges);
            for (var i = 0; i < g.nEdges; i++) {
                var e = g.Edges[i];
                var h = g.halfedges[e.hid];
                var f1 = h.onBoundary ? new float3() : field[h.face];
                var f2 = h.twin.onBoundary ? new float3() : field[h.twin.face];
                omega[i] = math.dot(f1 + f2, g.Vector(h));
            }
            return (omega, exactIds, coexactIds);
        }
    }
}