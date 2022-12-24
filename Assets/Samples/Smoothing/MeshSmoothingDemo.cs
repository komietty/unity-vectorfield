using System.Collections;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace ddg {
    using S = SparseMatrix;

    public class MeshSmoothingDemo : TangentBundle {
        [SerializeField] protected bool native = true;
        [SerializeField, Range(0.001f, 0.1f)] protected float delta = 0.001f;

        protected override void Start() {
            base.Start();
            StartCoroutine(Smooth());
        }

        IEnumerator Smooth() {
            var flow = new MeanCurvatureFlow(geom);
            while(true){
                yield return new WaitForSeconds(0.5f);
                if(native) flow.Integrate(delta);
                else       flow.IntegrateCSharp(delta);
                mesh.SetVertices(geom.Pos.ToArray());
                mesh.RecalculateNormals();
            }
        }
    }

    public class MeanCurvatureFlow {
        HeGeom g;
        SparseMatrix L;
        
        public MeanCurvatureFlow(HeGeom g) {
            this.g = g;
            L = Operator.Laplace(g);
        }

        public SparseMatrix GenFlowMtx(double h){
            var I = SparseMatrix.CreateIdentity(g.nVerts);
            var M = S.OfDiagonalVector(Operator.Mass(g).Diagonal().Map(v => 1 / v));
            return I + (M * L) * h;
        }

        public void Integrate(double h){
            var fm = GenFlowMtx(h);
            var data = SparseCompressedRowMatrixStorage<double>.OfMatrix(fm.Storage);
            var iter = data.EnumerateNonZeroIndexed();
            var outs = new Vector3[g.nVerts];
            //var trps = iter.Select(i => new Triplet(i.Item3, i.Item1, i.Item2)).ToArray();
            //Solver.DecompAndSolveLUVec(iter.Count(), geom.nVerts, trps, geom.pos, outs);
            g.pos = outs;
        }

        public void IntegrateCSharp(double h){
            var fm = GenFlowMtx(h);
            var f0 = new DenseMatrix(g.nVerts, 3);
            foreach (var v in g.Verts) {
                var p = g.Pos[v.vid];
                f0.SetRow(v.vid, new double[3] { p.x, p.y, p.z });
            }
            var lu = fm.LU();
            var fh = lu.Solve(f0);
            for (var i = 0; i < g.nVerts; i++) {
                var r = fh.Row(i);
                var p = new Vector3((float)r[0], (float)r[1], (float)r[2]);
                g.Pos[i] = p;
            }
        }
    }
}
