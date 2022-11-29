using UnityEngine;
using System.Runtime.InteropServices;

namespace ddg {
    public static class Solver {
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
                int nvrts,
                [In]  Triplet[] trps,
                [In]  float[] vrts,
                [Out] float[] outs
            );
        [DllImport("EigenSolver.bundle")]
            public static extern void DecompAndSolveLU(
                int ntrps,
                int nvrts,
                [In]  Triplet[] trps,
                [In]  float[] vrts,
                [Out] float[] outs
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
    }
}