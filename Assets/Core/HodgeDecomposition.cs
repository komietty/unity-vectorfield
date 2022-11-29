using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;

namespace ddg {
    public class HodgeDecomposition {
        SparseMatrix hodge1;
        SparseMatrix hodge2;
        SparseMatrix hodge1Inv;
        SparseMatrix hodge2Inv;
        SparseMatrix d0;
        SparseMatrix d1;
        SparseMatrix d0T;
        SparseMatrix d1T;
        SparseMatrix A;
        SparseMatrix B;

        public HodgeDecomposition(HalfEdgeGeom geom) {
            hodge1 = ExteriorDerivatives.BuildHodgeStar1Form(geom);
            hodge2 = ExteriorDerivatives.BuildHodgeStar2Form(geom);
            d0 = ExteriorDerivatives.BuildExteriorDerivative0Form(geom);
            d1 = ExteriorDerivatives.BuildExteriorDerivative1Form(geom);
            var ne = hodge1.RowCount;
            var nf = hodge2.RowCount;
            var stgHodge1 = hodge1.Storage;
            var stgHodge2 = hodge2.Storage;
            var hodge1InvArr = new double[ne];
            var hodge2InvArr = new double[nf];
            for (var i = 0; i < ne; i++) { hodge1InvArr[i] = 1.0 / stgHodge1[i, 0]; }
            for (var i = 0; i < nf; i++) { hodge2InvArr[i] = 1.0 / stgHodge2[i, 0]; }
            hodge1Inv = SparseMatrix.OfDiagonalArray(hodge1InvArr);
            hodge2Inv = SparseMatrix.OfDiagonalArray(hodge2InvArr);
            d0T = SparseMatrix.OfMatrix(d0.Transpose());
            d1T = SparseMatrix.OfMatrix(d1.Transpose());

            var oA = d0T * hodge1 * d0;
            var cA = SparseMatrix.CreateDiagonal(oA.RowCount, oA.RowCount, 1e-8);
            A = oA + cA;

            var oB = d1 * hodge1Inv * d1T;
            var cB = SparseMatrix.CreateDiagonal(oB.RowCount, oB.RowCount, 1e-8);
            B = oB + cB;
        }

        public float[] ComputeExactComponent(DenseMatrix omega) {
            var rslt = d0T * hodge1 * omega;
            var n = rslt.RowCount;
            var rsltArr = new float[n];
            var trps = A.Storage.EnumerateNonZeroIndexed().Select(t => new Triplet(t.Item3, t.Item1, t.Item2)).ToArray();
            var outs = new float[n];
            for (var i = 0; i < n; i++) { rsltArr[i] = (float)rslt[i, 0]; }
            Solver.DecompAndSolveChol(trps.Length, n, trps, rsltArr, outs);
            var tmp1 = DenseMatrix.OfColumnMajor(outs.Length, 1, outs.Select(v => (double)v).ToArray());
            var tmp2 = this.d0 * tmp1;
            var l = omega.RowCount;
            var rsltArr2 = new float[l];
            for (var i = 0; i < l; i++) { rsltArr2[i] = (float)tmp2[i, 0]; }
            return rsltArr2;
        }

        public float[] ComputeCoExactComponent(DenseMatrix omega) {
            var rslt = d1 * omega;
            var n = rslt.RowCount;
            var rsltArr = new float[n];
            var trps = B.Storage.EnumerateNonZeroIndexed().Select(t => new Triplet(t.Item3, t.Item1, t.Item2)).ToArray();
            var outs = new float[n];
            for (var i = 0; i < n; i++) { rsltArr[i] = (float)rslt[i, 0]; }
            Solver.DecompAndSolveChol(trps.Length, n, trps, rsltArr, outs);
            //Solver.DecompAndSolveLU(trps.Length, n, trps, rsltArr, outs);
            var tmp1 = DenseMatrix.OfColumnMajor(outs.Length, 1, outs.Select(v => (double)v).ToArray());
            var tmp2 = this.hodge1Inv * this.d1T * tmp1;
            var l = omega.RowCount;
            var rsltArr2 = new float[l];
            for (var i = 0; i < l; i++) { rsltArr2[i] = (float)tmp2[i, 0]; }
            return rsltArr2;
        }

        public float[] ComputeHarmonicComponent(DenseMatrix omega) {
            throw new System.Exception();
        }
    }
}
