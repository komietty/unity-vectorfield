using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace VectorField {
    using S = SparseMatrix;
    using V1 = Vector<double>;
    using V3 = Vector<double3>;

    public class ScalarHeatMethod {
        protected HeGeom geom; 
        protected S A; // The laplace matrix of the input mesh
        protected S F; // The mean curvature flow oparator built on the input mesh

        public ScalarHeatMethod(HeGeom geom) {
            this.geom = geom;
            var t = math.pow(geom.MeanEdgeLength(), 2);
            this.A = Operator.Laplace(geom);
            this.F = Operator.Mass(geom) + A * t;
        }

        public V3 ComputeVectorField(V1 u) {
            var X = V3.Build.Dense(geom.nFaces);
            foreach (var f in geom.Faces) {
                var n = geom.FaceNormal(f).n;
                var a = geom.Area(f);
                var g = new double3();
                foreach (var h in geom.GetAdjacentHalfedges(f)) {
                    var ui = u[h.prev.vid];
                    var ei = geom.Vector(h);
                    g += (double3)math.cross(n, ei) * ui;
                }
                X[f.fid] = -math.normalize(g / (2 * a));
            }
            return X;
        }

        public V1 ComputeDivergence(V3 X) {
            var D = V1.Build.Dense(geom.nVerts);
            foreach(var v in geom.Verts) {
                var sum = 0.0;
                foreach( var h in geom.GetAdjacentHalfedges(v)) {
                    if (!h.onBoundary) {
                        var xj = X[h.face.fid];
                        var e1 = geom.Vector(h);
                        var e2 = geom.Vector(h.prev.twin);
                        var cotTheta1 = geom.Cotan(h);
                        var cotTheta2 = geom.Cotan(h.prev);
                        sum += (cotTheta1 * math.dot(e1, xj) + cotTheta2 * math.dot(e2, xj));
                    }
                }
                D[v.vid] = sum * 0.5;
            }
            return D;
        }

        void SubtractMinDistance(V1 phi) {
            var min = double.NegativeInfinity;
            for (var i = 0; i < phi.Count; i++) min = math.min(phi[i], min);
            for (var i = 0; i < phi.Count; i++) phi[i] -= min;
        }

        public V1 Compute(V1 delta) {
            var u = Solver.Cholesky(F, delta);
            var X = ComputeVectorField(u);
            var D = ComputeDivergence(X);
            var phi = Solver.Cholesky(A, -D);
            SubtractMinDistance(phi);
            return phi;
        } 
    }
}
