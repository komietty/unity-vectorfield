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
            var v = new VectorHeatMethod(g);
            var s = new C[g.nVerts];
            var sources = new List<(int vid, double value)>();
            var i0 = Random.Range(0, g.nVerts);
            var i1 = Random.Range(0, g.nVerts);
            s[i0] = new C(0, 1);
            s[i1] = new C(1 , 0);
            sources.Add((i0, 3));
            sources.Add((i1, 1));
            c.PutSingularityPoint(i0);
            c.PutSingularityPoint(i1);

            var conn  = v.ComputeVectorHeatFlow(V.Build.DenseOfArray(s));
            var mags  = v.ExtendScalar(sources);
            var field = v.GenField(conn, mags);
            c.BuildVertArrowBuffer(field);
            
            // Show ExtendScalar
            var max = mags.Max(v => v);
            var min = mags.Min(v => v);
            var cols = new Color[g.nVerts];
            for(var i = 0; i < g.nVerts; i++) cols[i] = colScheme.Evaluate((float)((mags[i] - min) / (max - min)));
            c.vertexColors = cols;

            c.showFaceArrow = false;
            c.showVertArrow = true;
            c.showFaceRibbon = false;
            c.surfMode = GeomContainer.SurfMode.vertexColorBase;
        }
    }
}