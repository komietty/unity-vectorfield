using System.Linq;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra;

namespace VectorField {
    using RSprs = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using CSprs = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
    using Complex = System.Numerics.Complex;
    using RV = Vector<double>;
    using CV = Vector<System.Numerics.Complex>;

    public static class Solver {

        public static RV Cholesky(RSprs lhs, RV rhs) => Cholesky(lhs, rhs.ToArray());
        public static RV Cholesky(RSprs lhs, double[] rhs){
            var sln = new double[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new Trp<double>(t)).ToArray();
            DecompAndSolveCholReal(trp.Length, rhs.Length, trp, rhs, sln);
            return RV.Build.DenseOfArray(sln);
        }

        public static RV LU(RSprs lhs, RV rhs) => LU(lhs, rhs.ToArray());
        public static RV LU(RSprs lhs, double[] rhs) {
            var sln = new double[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new Trp<double>(t)).ToArray();
            DecompAndSolveLUReal(trp.Length, rhs.Length, trp, rhs, sln);
            return RV.Build.DenseOfArray(sln);
        }
        
        public static RV QR(RSprs lhs, RV rhs) => QR(lhs, rhs.ToArray());
        public static RV QR(RSprs lhs, double[] rhs) {
            var sln = new double[lhs.ColumnCount];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new Trp<double>(t)).ToArray();
            DecompAndSolveQRReal(trp.Length, lhs.RowCount, lhs.ColumnCount, trp, rhs, sln);
            return RV.Build.DenseOfArray(sln);
        }
        
        public static CV LUComp(CSprs lhs, CV rhs) => LUComp(lhs, rhs.ToArray());
        public static CV LUComp(CSprs lhs, Complex[] rhs) {
            var sln = new Complex[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new Trp<Complex>(t)).ToArray();
            DecompAndSolveLUComp(trp.Length, rhs.Length, trp, rhs, sln);
            return CV.Build.DenseOfArray(sln);
        }

        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        [DllImport("EigenSolver.bundle")]
        static extern void DecompAndSolveCholReal(
            int ntrps,
            int nresult,
            [In]  Trp<double>[] trplets,
            [In]  double[] result,
            [Out] double[] answer
        );

        [DllImport("EigenSolver.bundle")]
        static extern void DecompAndSolveLUReal(
            int ntrps,
            int nresult,
            [In]  Trp<double>[] trplets,
            [In]  double[] result,
            [Out] double[] answer
        );
        
        [DllImport("EigenSolver.bundle")]
        static extern void DecompAndSolveQRReal(
            int ntrps,
            int nrows,
            int ncols,
            [In]  Trp<double>[] a_strage,
            [In]  double[] rhs,
            [Out] double[] result
        );
        
        [DllImport("EigenSolver.bundle")]
        static extern void DecompAndSolveLUComp(
            int ntrps,
            int nresult,
            [In]  Trp<Complex>[] trplets,
            [In]  Complex[] result,
            [Out] Complex[] answer
        );
        #endif

        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("EigenSolver.dll")]
        static extern void DecompAndSolveCholReal(
            int ntrps,
            int nresult,
            [In]  Trp<double>[] trplets,
            [In]  double[] result,
            [Out] double[] answer
        );

        [DllImport("EigenSolver.dll")]
        static extern void DecompAndSolveLUReal(
            int ntrps,
            int nresult,
            [In]  Trp<double>[] trplets,
            [In]  double[] result,
            [Out] double[] answer
        );
        #endif

        /*
         * Triplet for Sparse Matrix.
         * v: value
         * i: row number
         * j: clm number
        */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Trp<T> {
            public T v;
            public int i;
            public int j;

            public Trp((int i, int j, T v) val) {
                this.v = val.v;
                this.i = val.i;
                this.j = val.j;
            }
        }
    }
}