using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorField.Demo {
    public class ParameterizationViewer : MonoBehaviour {
        void Start() {
            var C = GetComponent<GeomContainer>();
            var G = C.geom;
            var uv = Parameterization.SpectralConformal(G);
            C.mesh.SetUVs(0, uv.Select(c => new Vector2((float)c.Real, (float)c.Imaginary)).ToList());
        }
    }
}
