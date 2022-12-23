using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ddg {
    using Vd = Vector<double>;

    public static class Solver {

        public static DenseVector Cholesky(SparseMatrix lhs, Vd rhs) => Cholesky(lhs, rhs.ToArray());
        public static DenseVector LU(SparseMatrix lhs, Vd rhs) => LU(lhs, rhs.ToArray());

        public static DenseVector Cholesky(SparseMatrix lhs, double[] rhs){
            var outs = new double[rhs.Length];
            var trps = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new Triplet(t)).ToArray();
            DecompAndSolveChol(trps.Length, rhs.Length, trps, rhs, outs);
            return DenseVector.OfArray(outs);
        }

        public static DenseVector LU(SparseMatrix lhs, double[] rhs){
            var outs = new double[rhs.Length];
            var trps = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new Triplet(t)).ToArray();
            DecompAndSolveLU(trps.Length, rhs.Length, trps, rhs.ToArray(), outs);
            return DenseVector.OfArray(outs);
        }
        
        /*
         * LU decomposition and solver bundle
        */
        [DllImport("EigenSolver.bundle")]
            public static extern void DecompAndSolveLUVec(
                int ntrps,
                int nvrts,
                [In]  Triplet[] trps,
                [In]  Vector3[] vrts,
                [Out] Vector3[] outs
            );

        [DllImport("EigenSolver.bundle")]
            public static extern void DecompAndSolveChol(
                int ntrps,
                int nresult,
                [In]  Triplet[] trps,
                [In]  double[] result,
                [Out] double[] answer
            );
        [DllImport("EigenSolver.bundle")]
            public static extern void DecompAndSolveLU(
                int ntrps,
                int nresult,
                [In]  Triplet[] trps,
                [In]  double[] result,
                [Out] double[] answer
            );
    }

    /*
     * Triplet for Sparse Matrix.
     * v: value
     * i: row number
     * j: clm number
    */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Triplet {
        public double v;
        public int i;
        public int j;

        public Triplet(double v, int i, int j) {
            this.v = v;
            this.i = i;
            this.j = j;
        }

        public Triplet((int i, int j, double v) val) {
            this.v = val.v;
            this.i = val.i;
            this.j = val.j;
        }
    }
}