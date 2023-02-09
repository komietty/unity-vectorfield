using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Unity.Mathematics;
using UnityEngine;
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

        public float3[] GenField(CV phi) {
            var field = new float3[geom.nVerts];
            var laplace = Operator.ConnectionLaplace(geom);
            var t = pow(geom.MeanEdgeLength(), 2);
            var F = Operator.MassComplex(geom) + laplace * t;
            Debug.Log(laplace);
            Debug.Log(F);
            if (!F.IsHermitian()) {
                Debug.LogWarning("the Connection Laplace Op is not hermitian");
            }

            var llt = F.LU();
            var conn = F.Solve(phi);
            foreach (var v in geom.Verts) {
                var (e1, e2) = geom.OrthonormalBasis(v);
                field[v.vid] = e1 * cos(conn[v.vid].Real)
                             + e2 * sin(conn[v.vid].Imaginary);
            }
            return field;
        }
    }
}
