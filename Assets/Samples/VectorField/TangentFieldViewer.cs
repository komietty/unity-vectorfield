using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using System.Linq;

namespace ddg {
    using V = Vector<double>;

    public class TangentFieldViewer : TangentBundle {
        public enum Field { Random, Exact, CoExact, Harmonic }
        [SerializeField] protected Field field;
        [SerializeField] protected GameObject point;
        [SerializeField] protected ComputeShader cs;
        [SerializeField, Range(0, 10)] float potentialColor;
        [SerializeField] Color baseColor;
        [SerializeField] Color scalarColor;
        [SerializeField] Color vectorColor;
        protected bool flag = false;
        protected V random;
        protected V exact;
        protected V coexact;
        protected V harmonic;
        protected GraphicsBuffer scalarPots;
        protected GraphicsBuffer vectorPots;
        protected GraphicsBuffer distances;
        protected GameObject sPotGo;
        protected GameObject vPotGo;

        void OnValidate(){
            if (!flag) return;
            switch (field) {
                case Field.Random:   UpdateTng(random);   break;
                case Field.Exact:    UpdateTng(exact);    break;
                case Field.CoExact:  UpdateTng(coexact);  break;
                case Field.Harmonic: UpdateTng(harmonic); break;
            }
        }

        protected override void Start() {
            base.Start();
            var h = new HodgeDecomposition(geom);
            var (omega, sids, vids) = TangentField.GenRandomOneForm(geom);
            random   = omega;
            exact    = h.Exact(omega);
            coexact  = h.CoExact(omega);
            harmonic = h.Harmonic(omega, exact, coexact);
            UpdateTng(random);
            
            var t = GraphicsBuffer.Target.Structured;
            scalarPots = new GraphicsBuffer(t, sids.Length,  sizeof(float) * 3);
            vectorPots = new GraphicsBuffer(t, vids.Length,  sizeof(float) * 3);
            distances  = new GraphicsBuffer(t, tngBuf.count, sizeof(float) * 2);
            sPotGo = new GameObject();
            vPotGo = new GameObject();
            sPotGo.transform.SetParent(transform);
            vPotGo.transform.SetParent(transform);
            var sPots = sids.Select(i => geom.Pos[i]).ToArray();
            var vPots = vids.Select(i => geom.Pos[i]).ToArray();
            var q = Quaternion.identity;
            foreach (var p in sPots) { var g = GameObject.Instantiate(point, p, q); g.transform.SetParent(sPotGo.transform); }
            foreach (var p in vPots) { var g = GameObject.Instantiate(point, p, q); g.transform.SetParent(vPotGo.transform); }
            scalarPots.SetData(sPots);
            vectorPots.SetData(vPots);

            var k = cs.FindKernel("SetDistance");
            int s = Mathf.CeilToInt(tngBuf.count / 64.0f);
            cs.SetInt("ScalarPotentialNum", sids.Length);
            cs.SetInt("VectorPotentialNum", vids.Length);
            cs.SetFloat("MeanEdgeLength", geom.MeanEdgeLength());
            cs.SetBuffer(k, "TangentArrowBuf", tngBuf);
            cs.SetBuffer(k, "ScalarPotentialPosBuf", scalarPots);
            cs.SetBuffer(k, "VectorPotentialPosBuf", vectorPots);
            cs.SetBuffer(k, "Distances", distances);
            cs.Dispatch(k, s, 1, 1);
            tngtMat.SetBuffer("_Dist", distances);
            flag = true;
        }

        void Update() {
            switch (field) {
                case Field.Random:   sPotGo.SetActive(true);  vPotGo.SetActive(true);  break;
                case Field.Exact:    sPotGo.SetActive(true);  vPotGo.SetActive(false); break;
                case Field.CoExact:  sPotGo.SetActive(false); vPotGo.SetActive(true);  break;
                case Field.Harmonic: sPotGo.SetActive(false); vPotGo.SetActive(false); break;
            }
            tngtMat.SetInt("_M", (int)field);
            tngtMat.SetFloat("_T", Time.time);
            tngtMat.SetFloat("_C", potentialColor);
            tngtMat.SetColor("_C0", baseColor);
            tngtMat.SetColor("_C1", scalarColor);
            tngtMat.SetColor("_C2", vectorColor);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            distances.Dispose();
            scalarPots.Dispose();
            vectorPots.Dispose();
        }
    }
}