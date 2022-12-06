using System.Collections;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using System.Linq;
using System;

namespace ddg {
    public class MeshSmoothingDemo : TangentBundleBehaviour {
        [SerializeField] protected bool native = true;
        [SerializeField, Range(0.001f, 0.1f)] protected float delta = 0.001f;

        protected override void Start() {
            base.Start();
            StartCoroutine(Smooth());
        }

        IEnumerator Smooth() {
            var flow = new MeanCurvatureFlow(bundle.Geom);
            while(true){
                yield return new WaitForSeconds(0.5f);
                if(native) flow.IntegrateNative(delta);
                else       flow.IntegrateCSharp(delta);
                mesh.SetVertices(bundle.Geom.Pos.ToArray());
                mesh.RecalculateNormals();
            }
        }
    }

    public class MeanCurvatureFlow {
        HeGeom geom;
        SparseMatrix L;
        
        public MeanCurvatureFlow(HeGeom g) {
            this.geom = g;
            L = Operator.Laplace(g);
        }

        public SparseMatrix GenFlowMtx(double h){
            var I = SparseMatrix.CreateIdentity(geom.nVerts);
            var M = Operator.MassInv(geom);
            return I + (M * L) * h;
        }

        public void IntegrateNative(double h){
            var fm = GenFlowMtx(h);
            var data = SparseCompressedRowMatrixStorage<double>.OfMatrix(fm.Storage);
            var iter = data.EnumerateNonZeroIndexed();
            var outs = new Vector3[geom.nVerts];
            var trps = iter.Select(i => new Triplet(i.Item3, i.Item1, i.Item2)).ToArray();
            Solver.DecompAndSolveLUVec(iter.Count(), geom.nVerts, trps, geom.pos, outs);
            geom.pos = outs;
        }

        public void IntegrateCSharp(double h){
            var fm = GenFlowMtx(h);
            var f0 = new DenseMatrix(geom.nVerts, 3);
            foreach (var v in geom.Verts) {
                var p = geom.Pos[v.vid];
                f0.SetRow(v.vid, new double[3] { p.x, p.y, p.z });
            }
            var lu = fm.LU();
            var fh = lu.Solve(f0);
            for (var i = 0; i < geom.nVerts; i++) {
                var r = fh.Row(i);
                var p = new Vector3((float)r[0], (float)r[1], (float)r[2]);
                geom.Pos[i] = p;
            }
        }
    }
}
