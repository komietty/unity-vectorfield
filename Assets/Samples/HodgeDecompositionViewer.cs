using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace VectorField {
    using V = Vector<double>;

    public class HodgeDecompositionViewer : MonoBehaviour {
        public enum Field { Random, Exact, CoExact, Harmonic }
        [SerializeField] protected Field field;
        [SerializeField] protected Gradient colScheme;
        [SerializeField, Range(0, 10)] int hamonicBasisNum;
        V random, exact, coexact, harmonic;
        GeomContainer container;
        List<V> bases;
        bool flag;

        void OnValidate(){
            if (flag) SwitchFlow();
        }
        
        void Start() {
            container = GetComponent<GeomContainer>();
            var g  = container.geom;
            var hd = new HodgeDecomposition(g);
            var hm = new HomologyGenerator(g);
            random   = TangentField.GenRandomOneForm(g).oneForm;
            bases    = hd.ComputeHamonicBasis(hm.BuildGenerators());
            exact    = hd.ComputeExact(random);
            coexact  = hd.ComputeCoExact(random);
            harmonic = hd.ComputeHarmonic(random, exact, coexact);
            SwitchFlow();
            flag = true;
        }

        void SwitchFlow() {
            V v;
            switch (field) {
                default: throw new Exception();
                case Field.Random:  v = random;  break;
                case Field.Exact:   v = exact;   break;
                case Field.CoExact: v = coexact; break;
                case Field.Harmonic:
                    var f = hamonicBasisNum < bases.Count;
                    v = f ? bases[hamonicBasisNum] : harmonic; break;
            }

            var flow = ExteriorDerivatives.InterpolateWhitney(v, container.geom);
            container.BuildFaceArrowBuffer(flow);
            //container.BuildRibbonBuffer(flow, colScheme);
        }
    }
}
