using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;

namespace VectorField {
    using RS = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using RD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
    using RV = MathNet.Numerics.LinearAlgebra.Vector<double>;

    public class ScalarHeatMethod {
        protected HeGeom geom; 
        protected RS A; // The laplace matrix of the input mesh
        protected RS F; // The mean curvature flow oparator built on the input mesh

        public ScalarHeatMethod(HeGeom geom) {
            this.geom = geom;
            var t = math.pow(geom.MeanEdgeLength(), 2);
            this.A = Operator.Laplace(geom);
            this.F = Operator.Mass(geom) + A * t;
        }

        public RD ComputeVectorField(RV u) {
            var X = RD.Create(geom.nFaces, 3, 0);
            foreach (var f in geom.Faces) {
                var n = geom.FaceNormal(f).n;
                var a = geom.Area(f);
                var g = new double3();
                foreach (var h in geom.GetAdjacentHalfedges(f)) {
                    var ui = u[h.prev.vid];
                    var ei = geom.Vector(h);
                    g += (double3)math.cross(n, ei) * ui;
                }
                var xi = -math.normalize(g / (2 * a));
                X.SetRow(f.fid, RV.Build.DenseOfArray(new double[] { xi.x, xi.y, xi.z }));
            }
            return X;
        }

        public RV ComputeDivergence(DenseMatrix X) {
            var D = RV.Build.Dense(geom.nVerts);
            foreach(var v in geom.Verts) {
                var sum = 0.0;
                foreach( var h in geom.GetAdjacentHalfedges(v)) {
                    if (!h.onBoundary) {
                        var xm = X.Row(h.face.fid);
                        var xj = new double3(xm[0], xm[1], xm[2]);
                        var e1 = geom.Vector(h);
                        var e2 = geom.Vector(h.prev.twin);
                        var cotTheta1 = geom.Cotan(h);
                        var cotTheta2 = geom.Cotan(h.prev);
                        sum += cotTheta1 * math.dot(e1, xj) + cotTheta2 * math.dot(e2, xj);
                    }
                }
                D[v.vid] = sum * 0.5;
            }
            return D;
        }

        void SubtractMinDistance(RV phi) {
            var min = double.PositiveInfinity;
            for (var i = 0; i < phi.Count; i++) min = math.min(phi[i], min);
            for (var i = 0; i < phi.Count; i++) phi[i] -= min;
        }

        public RV Compute(RV delta) {
            var u = Solver.Cholesky(F, delta);
            var X = ComputeVectorField(u);
            var D = ComputeDivergence(X);
            var phi = Solver.Cholesky(A, -D);
            SubtractMinDistance(phi);
            return phi;
        } 
    }
}
