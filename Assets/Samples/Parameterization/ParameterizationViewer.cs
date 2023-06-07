using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

namespace VectorField.Demo {
    public class ParameterizationViewer : MonoBehaviour {
        void Start() {
            var C = GetComponent<GeomContainer>();
            var G = C.geom;
            /*
            var A = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix.OfRowMajor(4, 4, new[] {
                new Complex(0.500000005, 0), new Complex(-0.25, 0.25), new Complex(-0.25, -0.25), new Complex(0, 0),
                new Complex(-0.25, -0.25), new Complex(0.500000005, 0), new Complex(0, 0), new Complex(-0.25, 0.25), 
                new Complex(-0.25, 0.25), new Complex(0, 0), new Complex(0.500000005, 0), new Complex(-0.25, -0.25), 
                new Complex(0, 0), new Complex(-0.25, -0.25), new Complex(-0.25, 0.25), new Complex(0.500000005, 0), 
            });
            Solver.InversePowerMethod(A);
            */
            var uv = Parameterization.SpectralConformal(G);
            C.mesh.SetUVs(0, uv.Select(c => {
                var re = (float)c.Real % 1f;
                var im = (float)c.Imaginary % 1f;
                return new UnityEngine.Vector2(re, im);
            }).ToList());
        }
    }
}
