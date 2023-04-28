using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Random = UnityEngine.Random;

namespace VectorField.Demo {
    using C = System.Numerics.Complex;
    using V = Vector<System.Numerics.Complex>;

    public class VectorHeatMethodViewer : MonoBehaviour {
        [SerializeField] protected Gradient colScheme;

        void Start() {
            var c = GetComponent<GeomContainer>();
            var g = c.geom;
            var srcVectors = V.Build.Dense(g.nVerts);
            var srcScalars = new List<(int vid, double value)>();
            //var i0 = Random.Range(0, g.nVerts);
            //var i1 = Random.Range(0, g.nVerts);
            var i0 = Mathf.FloorToInt(g.nVerts * 1.0f / 5);
            var i1 = Mathf.FloorToInt(g.nVerts * 4.0f / 5);
            srcVectors[i0] = new C(0, 1);
            srcVectors[i1] = new C(1, 0);
            srcScalars.Add((i0, 3));
            srcScalars.Add((i1, 1));
            c.PutSingularityPoint(i0);
            c.PutSingularityPoint(i1);

            var conn = VectorHeatMethod.ComputeVectorHeatFlow(g, srcVectors);
            var mags = VectorHeatMethod.ComputeExtendedScalar(g, srcScalars);
            var fild = VectorHeatMethod.ComputeVertVectorField(g, conn, mags);
            c.BuildVertArrowBuffer(fild);
            
            // Show ExtendScalar
            var max  = mags.Max(v => v);
            var min  = mags.Min(v => v);
            var cols = new Color[g.nVerts];
            for(var i = 0; i < g.nVerts; i++)
                cols[i] = colScheme.Evaluate((float)((mags[i] - min) / (max - min)));
            c.vertexColors = cols;
            c.showFaceArrow = false;
            c.showVertArrow = true;
            c.showFaceRibbon = false;
            c.surfMode = GeomContainer.SurfMode.vertexColorBase;
        }
    }
}