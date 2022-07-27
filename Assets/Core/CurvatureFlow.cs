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
        bool native = false;
        int nv => geom.nVerts;
        
        public MeanCurvatureFlow(HalfEdgeGeom geom, Type type, bool native) {
            this.geom = geom;
            this.type = type;
            this.native = native;
            if (type == Type.Modified) L = GenLaplaceMtx();
        }

        public SparseMatrix GenFlowMtx(double h){
            if (type == Type.Simple) L = GenLaplaceMtx();
            var I = SparseMatrix.CreateIdentity(nv);
            var M = GenInversedMassMtx();
            return I + (M * L) * h;
        }

        public SparseMatrix GenInversedMassMtx(){
            Span<double> a = stackalloc double[nv];
            for (int i = 0; i < nv; i++) a[i] = 1 / geom.BarycentricDualArea(geom.Verts[i]);
            return SparseMatrix.OfDiagonalArray(a.ToArray());
        }

        public SparseMatrix GenLaplaceMtx(){
            var t = new List<(int, int, double)>();
            for (var i = 0; i < nv; i++) {
                var v = geom.Verts[i];
                var s = 0f;
                foreach (var h in geom.GetAdjacentHalfedges(v)) {
                    var a = geom.Cotan(h);
                    var b = geom.Cotan(h.twin);
                    var c = (a + b) * 0.5f;
                    t.Add((i, h.next.vid, -c));
                    s += c;
                }
                t.Add((i, i, s));
            }
            var M = SparseMatrix.OfIndexed(nv, nv, t);
            var C = SparseMatrix.CreateDiagonal(nv, nv, 1e-8d);
            return M + C;
        }

        public void Integrate(double h){
            var fm = GenFlowMtx(h);

            if (native) {
                var data = SparseCompressedRowMatrixStorage<double>.OfMatrix(fm.Storage);
                var iter = data.EnumerateNonZeroIndexed();
                var outs = new Vector3[nv];
                var trps = iter.Select(i => new Trp(i.Item3, i.Item1, i.Item2)).ToArray();
                SolveLU(iter.Count(), nv, trps, geom.pos, outs);
                geom.pos = outs;
            } else {
                var f0 = new DenseMatrix(nv, 3);
                foreach (var v in geom.Verts) {
                    var p = geom.Pos[v.vid];
                    f0.SetRow(v.vid, new double[3] { p.x, p.y, p.z });
                }
                var lu = fm.LU();
                var fh = lu.Solve(f0);
                for (var i = 0; i < nv; i++) {
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
