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
            C.mesh.SetUVs(0, uv.Select(c => {
                var re = (float)c.Real;
                var im = (float)c.Imaginary;
                //if (re < 0) re = 1f + re;
                //if (im < 0) im = 1f + im;
                //if (re > 1) Debug.LogWarning(re); else Debug.Log(re);
                //if (im > 1) Debug.LogWarning(im); else Debug.Log(im);
                return new Vector2(re, im);
            }).ToList());
        }
    }
}
