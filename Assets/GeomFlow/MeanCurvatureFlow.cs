using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ddg {
    public class MeanCurvatureFlow {
        HalfEdgeGeom geom;
        SparseMatrix massMtx;
        SparseMatrix lplcMtx;
        SparseMatrix flowMtx;
        int l => geom.mesh.verts.Length;
        double[] a;
        
        public MeanCurvatureFlow(HalfEdgeGeom geom) {
            this.geom = geom;
        }

        public SparseMatrix GenFlowMtx(double h){
            massMtx = GenMassMtx();
            lplcMtx = GenLaplaceMtx();
            //Debug.Log(massMtx.ToString());
            //Debug.Log(lplcMtx.ToString());
            var I = SparseMatrix.CreateIdentity(l);
            var inv = new double[l];
            for (var i = 0; i < l; i++) { inv[i] = 1.0 / a[i]; }
            var invM = SparseMatrix.OfDiagonalArray(inv);
            var flow = I + (invM * lplcMtx) * h;
            //Debug.Log(invM);
            //Debug.Log(flow);
            return flow;
        }

        public SparseMatrix GenMassMtx(){
            a = new double[l];
            for (var i = 0; i < l; i++) {
                a[i] = geom.BarycentricDualArea(geom.mesh.verts[i]);
            }
            return SparseMatrix.OfDiagonalArray(a);
        }

        public SparseMatrix GenLaplaceMtx(){
            var t = new List<(int, int, double)>();
            for (var i = 0; i < l; i++) {
                var v = geom.mesh.verts[i];
                var s = 0f;
                foreach (var h in v.GetAdjacentHalfedges(geom.halfedges)) {
                    var a = geom.Cotan(h);
                    var b = geom.Cotan(h.twin);
                    var c = (a + b) * 0.5f;
                    t.Add((i, h.next.vid, -c));
                    //t.Add((i, h.next.vert.vid, -c));
                    s += c;
                }
                t.Add((i, i, s));
            }
            var M = SparseMatrix.OfIndexed(l, l, t);
            var C = SparseMatrix.CreateDiagonal(l, l, 1e-8d);
            return M + C;
        }


        public void Integrate(double h){
            flowMtx = GenFlowMtx(h);
            var f0 = new DenseMatrix(l, 3);
            foreach(var v in geom.mesh.verts){
                var p = v.pos;
                f0.SetRow(v.vid, new double[3] { p.x, p.y, p.z });
            }
            Debug.Log(f0);
            var lu = flowMtx.LU();
            var fh = lu.Solve(f0);
            //Debug.Log(fh);

            for (var i = 0; i < l; i++) {
                var r = fh.Row(i);
                var p = new Vector3((float)r[0], (float)r[1], (float)r[2]);
                //Debug.Log(p.ToString("F6"));
                geom.mesh.verts[i].pos = new Vector3((float)r[0], (float)r[1], (float)r[2]);
            }
        }

    }
}
