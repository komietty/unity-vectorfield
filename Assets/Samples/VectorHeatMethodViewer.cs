using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Numerics;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;

namespace VectorField.Demo {
    public class VectorHeatMethodViewer : MonoBehaviour {
        [SerializeField] protected Gradient colScheme;

        void Start() {
            var C = GetComponent<GeomContainer>();
            var G = C.geom;
            var srcVectors = Vector.Build.Dense(G.nVerts);
            var srcScalars = new List<(int vid, double value)>();
            var i0 = Random.Range(0, G.nVerts);
            var i1 = Random.Range(0, G.nVerts);
            srcVectors[i0] = new Complex(0, 1);
            srcVectors[i1] = new Complex(1, 0);
            srcScalars.Add((i0, 3));
            srcScalars.Add((i1, 1));
            C.PutSingularityPoint(i0);
            C.PutSingularityPoint(i1);

            // Show vector field
            var heat = new VectorHeatMethod(G);
            var conn = heat.ComputeVectorHeatFlow(srcVectors);
            var mags = heat.ComputeExtendedScalar(srcScalars);
            var X    = heat.ComputeVectorField(conn, mags);
            C.BuildVertArrowBuffer(X);
            
            // Show extend scalar
            var max  = mags.Max(v => v);
            var min  = mags.Min(v => v);
            var cols = new Color[G.nVerts];
            for(var i = 0; i < G.nVerts; i++)
                cols[i] = colScheme.Evaluate((float)((mags[i] - min) / (max - min)));
            C.vertexColors = cols;
            C.showFaceArrow = false;
            C.showVertArrow = true;
            C.showFaceRibbon = false;
            C.surfMode = GeomContainer.SurfMode.vertexColorBase;
        }
    }
}