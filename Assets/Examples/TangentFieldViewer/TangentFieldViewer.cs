using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine;
using System.Linq;

namespace ddg {
    public class TangentFieldViewer : TangentBundleBehaviour {
        public enum Field { Random, Exact, CoExact }
        [SerializeField] protected Field field;
        protected bool flag = false;
        protected double[] random;
        protected double[] exact;
        protected double[] coexact;

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
            random = TangentBundle.GenRandomOneForm(g);
            var m = DenseMatrix.OfColumnMajor(random.Length, 1, random.Select(v => (double)v));
            exact = h.ComputeExactComponent(m);
            coexact = h.ComputeCoExactComponent(m);
            UpdateTng(random);
        }

        void Update() {
            tngMat.SetFloat("_T", Time.time);
        }
    }
}