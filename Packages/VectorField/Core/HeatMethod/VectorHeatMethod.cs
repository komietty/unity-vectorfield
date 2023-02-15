using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using Unity.Mathematics;
using UnityEngine;

namespace VectorField {
    using static math;
    using RS = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using DD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
    using CS = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
    using RV = MathNet.Numerics.LinearAlgebra.Vector<double>;
    using CV = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;

    public class VectorHeatMethod {
        protected HeGeom geom;
        protected ScalarHeatMethod shm;

        public VectorHeatMethod(HeGeom geom) {
            this.geom = geom;
            this.shm  = new ScalarHeatMethod(geom);
        }
        
        public RV ExtendScaler(List<(int vid, double val)> src) {
            var dataRhs = new double[geom.nVerts];
            var idctRhs = new double[geom.nVerts];
            foreach (var s in src) {
                dataRhs[s.vid] = s.val;
                idctRhs[s.vid] = 1;
            }
            var dataSol = shm.Compute(RV.Build.DenseOfArray(dataRhs));
            var idctSol = shm.Compute(RV.Build.DenseOfArray(idctRhs));
            var result = new double[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) {
                var data = dataSol[i];
                var idct = idctSol[i];
                if (abs(idct) < 1e-3) result[i] = 0;
                else result[i] = data / idct;
            }
            return RV.Build.DenseOfArray(result);
        }
        
        public CV ComputeVectorHeatFlow(CV phi) {
            var t = pow(geom.MeanEdgeLength(), 2);
            var L = Operator.ConnectionLaplace(geom);
            var F = Operator.MassComplex(geom) + L * t;
            var C = Solver.LUComp(F,phi);
            return CV.Build.DenseOfEnumerable(C.Select(c => c / c.Norm()));
        }

        public float3[] GenField(CV connection, RV magnitude) {
            var field = new float3[geom.nVerts];
            foreach (var v in geom.Verts) {
                var (e1, e2) = geom.OrthonormalBasis(v);
                var cmp = connection[v.vid];
                var mag = magnitude[v.vid];
                var sol = cmp;// * mag;
                field[v.vid] = e1 * (float)sol.Real + e2 * (float)sol.Imaginary;
            }
            return field;
        }
    }
}
