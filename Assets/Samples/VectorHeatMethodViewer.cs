using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Random = UnityEngine.Random;

namespace VectorField.Demo {
    using C = System.Numerics.Complex;
    using V = Vector<System.Numerics.Complex>;

    public class VectorHeatMethodViewer : MonoBehaviour {
        GeomContainer container;
        [SerializeField] protected Gradient colScheme;

        void Start() {
            container = GetComponent<GeomContainer>();
            var geom = container.geom;
            var vhmd = new VectorHeatMethod(geom);
            var s = new C[geom.nVerts];
            var sources = new List<(int vid, double value)>();
            //var i0 = Random.Range(0, geom.nVerts);
            //var i1 = Random.Range(0, geom.nVerts);
            var i0 = 0;
            var i1 = geom.nVerts / 2;
            //var i2 = Random.Range(0, geom.nVerts);
            //s[i0] = new C(1 / math.sqrt(2),  1 / math.sqrt(2));
            s[i0] = new C(0, 1);
            s[i1] = new C(1 , 0);
            //s[i2] = new C(0 , 1);
            sources.Add((i0, 1));
            sources.Add((i1, 3));
            //sources.Add((i2, 5));
            container.PutSingularityPoint(i0);
            container.PutSingularityPoint(i1);
            //container.PutSingularityPoint(i2);

            var connection = vhmd.ComputeVectorHeatFlow(V.Build.DenseOfArray(s));
            var magnitude  = vhmd.ExtendScaler(sources);
            var field = vhmd.GenField(connection, magnitude);
            container.BuildVertArrowBuffer(field);
            //container.BuildRibbonBuffer(field);
            ShowExtendedScalar(magnitude);
        }

        void ShowExtendedScalar(Vector<double> vals) {
            var g = container.geom;
            var max = vals.Max(v => v);
            var min = vals.Min(v => v);
            var cols = new Color[g.nVerts];
            for(var i = 0; i < g.nVerts; i++) cols[i] = colScheme.Evaluate((float)((vals[i] - min) / (max - min)));
            container.vertexColors = cols;
        }
    }
}