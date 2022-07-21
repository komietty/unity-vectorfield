using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace ddg {
    public class DenseMtx: IDisposable {
        public enum Type { Identity, Zeros, Ones, Const }
        [DllImport("libEigenDll")] static extern IntPtr GenDenseIdentity(int nrow, int ncol);
        [DllImport("libEigenDll")] static extern IntPtr GenDenseZeros(int nrow, int ncol);
        [DllImport("libEigenDll")] static extern IntPtr GenDenseOnes(int nrow, int ncol);
        [DllImport("libEigenDll")] static extern IntPtr GenDenseConst(int nrow, int ncol, float v);

        [DllImport("libEigenDll")] static extern void GetValues(IntPtr m, [In, Out] float[] values);

        [DllImport("libEigenDll")] static extern IntPtr Plus(IntPtr m1, IntPtr m2);
        [DllImport("libEigenDll")] static extern IntPtr Minus(IntPtr m1, IntPtr m2);
        [DllImport("libEigenDll")] static extern IntPtr Times(IntPtr m1, IntPtr m2);
        [DllImport("libEigenDll")] static extern IntPtr Invert(IntPtr m);
        [DllImport("libEigenDll")] static extern void FreeAllocDense(IntPtr m);
        [DllImport("libEigenDll")] static extern int nCols(IntPtr m);
        [DllImport("libEigenDll")] static extern int nRows(IntPtr m);

        public IntPtr m { get; }
        public int NCols;
        public int NRows;

        public DenseMtx(Type t, int nrow, int ncol, float v = 0) {
            this.NRows = nrow;
            this.NCols = ncol;
            switch(t) {
                case Type.Identity: m = GenDenseIdentity(nrow, ncol); break;
                case Type.Zeros:    m = GenDenseZeros(nrow, ncol);    break;
                case Type.Ones:     m = GenDenseOnes(nrow, ncol);     break;
                case Type.Const:    m = GenDenseConst(nrow, ncol, v); break;
                default: throw new Exception();
            }
        }
        
        public float[] GetValues() {
            var arr = new float[NRows * NCols];
            GetValues(m, arr);
            return arr;
        }

        public void Dispose() {
            FreeAllocDense(m);
        }
    }
}
