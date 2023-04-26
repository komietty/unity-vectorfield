using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorField {
    
    public class KillingVectorFieldViewer : MonoBehaviour {
        
        void Start() {
            var c = GetComponent<GeomContainer>();
            var g = c.geom;
            var k = new KillingVectorField(g);
            c.BuildFaceArrowBuffer(k.GenVectorField());
        }
    }
}
