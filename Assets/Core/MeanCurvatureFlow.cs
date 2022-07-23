using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ddg {
    public class MeanCurvatureFlow {
        public enum Type { Simple, Modified }
        Type type = Type.Simple;
        HalfEdgeGeom geom;
        SparseMatrix L;
        int l => geom.mesh.verts.Length;
        
        public MeanCurvatureFlow(HalfEdgeGeom geom, Type type) {
            this.geom = geom;
            this.type = type;
            L = GenLaplaceMtx();
        }

        public SparseMatrix GenFlowMtx(double h){
            if (type == Type.Simple) L = GenLaplaceMtx();
            var I = SparseMatrix.CreateIdentity(l);
            var M = GenInversedMassMtx();
            return I + (M * L) * h;
        }

        public SparseMatrix GenInversedMassMtx(){
            var a = new double[l];
            for (var i = 0; i < l; i++) { a[i] = 1 / geom.BarycentricDualArea(geom.mesh.verts[i]); }
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
            var f0 = new DenseMatrix(l, 3);
            foreach(var v in geom.mesh.verts){
                var p = v.pos;
                f0.SetRow(v.vid, new double[3] { p.x, p.y, p.z });
            }
            var lu = fm.LU();
            var fh = lu.Solve(f0);

            for (var i = 0; i < l; i++) {
                var r = fh.Row(i);
                var p = new Vector3((float)r[0], (float)r[1], (float)r[2]);
                geom.mesh.verts[i].pos = p;
            }
        }

    }
}
