using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine;
using System.Linq;

namespace ddg {
    public class TangentFieldViewer : TangentBundleBehaviour {
        public enum Field { Random, Exact, CoExact }
        [SerializeField] protected Field field;
        [SerializeField] protected GameObject point;
        [SerializeField] protected ComputeShader cs;
        [SerializeField, Range(0, 10)] float potentialColor;
        protected bool flag = false;
        protected double[] random;
        protected double[] exact;
        protected double[] coexact;
        protected GraphicsBuffer scalarPotentialPos;
        protected GraphicsBuffer vectorPotentialPos;
        protected GraphicsBuffer distances;

        void OnValidate(){
            if (!flag) return;
            switch (field) {
                case Field.Random:  UpdateTng(random);  break;
                case Field.Exact:   UpdateTng(exact);   break;
                case Field.CoExact: UpdateTng(coexact); break;
            }
        }
    
        protected override void Start() {
            base.Start();
            var g = bundle.Geom;
            var h = new HodgeDecomposition(g);
            flag = true;
            var (omega, exactIds, coexactIds) = TangentBundle.GenRandomOneForm(g);
            random = omega;
            var m = DenseMatrix.OfColumnMajor(random.Length, 1, random.Select(v => (double)v));
            exact = h.ComputeExactComponent(m);
            coexact = h.ComputeCoExactComponent(m);
            UpdateTng(random);
            
            var t = GraphicsBuffer.Target.Structured;
            scalarPotentialPos = new GraphicsBuffer(t, exactIds.Length,   sizeof(float) * 3);
            vectorPotentialPos = new GraphicsBuffer(t, coexactIds.Length, sizeof(float) * 3);
            distances          = new GraphicsBuffer(t, tngBuf.count,     sizeof(float) * 2);
            var scalarPotPos =   exactIds.Select(i => g.Pos[i]).ToArray();
            var vectorPotPos = coexactIds.Select(i => g.Pos[i]).ToArray();
            foreach (var p in scalarPotPos) { var go = GameObject.Instantiate(point, p, Quaternion.identity); go.transform.SetParent(this.transform); }
            foreach (var p in vectorPotPos) { var go = GameObject.Instantiate(point, p, Quaternion.identity); go.transform.SetParent(this.transform); }
            scalarPotentialPos.SetData(scalarPotPos);
            vectorPotentialPos.SetData(vectorPotPos);

            var k = cs.FindKernel("SetDistance");
            int s = Mathf.CeilToInt(tngBuf.count / 64.0f);
            cs.SetInt("ScalarPotentialNum",   exactIds.Length);
            cs.SetInt("VectorPotentialNum", coexactIds.Length);
            cs.SetFloat("MeanEdgeLength", g.MeanEdgeLength());
            cs.SetBuffer(k, "TangentArrowBuf", tngBuf);
            cs.SetBuffer(k, "ScalarPotentialPosBuf", scalarPotentialPos);
            cs.SetBuffer(k, "VectorPotentialPosBuf", vectorPotentialPos);
            cs.SetBuffer(k, "Distances", distances);
            cs.Dispatch(k, s, 1, 1);
            tngMat.SetBuffer("_Dist", distances);
        }

        void Update() {
            tngMat.SetInt("_M", (int)field);
            tngMat.SetFloat("_T", Time.time);
            tngMat.SetFloat("_C", potentialColor);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            distances.Dispose();
            scalarPotentialPos.Dispose();
            vectorPotentialPos.Dispose();
        }
    }
}