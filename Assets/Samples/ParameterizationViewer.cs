using System.Linq;
using UnityEngine;

namespace VectorField.Demo {
    public class ParameterizationViewer : MonoBehaviour {
        void Start() {
            var C = GetComponent<GeomContainer>();
            var uv = Parameterization.SpectralConformal(C.geom);
            C.mesh.SetUVs(0, uv.Select(c => new Vector2((float)c.Real, (float)c.Imaginary)).ToList()); 
        }
    }
}
