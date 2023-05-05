using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorField.Demo {
    public class StripePatternViewer : MonoBehaviour {
        void Start() {
            var c = GetComponent<GeomContainer>();
            var G = c.geom;
            var v = TangentField.GenRandomOneForm(G).oneForm;
            
            
            
            var f = ExteriorDerivatives.InterpolateWhitney(v, G);
            c.BuildFaceArrowBuffer(f);
        }
    }
}
