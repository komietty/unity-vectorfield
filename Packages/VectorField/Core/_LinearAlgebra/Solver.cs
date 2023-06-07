using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;
using RS = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
using CS = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
using RD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using CD = MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix;
using CV = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;
using RV = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Complex = System.Numerics.Complex;

namespace VectorField {
    public static class Solver {

        static double Residual(CS A, CV x) {
            Assert.IsTrue(A.IsHermitian());
            var Ax = A * x;
            var lambda = x.ConjugateDotProduct(Ax) / x.ConjugateDotProduct(x);
            return (Ax - x * lambda).L2Norm() / x.L2Norm();
        }

        public static RV SmallestEigenPositiveDefinite(RS A, RS B) {
            var TA = A.Storage.EnumerateNonZeroIndexed().Select(t => new TrpReal(t)).ToArray();
            var TB = B.Storage.EnumerateNonZeroIndexed().Select(t => new TrpReal(t)).ToArray();
            var x = new double[A.ColumnCount];
            SmallestEigenPositiveDefiniteReal(TA.Length, TB.Length, A.RowCount, A.ColumnCount, TA, TB, x);
            return RV.Build.DenseOfArray(x);
        }
        
        public static CV SmallestEigenPositiveDefinite(CS A, CS B) {
            var TA = A.Storage.EnumerateNonZeroIndexed().Select(t => new TrpComp(t)).ToArray();
            var TB = B.Storage.EnumerateNonZeroIndexed().Select(t => new TrpComp(t)).ToArray();
            var x = new Complex[A.ColumnCount];
            SmallestEigenPositiveDefinite(TA.Length, TB.Length, A.RowCount, A.ColumnCount, TA, TB, x);
            return CV.Build.DenseOfArray(x);
        }

        public static CV InversePowerMethod(CS A) {
            var rhs = CV.Build.Random(A.RowCount);
            var trp = A.Storage.EnumerateNonZeroIndexed().Select(t => new TrpComp(t)).ToArray();
            for (var i = 0; i < 100; i++) {
                rhs = CholeskyComp(trp, rhs.ToArray());
                rhs -= CV.Build.Dense(rhs.Count, rhs.Sum() / rhs.Count);
                rhs *= new Complex(1d / rhs.L2Norm(), 0);
                if (Residual(A, rhs) < 1e-10) break;
            }
            return rhs;
        }

        public static RV Cholesky(RS lhs, RV rhs) => Cholesky(lhs, rhs.ToArray());
        public static RV Cholesky(RS lhs, double[] rhs){
            var sln = new double[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new TrpReal(t)).ToArray();
            DecompAndSolveCholReal(trp.Length, rhs.Length, trp, rhs, sln);
            return RV.Build.DenseOfArray(sln);
        }

        public static RV LU(RS lhs, RV rhs) => LU(lhs, rhs.ToArray());
        public static RV LU(RS lhs, double[] rhs) {
            var sln = new double[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new TrpReal(t)).ToArray();
            DecompAndSolveLUReal(trp.Length, rhs.Length, trp, rhs, sln);
            return RV.Build.DenseOfArray(sln);
        }
        
        public static RV QR(RS lhs, RV rhs) => QR(lhs, rhs.ToArray());
        public static RV QR(RS lhs, double[] rhs) {
            var sln = new double[lhs.ColumnCount];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new TrpReal(t)).ToArray();
            DecompAndSolveQRReal(trp.Length, lhs.RowCount, lhs.ColumnCount, trp, rhs, sln);
            return RV.Build.DenseOfArray(sln);
        }
        
        public static CV LUComp(CS lhs, CV rhs) => LUComp(lhs, rhs.ToArray());
        public static CV LUComp(CS lhs, Complex[] rhs) {
            var sln = new Complex[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new TrpComp(t)).ToArray();
            DecompAndSolveLUComp(trp.Length, rhs.Length, trp, rhs, sln);
            return CV.Build.DenseOfArray(sln);
        }
        
        public static CV CholeskyComp(TrpComp[] lhs, Complex[] rhs) {
            var sln = new Complex[rhs.Length];
            DecompAndSolveCholComp(lhs.Length, rhs.Length, lhs, rhs, sln);
            return CV.Build.DenseOfArray(sln);
        }
        public static CV CholeskyComp(CS lhs, Complex[] rhs){
            var sln = new Complex[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new TrpComp(t)).ToArray();
            DecompAndSolveCholComp(trp.Length, rhs.Length, trp, rhs, sln);
            return CV.Build.DenseOfArray(sln);
        }

        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        [DllImport("EigenSolver.bundle")]
        static extern void DecompAndSolveCholReal(
            int ntrps,
            int nresult,
            [In]  TrpReal[] trplets,
            [In]  double[] result,
            [Out] double[] answer
        );

        [DllImport("EigenSolver.bundle")]
        static extern void DecompAndSolveLUReal(
            int ntrps,
            int nresult,
            [In]  TrpReal[] trplets,
            [In]  double[] result,
            [Out] double[] answer
        );
        
        [DllImport("EigenSolver.bundle")]
        static extern void DecompAndSolveQRReal(
            int ntrps,
            int nrows,
            int ncols,
            [In]  TrpReal[] a_strage,
            [In]  double[] rhs,
            [Out] double[] result
        );
        
        [DllImport("EigenSolver.bundle")]
        static extern void DecompAndSolveLUComp(
            int ntrps,
            int nresult,
            [In]  TrpComp[] trplets,
            [In]  Complex[] result,
            [Out] Complex[] answer
        );
        
        [DllImport("EigenSolver.bundle")]
        static extern void DecompAndSolveCholComp(
            int ntrps,
            int nresult,
            [In]  TrpComp[] trplets,
            [In]  Complex[] result,
            [Out] Complex[] answer
        );
        
        [DllImport("EigenSolver.bundle")]
        static extern void SmallestEigenPositiveDefiniteReal(
            int ntrpsA,
            int ntrpsB,
            int nrow,
            int ncol,
            [In]  TrpReal[] trpsA,
            [In]  TrpReal[] trpsB,
            [Out] double[] answer
        );
        
        [DllImport("EigenSolver.bundle")]
        static extern void SmallestEigenPositiveDefinite(
            int ntrpsA,
            int ntrpsB,
            int nrow,
            int ncol,
            [In]  TrpComp[] trpsA,
            [In]  TrpComp[] trpsB,
            [Out] Complex[] answer
        );
        #endif

        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("EigenSolver.dll")]
        static extern void DecompAndSolveCholReal(
            int ntrps,
            int nresult,
            [In]  TrpReal[] trplets,
            [In]  double[] result,
            [Out] double[] answer
        );

        [DllImport("EigenSolver.dll")]
        static extern void DecompAndSolveLUReal(
            int ntrps,
            int nresult,
            [In]  TrpReal[] trplets,
            [In]  double[] result,
            [Out] double[] answer
        );
        
        [DllImport("EigenSolver.dll")]
        static extern void DecompAndSolveQRReal(
            int ntrps,
            int nrows,
            int ncols,
            [In]  TrpReal[] a_strage,
            [In]  double[] rhs,
            [Out] double[] result
        );
        
        [DllImport("EigenSolver.dll")]
        static extern void DecompAndSolveLUComp(
            int ntrps,
            int nresult,
            [In]  TrpComp[] trplets,
            [In]  Complex[] result,
            [Out] Complex[] answer
        );
        
        [DllImport("EigenSolver.dll")]
        static extern void SmallestEigenPositiveDefiniteReal(
            int ntrpsA,
            int ntrpsB,
            int nrow,
            int ncol,
            [In]  TrpReal[] trpsA,
            [In]  TrpReal[] trpsB,
            [Out] double[] answer
        );
        
        [DllImport("EigenSolver.dll")]
        static extern void SmallestEigenPositiveDefinite(
            int ntrpsA,
            int ntrpsB,
            int nrow,
            int ncol,
            [In]  TrpComp[] trpsA,
            [In]  TrpComp[] trpsB,
            [Out] Complex[] answer
        );
        #endif

        /*
         * Triplet for Sparse Matrix.
         * v: value
         * i: row number
         * j: clm number
         * NOTE: generic pattern does not work with .dll file!!
        */
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TrpReal {
            public double v;
            public int i;
            public int j;

            public TrpReal((int i, int j, double v) val) {
                this.v = val.v;
                this.i = val.i;
                this.j = val.j;
            }
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TrpComp {
            public Complex v;
            public int i;
            public int j;

            public TrpComp((int i, int j, Complex v) val) {
                this.v = val.v;
                this.i = val.i;
                this.j = val.j;
            }
        }
    }
}