using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace VectorField {
    using V = Vector<double>;

    public class HodgeDecompositionViewer : MonoBehaviour {
        public enum Field { Random, Exact, CoExact, Harmonic }
        [SerializeField] protected Field field;
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
            var G  = container.geom;
            var h = new HodgeDecomposition(G);
            var g = new HomologyGenerator(G).BuildGenerators();
            random   = ScalarPoissonProblem.ComputeRandomOneForm(G);
            bases    = g.Select(g => h.ComputeHarmonicBasis(g)).ToList();
            exact    = h.ComputeExact(random);
            coexact  = h.ComputeCoExact(random);
            harmonic = h.ComputeHarmonic(random, exact, coexact);
            SwitchFlow();
            flag = true;
            container.surfMode = GeomContainer.SurfMode.blackBase;
            container.showVertArrow = false;
            container.showFaceArrow = true;
        }

        void SwitchFlow() {
            V v;
            switch (field) {
                default: throw new Exception();
                case Field.Random:  v = random;  break;
                case Field.Exact:   v = exact;   break;
                case Field.CoExact: v = coexact; break;
                case Field.Harmonic:
                    v = hamonicBasisNum < bases.Count ?
                        bases[hamonicBasisNum] : harmonic; break;
            }

            var f = DEC.InterpolateWhitney(v, container.geom);
            container.BuildFaceArrowBuffer(f);
            container.BuildRibbonBuffer(f);
        }
    }
}
