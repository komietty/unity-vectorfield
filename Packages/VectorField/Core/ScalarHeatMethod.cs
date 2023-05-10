using Unity.Mathematics;
using System.Collections.Generic;
using Dense  = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using static Unity.Mathematics.math;

namespace VectorField {
    public static class ScalarHeatMethod {

        static Dense ComputeVectorField(HeGeom g, IList<double> u) {
            var X = Dense.Create(g.nFaces, 3, 0);
            foreach (var f in g.Faces) {
                var n = g.FaceNormal(f).n;
                var a = g.Area(f);
                var d = new double3();
                foreach (var h in g.GetAdjacentHalfedges(f)) {
                    var ui = u[h.prev.vid];
                    var ei = g.Vector(h);
                    d += (double3)cross(n, ei) * ui;
                }
                var xi = -normalize(d / (2 * a));
                X.SetRow(f.fid, Vector.Build.DenseOfArray(new [] { xi.x, xi.y, xi.z }));
            }
            return X;
        }

        static Vector ComputeDivergence(HeGeom g, Dense X) {
            var D = Vector.Build.Dense(g.nVerts);
            foreach(var v in g.Verts) {
                var sum = 0.0;
                foreach (var h in g.GetAdjacentHalfedges(v)) {
                    if (h.onBoundary) continue;
                    var xm = X.Row(h.face.fid);
                    var xj = new double3(xm[0], xm[1], xm[2]);
                    var e1 = g.Vector(h);
                    var e2 = g.Vector(h.prev.twin);
                    var c1 = g.Cotan(h);
                    var c2 = g.Cotan(h.prev);
                    sum += c1 * dot(e1, xj) + c2 * dot(e2, xj);
                }
                D[v.vid] = sum * 0.5;
            }
            return D;
        }

        /*
         * Compute heat flow with the mean curvature flow operator F. 
         */
        public static Vector ComputeScalarHeatFlow(HeGeom g, Vector delta) {
            var t = pow(g.MeanEdgeLength(), 2);
            var L = DEC.Laplace(g);
            var F = DEC.Mass(g) + L * t;
            var u = Solver.Cholesky(F, delta);
            var X = ComputeVectorField(g, u);
            var D = ComputeDivergence(g, X);
            var phi = Solver.Cholesky(L, -D);
            //subtract min distance
            var min = double.PositiveInfinity;
            for (var i = 0; i < phi.Count; i++) min = math.min(phi[i], min);
            for (var i = 0; i < phi.Count; i++) phi[i] -= min;
            return phi;
        } 
    }
}
