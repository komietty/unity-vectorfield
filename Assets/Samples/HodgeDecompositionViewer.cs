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
        HodgeDecomposition hd;
        HamonicBasis hb;
        HomologyGenerator hm;
        List<V> bs;
        GeomContainer container;
        bool flag;

        void OnValidate(){
            if (flag) SwitchFlow();
        }
        
        void Start() {
            container = GetComponent<GeomContainer>();
            var g = container.geom;
            hd = new HodgeDecomposition(g);
            hb = new HamonicBasis(g);
            hm = new HomologyGenerator(g);
            bs = hb.Compute(hd, hm.BuildGenerators());
            var (omega, sids, vids) = TangentField.GenRandomOneForm(g);
            random   = omega;
            exact    = hd.Exact(omega);
            coexact  = hd.CoExact(omega);
            harmonic = hd.Harmonic(omega, exact, coexact);
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
                    var f = hamonicBasisNum < bs.Count;
                    v = f ? bs[hamonicBasisNum] : harmonic; break;
            }

            var flw = TangentField.InterpolateWhitney(v, container.geom);
            container.BuildFaceArrowBuffer(flw);
            container.BuildRibbonBuffer(flw, colScheme);
        }
    }
}
