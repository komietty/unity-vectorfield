using System.Linq;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace VectorField {
    using RSprs = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using CSprs = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
    using Complex = System.Numerics.Complex;
    using RV = Vector<double>;
    using CV = Vector<System.Numerics.Complex>;
    using RD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
    using CD = MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix;

    public static class Solver {

        static double Residual(CSprs A, CV x) {
            var Ax = A * x;
            return 0;
        }

        public static RV SmallestEigenPositiveDefinite(RSprs A, RSprs B) {
            var trpA = A.Storage.EnumerateNonZeroIndexed().Select(t => new TrpReal(t)).ToArray();
            var trpB = B.Storage.EnumerateNonZeroIndexed().Select(t => new TrpReal(t)).ToArray();
            var res_x = new double[A.ColumnCount];
            Assert.IsTrue(A.RowCount == B.RowCount && A.ColumnCount == B.ColumnCount);
            SmallestEigenPositiveDefiniteReal(
                trpA.Length,
                trpB.Length,
                A.RowCount,
                A.ColumnCount,
                trpA,
                trpB,
                res_x
            );
            return RV.Build.DenseOfArray(res_x);
        }
        
        public static CV SmallestEigenPositiveDefinite(CSprs A, CSprs B) {
            var trpA = A.Storage.EnumerateNonZeroIndexed().Select(t => new TrpComp(t)).ToArray();
            var trpB = B.Storage.EnumerateNonZeroIndexed().Select(t => new TrpComp(t)).ToArray();
            var res_x = new Complex[A.ColumnCount];
            Assert.IsTrue(A.RowCount == B.RowCount && A.ColumnCount == B.ColumnCount);
            SmallestEigenPositiveDefinite(
                trpA.Length,
                trpB.Length,
                A.RowCount,
                A.ColumnCount,
                trpA,
                trpB,
                res_x
            );
            return CV.Build.DenseOfArray(res_x);
        }
        
        public static CV InversePowerMethod(CSprs A) {
            var n = A.RowCount;
            var rhs = CV.Build.Random(n).ToArray();
            var trp = A.Storage.EnumerateNonZeroIndexed().Select(t => new TrpComp(t)).ToArray();
            for (var i = 0; i < 10; i++) {
                var sln = new Complex[rhs.Length];
                DecompAndSolveCholComp(trp.Length, rhs.Length, trp, rhs, sln);
                var mean = new Complex();
                var norm = 0d;
                for (var j = 0; j < sln.Length; j++) {
                    var s = sln[j];
                    mean += s;
                    norm += s.Real * s.Real - s.Imaginary * s.Imaginary;
                }
                mean /= sln.Length;
                norm = math.sqrt(norm);
                
                for (var j = 0; j < sln.Length; j++)
                    rhs[j] = (sln[j] - mean * new Complex(1, 1)) * new Complex(1.0 / norm, 0);
            }
            return CV.Build.DenseOfArray(rhs);
        }

        public static RV Cholesky(RSprs lhs, RV rhs) => Cholesky(lhs, rhs.ToArray());
        public static RV Cholesky(RSprs lhs, double[] rhs){
            var sln = new double[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new TrpReal(t)).ToArray();
            DecompAndSolveCholReal(trp.Length, rhs.Length, trp, rhs, sln);
            return RV.Build.DenseOfArray(sln);
        }

        public static RV LU(RSprs lhs, RV rhs) => LU(lhs, rhs.ToArray());
        public static RV LU(RSprs lhs, double[] rhs) {
            var sln = new double[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new TrpReal(t)).ToArray();
            DecompAndSolveLUReal(trp.Length, rhs.Length, trp, rhs, sln);
            return RV.Build.DenseOfArray(sln);
        }
        
        public static RV QR(RSprs lhs, RV rhs) => QR(lhs, rhs.ToArray());
        public static RV QR(RSprs lhs, double[] rhs) {
            var sln = new double[lhs.ColumnCount];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new TrpReal(t)).ToArray();
            DecompAndSolveQRReal(trp.Length, lhs.RowCount, lhs.ColumnCount, trp, rhs, sln);
            return RV.Build.DenseOfArray(sln);
        }
        
        public static CV LUComp(CSprs lhs, CV rhs) => LUComp(lhs, rhs.ToArray());
        public static CV LUComp(CSprs lhs, Complex[] rhs) {
            var sln = new Complex[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new TrpComp(t)).ToArray();
            DecompAndSolveLUComp(trp.Length, rhs.Length, trp, rhs, sln);
            return CV.Build.DenseOfArray(sln);
        }
        
        public static CV CholeskyComp(CSprs lhs, CV rhs) => CholeskyComp(lhs, rhs.ToArray());
        public static CV CholeskyComp(CSprs lhs, Complex[] rhs){
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