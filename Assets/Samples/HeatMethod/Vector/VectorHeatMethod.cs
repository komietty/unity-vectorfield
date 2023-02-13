using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace VectorField {
    using RV = MathNet.Numerics.LinearAlgebra.Vector<double>;
    using RS = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using DD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
    using CS = MathNet.Numerics.LinearAlgebra.Complex32.SparseMatrix;
    using CV =Vector<Complex32>;

    public class VectorHeatMethod {
        protected HeGeom geom;
        protected ScalarHeatMethod shm;

        public VectorHeatMethod(HeGeom geom) {
            this.geom = geom;
            this.shm = new ScalarHeatMethod(geom);
        }
        
        CV ComputeVectorHeatFlow(CV phi) {
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

        public float3[] GenField(CV phi, List<(int vid, float value)> sources) {
            var field = new float3[geom.nVerts];
            var conn = ComputeVectorHeatFlow(phi);
            var mags = ExtendScaler(sources);
            
            foreach (var v in geom.Verts) {
                var (e1, e2) = geom.OrthonormalBasis(v);
                var cmp = conn[v.vid] / (float)conn[v.vid].Norm();
                var mag = mags[v.vid];
                var sol = cmp * (float)mag;
                field[v.vid] = e1 * sol.Real + e2 * sol.Imaginary;
            }
            return field;
        }

        double[] ExtendScaler(List<(int vid, float value)> source) {
            var dataRhs = new double[geom.nVerts];
            var idctRhs = new double[geom.nVerts];
            foreach (var s in source) {
                dataRhs[s.vid] = s.value;
                idctRhs[s.vid] = 1;
            }
            var dataSol = shm.Compute(RV.Build.DenseOfArray(dataRhs));
            var idctSol = shm.Compute(RV.Build.DenseOfArray(idctRhs));
            var result = new double[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) {
                var data = dataSol[i];
                var indicator = idctSol[i];
                if (indicator != 0) result[i] = data / indicator;
                else result[i] = 0;
            }
            return result;
        }
    }
}
