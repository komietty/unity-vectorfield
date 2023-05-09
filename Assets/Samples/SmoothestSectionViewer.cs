using UnityEngine;

namespace VectorField.Demo {
    public class SmoothestSectionViewer : MonoBehaviour {
        void Start() {
            var c = GetComponent<GeomContainer>();
            var g = c.geom;
            var s = new SmoothestSection(g);
            var f = SmoothestSection.ComputeVertVectorField(g, s.fileds);
            c.BuildVertArrowBuffer(f);
        }

    }
}
