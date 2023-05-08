using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace VectorField.Demo
{
    public class SmoothestSectionViewer : MonoBehaviour {
        void Start() {
            var c = GetComponent<GeomContainer>();
            var g = c.geom;
            var s = new SmoothestSection(g);
            c.BuildVertArrowBuffer(SmoothestSection.ComputeVertVectorField(g, s.fileds));
        }

    }
}
