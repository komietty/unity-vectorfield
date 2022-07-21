using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

namespace ddg {
    public class SparseMtx: IDisposable {
        [DllImport("libSparseMatrixDll")] static extern IntPtr GenDiagMtx(float[] vals, int len);
        [DllImport("libSparseMatrixDll")] static extern IntPtr GenSparseMtx(Triplet[] vals, int len);
        /*
        [DllImport("libSparseMatrixDll")] static extern IntPtr Plus(IntPtr m1, IntPtr m2);
        [DllImport("libSparseMatrixDll")] static extern IntPtr Minus(IntPtr m1, IntPtr m2);
        [DllImport("libSparseMatrixDll")] static extern IntPtr Times(IntPtr m1, IntPtr m2);
        [DllImport("libSparseMatrixDll")] static extern IntPtr Invert(IntPtr m);
        [DllImport("libSparseMatrixDll")] static extern IntPtr ToDense(IntPtr m);
        */
        [DllImport("libSparseMatrixDll")] static extern void FreeAlloc(IntPtr m);
        [DllImport("libSparseMatrixDll")] static extern int nCols(IntPtr m);
        [DllImport("libSparseMatrixDll")] static extern int nRows(IntPtr m);

        [DllImport("libSparseMatrixDll")] static extern IntPtr GenTestSparseMtx();

        public IntPtr m { get; }
        public int NCols => nCols(m);
        public int NRows => nRows(m);

        public SparseMtx(float[] vals){
            this.m = GenDiagMtx(vals, vals.Length);
        }

        public SparseMtx() { this.m = GenTestSparseMtx(); }
        public SparseMtx(IntPtr m) { this.m = m; }
        public SparseMtx(Triplet[] t){ m = GenSparseMtx(t, t.Length); }

        /*
        public static SparseMtx Plus (SparseMtx m1, SparseMtx m2) { return new SparseMtx(Plus(m1.m, m2.m));  }
        public static SparseMtx Minus(SparseMtx m1, SparseMtx m2) { return new SparseMtx(Minus(m1.m, m2.m)); }

        public static SparseMtx Times(SparseMtx m1, SparseMtx m2) {
            if (m1.NCols != m2.NRows) throw new Exception("nrows ans ncols are not correct");
            return new SparseMtx(Times(m1.m, m2.m));
        }



        public void Invert()  { Invert(m); }
        */
        public void Dispose() { FreeAlloc(m); }

        //public DenseMtx ToDense() { }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Triplet {
        float v;
        int i;
        int j;
        public Triplet(float v, int i, int j) {
            this.v = v;
            this.i = i;
            this.j = j;
        }
    }
}