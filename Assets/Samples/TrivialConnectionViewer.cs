using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using Random = UnityEngine.Random;

namespace VectorField {
    using V = Vector<double>;
    
    public class TrivialConnectionViewer : MonoBehaviour {
        [SerializeField] protected Gradient colScheme;
        [SerializeField] protected List<float> singularities;
        [SerializeField] protected int mode;
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

            switch (mode) {
                case 0: modeA(g, sings); break;
                case 1: modeB(g, sings); break;
                case 2: modeC(g, sings); break;
                case 3: modeD(g, sings); break;
            }
        }
        
        void modeA(HeGeom g, float[] singularities) {
            var t = new TrivialConnection(g);
            var phi = t.ComputeCoExactComponent(singularities);
            var flw = t.GetFaceVectorFromConnection(phi);
            container.BuildFaceArrowBuffer(flw);
            container.BuildRibbonBuffer(flw, colScheme);
        }
        
        void modeB(HeGeom g, float[] singularities) {
            var t = new TrivialConnection(g);
            var phi = t.ComputeConnections(singularities);
            var flw = t.GetFaceVectorFromConnection(phi);
            container.BuildFaceArrowBuffer(flw);
            container.BuildRibbonBuffer(flw, colScheme);
        }
        
        void modeC(HeGeom g, float[] singularities) {
            var t = new TrivialConnectionAlt(g);
            var phi = t.ComputeCoExactComponent(singularities);
            var flw = t.GetFaceVectorFromConnection(phi);
            container.BuildFaceArrowBuffer(flw);
            container.BuildRibbonBuffer(flw, colScheme);
        }
        
        void modeD(HeGeom g, float[] singularities) {
            var t = new TrivialConnectionAlt(g);
            var phi = t.ComputeConnections(singularities);
            var flw = t.GetFaceVectorFromConnection(phi);
            container.BuildFaceArrowBuffer(flw);
            container.BuildRibbonBuffer(flw, colScheme);
        }
    }
}
