using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorField {
    public class KillingVectorFieldViewer : MonoBehaviour {
        
        void Start() {
            var c = GetComponent<GeomContainer>();
            var G = c.geom;
            var k = new KillingVectorField(G);
            var f = k.GenVectorField(k.omega);

            c.BuildFaceArrowBuffer(f);
            c.surfMode = GeomContainer.SurfMode.blackBase;
            c.showFaceArrow = true;
            c.showVertArrow = false;
        }

        void Update() {
        
        }
    }
}
