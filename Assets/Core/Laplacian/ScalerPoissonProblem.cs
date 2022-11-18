using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

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
            var rhoBar = DenseMatrix.Create(M.RowCount, 1, rhoSum / T);
            var rhoDif = rho - rhoBar;
            var B = - M * rhoDif;
            var LLT = A.LU();
            return LLT.Solve(B);
            //var data = SparseCompressedRowMatrixStorage<double>.OfMatrix(A.Storage);
            //var iter = data.EnumerateNonZeroIndexed();
            //var outs = new Vector3[geom.nVerts];
            //var trps = iter.Select(i => new Triplet(i.Item3, i.Item1, i.Item2)).ToArray();
            //Solver.SolveLU(iter.Count(), geom.nVerts, trps, geom.pos, outs);
        }
    }
}
