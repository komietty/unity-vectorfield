using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using Random = UnityEngine.Random;

namespace VectorField {
    using V = Vector<double>;
    
    public class TrivialConnectionViewer : MonoBehaviour {
        [SerializeField] protected Gradient colScheme;
        [SerializeField] protected List<float> singularities;
        GeomContainer container;
        
        void Start() {
            container = GetComponent<GeomContainer>();
            var g = container.geom;
            var sings = new float[g.nVerts];
            var c = 0;
            foreach (var s in singularities) {
                var i = Random.Range(0, g.nVerts); 
                //var i = g.nVerts * c / singularities.Count; 
                sings[i] = s;
                c++;
                if (s != 0) container.PutSingularityPoint(i);
            }
            var t = new TrivialConnection(g);
            var phi = t.ComputeConnections(sings);
            var flw = t.GetFaceVectorFromConnection(phi);
            container.BuildFaceArrowBuffer(flw);
            container.BuildRibbonBuffer(flw, colScheme);
        }
    }
}
