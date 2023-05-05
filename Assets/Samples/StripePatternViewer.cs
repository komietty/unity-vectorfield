using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace VectorField.Demo {
    using RVector = MathNet.Numerics.LinearAlgebra.Vector<double>;
    using CVector = MathNet.Numerics.LinearAlgebra.Vector<Complex>;
    
    public class StripePatternViewer : MonoBehaviour {
        void Start() {
            var c = GetComponent<GeomContainer>();
            var G = c.geom;
            var srcVectors = CVector.Build.Dense(G.nVerts);
            var srcScalars = new List<(int vid, double value)>();
            var i0 = 0;
            srcVectors[i0] = new Complex(0, 1);
            srcScalars.Add((i0, 3));
            c.PutSingularityPoint(i0);

            var conn = VectorHeatMethod.ComputeVectorHeatFlow(G, srcVectors);
            var mags = VectorHeatMethod.ComputeExtendedScalar(G, srcScalars);
            var rX = VectorHeatMethod.ComputeVertVectorField(G, conn, mags);
            var cX = VectorHeatMethod.ComputeVertVectorFieldComplex(G, conn, mags);
            
            var stripePattern = new StripePattern(G, cX);
            
            c.BuildVertArrowBuffer(rX);
        }
    }
}
