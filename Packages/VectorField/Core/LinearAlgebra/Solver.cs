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
        
        public static CV LUComp(CSprs lhs, CV rhs) => LUComp(lhs, rhs.ToArray());
        public static CV LUComp(CSprs lhs, Complex[] rhs) {
            var sln = new Complex[rhs.Length];
            var trp = lhs.Storage.EnumerateNonZeroIndexed().Select(t => new Trp<Complex>(t)).ToArray();
            DecompAndSolveLUComp(trp.Length, rhs.Length, trp, rhs, sln);
            return CV.Build.DenseOfArray(sln);
        }

        /*
         * Computes the solution of the poisson problem Ax = -M(rho - rhoBar),
         * where A is the positive definite laplace matrix and M is the mass matrix.
         * rho: A scalar density of vertices of the input mesh.
        */
        public static RV ScalarPoissonProblem(HeGeom g, RV rho){
            var M = Operator.Mass(g);
            var A = Operator.Laplace(g);
            var T = g.TotalArea();
            var rhoSum = (M * rho).Sum();
            var rhoBar = MathNet.Numerics.LinearAlgebra.Double.DenseVector.Create(M.RowCount, rhoSum / T);
            return Cholesky(A, -M * (rho - rhoBar));
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