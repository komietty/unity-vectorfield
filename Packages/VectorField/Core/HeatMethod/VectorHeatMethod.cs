using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace VectorField {
    using RS = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using DD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
    using CS = MathNet.Numerics.LinearAlgebra.Complex32.SparseMatrix;
    
    using RV = Vector<float>;
    using CV =Vector<Complex32>;

    public class VectorHeatMethod {
        protected HeGeom geom;
        protected ScalarHeatMethod shm;

        public VectorHeatMethod(HeGeom geom) {
            this.geom = geom;
            this.shm = new ScalarHeatMethod(geom);
        }
        
        public Vector<float> ExtendScaler(List<(int vid, float value)> source) {
            var dataRhs = new double[geom.nVerts];
            var idctRhs = new double[geom.nVerts];
            foreach (var s in source) {
                dataRhs[s.vid] = s.value;
                idctRhs[s.vid] = 1;
            }
            var dataSol = shm.Compute(Vector<double>.Build.DenseOfArray(dataRhs));
            var idctSol = shm.Compute(Vector<double>.Build.DenseOfArray(idctRhs));
            var result = new float[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) {
                var data = dataSol[i];
                var indicator = idctSol[i];
                if (abs(indicator) < 1e-3) result[i] = 0;
                else result[i] = (float)(data / indicator);
            }
            return Vector<float>.Build.DenseOfArray(result);
        }
        
        public Vector<Complex32> ComputeVectorHeatFlow(CV phi) {
            var t = pow(geom.MeanEdgeLength(), 2);
            var L = Operator.ConnectionLaplace(geom);
            var F = Operator.MassComplex(geom) + L * t;
            if (!F.IsHermitian()) Debug.LogWarning("Op should be hermitian");
            var conn = F.LU().Solve(phi);
            var vecs = new Complex32[geom.nVerts];
            for (var i =0; i < geom.nVerts; i++) 
                vecs[i] = conn[i] / (float)conn[i].Norm();
            return Vector<Complex32>.Build.DenseOfArray(vecs);
        }

        public float3[] GenField(Vector<Complex32> connection, Vector<float> magnitude) {
            var field = new float3[geom.nVerts];
            foreach (var v in geom.Verts) {
                var (e1, e2) = geom.OrthonormalBasis(v);
                var cmp = connection[v.vid];
                var mag = magnitude[v.vid];
                var sol = cmp * mag;
                field[v.vid] = e1 * sol.Real + e2 * sol.Imaginary;
            }
            return field;
        }

    }
}
