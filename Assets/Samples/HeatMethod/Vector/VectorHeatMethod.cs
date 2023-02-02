using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace VectorField {
    using RS = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using DD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
    using CS = MathNet.Numerics.LinearAlgebra.Complex32.SparseMatrix;
    using CV =Vector<Complex32>;

    public class VectorHeatMethod {
        protected HeGeom geom;
        protected ScalarHeatMethod shm;

        public VectorHeatMethod(HeGeom geom) {
            this.geom = geom;
        }
        
        // must have right direction, but not magnitude
        public DD ComputeVectorHeatFlow() {
            throw new System.Exception();
        }

        public static CV Cholesky(CS lhs, Vector<Complex32> rhs){
            var llt = lhs.LU();
            var rlt = llt.Solve(rhs);
            return rlt;
        }

        public float3[] GenField(CV phi) {
            var field = new float3[geom.nVerts];
            var laplace = Operator.ConnectionLaplace(geom);
            var t = math.pow(geom.MeanEdgeLength(), 2);
            var F = Operator.MassComplex(geom) - laplace * t;
            var conn = Cholesky(F, phi);
            foreach (var v in geom.Verts) {
                var (e1, e2) = geom.OrthonormalBasis(v);
                field[v.vid] = e1 * (float)cos(conn[v.vid].Imaginary)
                             + e2 * (float)sin(conn[v.vid].Imaginary);
            }
            return field;
        }
    }
}
