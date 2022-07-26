using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using System.Runtime.InteropServices;
using System.Linq;
using System;

namespace ddg {
    public class MeanCurvatureFlow {
        [DllImport("libEigenDll")] static extern void SolveLU(int ntrps, int nvrts, [In] Trp[] trps, [In] Vector3[] vrts, [Out] Vector3[] outs);
        public enum Type { Simple, Modified }
        Type type = Type.Simple;
        HalfEdgeGeom geom;
        SparseMatrix L;
        int l => geom.nVerts;
        bool native = false;
        
        public MeanCurvatureFlow(HalfEdgeGeom geom, Type type) {
            this.geom = geom;
            this.type = type;
            if (type == Type.Modified) L = GenLaplaceMtx();
        }

        public SparseMatrix GenFlowMtx(double h){
            if (type == Type.Simple) L = GenLaplaceMtx();
            var I = SparseMatrix.CreateIdentity(l);
            var M = GenInversedMassMtx();
            return I + (M * L) * h;
        }

        public SparseMatrix GenInversedMassMtx(){
            Span<double> a = stackalloc double[l];
            for (var i = 0; i < l; i++) { a[i] = 1 / geom.BarycentricDualArea(geom.Verts[i]); }
            return SparseMatrix.OfDiagonalArray(a.ToArray());
        }

        public SparseMatrix GenLaplaceMtx(){
            var t = new List<(int, int, double)>();
            for (var i = 0; i < l; i++) {
                var v = geom.Verts[i];
                var s = 0f;
                foreach (var h in v.GetAdjacentHalfedges(geom.halfedges)) {
                    var a = geom.Cotan(h);
                    var b = geom.Cotan(h.twin);
                    var c = (a + b) * 0.5f;
                    t.Add((i, h.next.vid, -c));
                    s += c;
                }
                t.Add((i, i, s));
            }
            var M = SparseMatrix.OfIndexed(l, l, t);
            var C = SparseMatrix.CreateDiagonal(l, l, 1e-8d);
            return M + C;
        }

        public void Integrate(double h){
            var fm = GenFlowMtx(h);

            if (native) {
                var storage = SparseCompressedRowMatrixStorage<double>.OfMatrix(fm.Storage);
                var itrator = storage.EnumerateNonZeroIndexed();
                var c = itrator.Count();
                var trps = new Trp[c];
                var vrts = new Vector3[l];
                var outs = new Vector3[l];
                var itr = 0;
                foreach (var v in itrator) { trps[itr] = new Trp(v.Item3, v.Item1, v.Item2); itr++; }
                for (var i = 0; i < l; i++) { vrts[i] = geom.Pos[i]; }

                SolveLU(c, l, trps, vrts, outs);

                MeshUtils.Normalize(outs, true);
                for (var i = 0; i < l; i++) { geom.Pos[i] = outs[i]; }
            }else {
                var f0 = new DenseMatrix(l, 3);
                foreach (var v in geom.Verts) {
                    var p = geom.Pos[v.vid];
                    f0.SetRow(v.vid, new double[3] { p.x, p.y, p.z });
                }
                var lu = fm.LU();
                var fh = lu.Solve(f0);

                for (var i = 0; i < l; i++) {
                    var r = fh.Row(i);
                    var p = new Vector3((float)r[0], (float)r[1], (float)r[2]);
                    geom.Pos[i] = p;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Trp {
            public double v;
            public int i;
            public int j;
            public Trp(double v, int i, int j) {
                this.v = v;
                this.i = i;
                this.j = j;
            }
        }
    }
}
