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
            var t = pow(geom.MeanEdgeLength(), 2);
            var L = Operator.ConnectionLaplace(geom);
            var F = Operator.MassComplex(geom) + L * t;
            if (!F.IsHermitian()) Debug.LogWarning("Op should be hermitian");
            
/*
            var FF = Matrix<Complex32>.Build.Dense(4, 4, new Complex32(0, 0));
            // row  
            FF[0, 0] = new Complex32(2.16506f,0);
            FF[0, 1] = new Complex32(0.57735f,0);
            FF[0, 2] = new Complex32(-0.288675f,-0.5f);
            FF[0, 3] = new Complex32(-0.288675f,0.5f);
            // row
            FF[1, 0] = new Complex32(0.57735f,0);
            FF[1, 1] = new Complex32(2.16506f,0);
            FF[1, 2] = new Complex32(-0.288675f,-0.5f);
            FF[1, 3] = new Complex32(-0.288675f,0.5f);
            // row  
            FF[2, 0] = new Complex32(-0.288675f,0.5f);
            FF[2, 1] = new Complex32(-0.288675f,0.5f);
            FF[2, 2] = new Complex32(2.16506f,0);
            FF[2, 3] = new Complex32(-0.288675f,-0.5f);
            // row  
            FF[3, 0] = new Complex32(-0.288675f,-0.5f);
            FF[3, 1] = new Complex32(-0.288675f,-0.5f);
            FF[3, 2] = new Complex32(-0.288675f,0.5f);
            FF[3, 3] = new Complex32(2.16506f,0);
 */

            var llt = F.LU();
            var conn = llt.Solve(phi);
            Debug.Log(L);
            Debug.Log(conn);
            foreach (var v in geom.Verts) {
                var (e1, e2) = geom.OrthonormalBasis(v);
                field[v.vid] = normalize(
                    e1 * conn[v.vid].Real +
                    e2 * conn[v.vid].Imaginary
                    );
            }
            return field;
        }
    }
}
