using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ddg {
    using S = SparseMatrix;
    using V = Vector<double>;

    public static class Solver {

        public static V Cholesky(S lhs, V rhs) => Cholesky(lhs, rhs.ToArray());
        public static V Cholesky(S lhs, double[] rhs){
            var sln = new double[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new T(t)).ToArray();
            DecompAndSolveChol(trp.Length, rhs.Length, trp, rhs, sln);
            return DenseVector.OfArray(sln);
        }

        public static V LU(S lhs, V rhs) => LU(lhs, rhs.ToArray());
        public static V LU(S lhs, double[] rhs) {
            var sln = new double[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new T(t)).ToArray();
            DecompAndSolveLU(trp.Length, rhs.Length, trp, rhs, sln);
            return DenseVector.OfArray(sln);
        }

        /*
         * Computes the solution of the poisson problem Ax = -M(rho - rhoBar),
         * where A is the positive definite laplace matrix and M is the mass matrix.
         * rho: A scalar density of vertices of the input mesh.
        */
        public static V ScalarPoissonProblem(HeGeom g, V rho){
            var M = Operator.Mass(g);
            var A = Operator.Laplace(g);
            var T = g.TotalArea();
            var rhoSum = (M * rho).Sum();
            var rhoBar = DenseVector.Create(M.RowCount, rhoSum / T);
            return Cholesky(A, -M * (rho - rhoBar));
            // C#
            //return A.LU().Solve(-M * (rho - rhoBar));
        }
        
        [DllImport("EigenSolver.bundle")]
            public static extern void DecompAndSolveLUVec(
                int ntrps,
                int nvrts,
                [In]  T[] trplets,
                [In]  Vector3[] vrts,
                [Out] Vector3[] outs
            );

        [DllImport("EigenSolver.bundle")]
            static extern void DecompAndSolveChol(
                int ntrps,
                int nresult,
                [In]  T[] trplets,
                [In]  double[] result,
                [Out] double[] answer
            );
        [DllImport("EigenSolver.bundle")]
            static extern void DecompAndSolveLU(
                int ntrps,
                int nresult,
                [In]  T[] trplets,
                [In]  double[] result,
                [Out] double[] answer
            );

        /*
         * Triplet for Sparse Matrix.
         * v: value
         * i: row number
         * j: clm number
        */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct T {
            public double v;
            public int i;
            public int j;

            public T((int i, int j, double v) val) {
                this.v = val.v;
                this.i = val.i;
                this.j = val.j;
            }
        }
    }
}