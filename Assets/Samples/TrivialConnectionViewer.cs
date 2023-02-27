using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using Random = UnityEngine.Random;

namespace VectorField {
    using V = Vector<double>;
    
    public class TrivialConnectionViewer : MonoBehaviour {
        [SerializeField] protected List<float> singularities;
        GeomContainer container;
        
        void Start() {
            container = GetComponent<GeomContainer>();
            var g = container.geom;
            var tt = new TrivialConnectionAlt(g);
            var sings = new float[g.nVerts];
            foreach (var s in singularities) {
                var i = Random.Range(0, g.nVerts); 
                sings[i] = s;
                if (s != 0) container.PutSingularityPoint(i);
            }
            var phi = tt.ComputeCoExactComponent(sings);
            var flw = tt.GetFaceVectorFromConnection(phi);
            container.BuildArrowBuffer(flw);
            //container.BuildRibbonBuffer(flw);
        }

        private void OnRenderObject() {
            container.DrawArrows();
            //container.DrawRibbons();
        }
    }
}
