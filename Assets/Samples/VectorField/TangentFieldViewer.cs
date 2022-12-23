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
        [SerializeField] Color baseColor;
        [SerializeField] Color scalarColor;
        [SerializeField] Color vectorColor;
        protected bool flag = false;
        protected double[] random;
        protected double[] exact;
        protected double[] coexact;
        protected GraphicsBuffer scalarPots;
        protected GraphicsBuffer vectorPots;
        protected GraphicsBuffer distances;
        protected GameObject scalarPotGo;
        protected GameObject vectorPotGo;

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
            var (omega, sids, vids) = TangentBundle.GenRandomOneForm(g);
            var m = DenseVector.OfArray(omega);
            random  = omega;
            exact   = h.Exact(m).ToArray();
            coexact = h.CoExact(m).ToArray();
            UpdateTng(random);
            
            var t = GraphicsBuffer.Target.Structured;
            scalarPots = new GraphicsBuffer(t, sids.Length,  sizeof(float) * 3);
            vectorPots = new GraphicsBuffer(t, vids.Length,  sizeof(float) * 3);
            distances  = new GraphicsBuffer(t, tngBuf.count, sizeof(float) * 2);
            scalarPotGo = new GameObject();
            vectorPotGo = new GameObject();
            scalarPotGo.transform.SetParent(transform);
            vectorPotGo.transform.SetParent(transform);
            var sPots = sids.Select(i => g.Pos[i]).ToArray();
            var vPots = vids.Select(i => g.Pos[i]).ToArray();
            var q = Quaternion.identity;
            foreach (var p in sPots) { var go = GameObject.Instantiate(point, p, q); go.transform.SetParent(scalarPotGo.transform); }
            foreach (var p in vPots) { var go = GameObject.Instantiate(point, p, q); go.transform.SetParent(vectorPotGo.transform); }
            scalarPots.SetData(sPots);
            vectorPots.SetData(vPots);

            var k = cs.FindKernel("SetDistance");
            int s = Mathf.CeilToInt(tngBuf.count / 64.0f);
            cs.SetInt("ScalarPotentialNum", sids.Length);
            cs.SetInt("VectorPotentialNum", vids.Length);
            cs.SetFloat("MeanEdgeLength", g.MeanEdgeLength());
            cs.SetBuffer(k, "TangentArrowBuf", tngBuf);
            cs.SetBuffer(k, "ScalarPotentialPosBuf", scalarPots);
            cs.SetBuffer(k, "VectorPotentialPosBuf", vectorPots);
            cs.SetBuffer(k, "Distances", distances);
            cs.Dispatch(k, s, 1, 1);
            tngMat.SetBuffer("_Dist", distances);
            flag = true;
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                field = (Field)(((int)field + 1) % 3);
                switch (field) {
                    case Field.Random: UpdateTng(random); break;
                    case Field.Exact: UpdateTng(exact); break;
                    case Field.CoExact: UpdateTng(coexact); break;
                }
            }
            switch (field) {
                case Field.Random:  scalarPotGo.SetActive(true);  vectorPotGo.SetActive(true);  break;
                case Field.Exact:   scalarPotGo.SetActive(true);  vectorPotGo.SetActive(false); break;
                case Field.CoExact: scalarPotGo.SetActive(false); vectorPotGo.SetActive(true);  break;
            }
            tngMat.SetInt("_M", (int)field);
            tngMat.SetFloat("_T", Time.time);
            tngMat.SetFloat("_C", potentialColor);
            tngMat.SetColor("_C0", baseColor);
            tngMat.SetColor("_C1", scalarColor);
            tngMat.SetColor("_C2", vectorColor);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            distances.Dispose();
            scalarPots.Dispose();
            vectorPots.Dispose();
        }
    }
}