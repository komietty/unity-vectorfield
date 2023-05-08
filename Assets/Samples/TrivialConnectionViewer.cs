using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VectorField {
    using V = MathNet.Numerics.LinearAlgebra.Vector<double>;
    
    public class TrivialConnectionViewer : MonoBehaviour {
        [SerializeField] protected Gradient colScheme;
        [SerializeField] protected List<float> singularities;
        
        void Start() {
            var c = GetComponent<GeomContainer>();
            var g = c.geom;
            var b = new float[g.nVerts];
            foreach (var s in singularities) {
                var i = Random.Range(0, g.nVerts); 
                b[i] = s;
                if (s != 0) c.PutSingularityPoint(i);
            }
            var t = new TrivialConnection(g);
            var p = t.ComputeConnections(b);
            var f = t.GetFaceVectorFromConnection(p);
            c.BuildFaceArrowBuffer(f);
            c.BuildRibbonBuffer(f, colScheme);
        }
    }
}
