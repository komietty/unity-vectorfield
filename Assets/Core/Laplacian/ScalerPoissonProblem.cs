using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using UnityEngine;
using System.Linq;

namespace ddg {
    public static class ScalerPoissonProblem {
        /*
         * Computes the solution of the poisson problem Ax = -M(rho - rhoBar),
         * where A is the positive definite laplace matrix and M is the mass matrix.
         * rho: A scalar density of vertices of the input mesh.
        */
        public static Matrix<double> SolveOnSurface(HalfEdgeGeom geom, DenseMatrix rho){
            var M = Operator.Mass(geom);
            var A = Operator.Laplace(geom);
            var T = geom.TotalArea();
            var rhoSum = (M * rho).RowSums().Sum();
            //UnityEngine.Debug.Log(rhoSum);
            //UnityEngine.Debug.Log(T);
            var rhoBar = DenseMatrix.Create(M.RowCount, 1, rhoSum / T);
            var rhoDif = rho - rhoBar;
            var B = - M * rhoDif;
            var LLT = A.LU();
            return LLT.Solve(B);
        }

        public static float[] SolveOnSurfaceNative(HalfEdgeGeom geom, DenseMatrix rho){
            var M = Operator.Mass(geom);
            var A = Operator.Laplace(geom);
            var T = geom.TotalArea();
            var rhoSum = (M * rho).RowSums().Sum();
            var rhoBar = DenseMatrix.Create(M.RowCount, 1, rhoSum / T);
            var rhoDif = rho - rhoBar;
            var B = - M * rhoDif;
            var data = SparseCompressedRowMatrixStorage<double>.OfMatrix(A.Storage);
            var iter = data.EnumerateNonZeroIndexed();
            var outs = new float[geom.nVerts];
            var trps = iter.Select(i => new Triplet(i.Item3, i.Item1, i.Item2)).ToArray();
            var rslt = new float[B.RowCount];
            for(var i = 0; i < rslt.Length; i++) {
                rslt[i] = (float)B[i, 0];
            }
            Solver.DecompAndSolveChol(iter.Count(), geom.nVerts, trps, rslt, outs);
            return outs;
        }
    }
}
