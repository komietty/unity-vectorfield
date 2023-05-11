using System.Collections.Generic;
using Unity.Mathematics;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

namespace VectorField {
    public static class ScalarPoissonProblem {
        /*
         * Computes the solution of the poisson problem Ax = -M(rho - rhoBar).
         * A: the positive definite laplace matrix
         * M: the mass matrix
         * rho: a scalar density of verts of the input mesh
        */
        public static Vector Compute(HeGeom g, Vector rho){
            var M = DEC.Mass(g);
            var A = DEC.Laplace(g);
            var T = g.TotalArea();
            var rhoSum = (M * rho).Sum();
            var rhoBar = Vector.Build.Dense(M.RowCount, rhoSum / T);
            return Solver.Cholesky(A, -M * (rho - rhoBar));
        }

        /*
         * Computes a random one-form by solving scalar poisson problem.
         */
        public static Vector ComputeRandomOneForm(HeGeom g) {
            var nv = g.nVerts;
            var r = math.max(2, (int)(nv / 1000f));
            var rho1 = Vector.Build.Dense(nv, 0); // exact component
            var rho2 = Vector.Build.Dense(nv, 0); // coexact component
            for (var i = 0; i < r; i++) {
                var j1 = (int)(UnityEngine.Random.value * nv);
                var j2 = (int)(UnityEngine.Random.value * nv);
                rho1[j1] = UnityEngine.Random.Range(-2500f, 2500f);
                rho2[j2] = UnityEngine.Random.Range(-2500f, 2500f);
            }
            var scalarPotential = Compute(g, rho1);
            var vectorPotential = Compute(g, rho2);
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
            return omega;
        }
    }
}