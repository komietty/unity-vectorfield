using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VectorField {
    using V = MathNet.Numerics.LinearAlgebra.Vector<double>;
    
    public class TrivialConnectionViewer : MonoBehaviour {
        [SerializeField] protected Gradient colScheme;
        [SerializeField] protected List<float> singularities;
        [SerializeField] protected Material mat;
        private List<List<HalfEdge>> generators;
        private HeGeom G;
        private HomologyGenerator hg;

        void Start() {
            var c = GetComponent<GeomContainer>();
            G = c.geom;
            hg = new HomologyGenerator(G);
            generators = hg.BuildGenerators();
            var b = new float[G.nVerts];
            foreach (var s in singularities) {
                var i = Random.Range(0, G.nVerts); 
                b[i] = s;
                if (s != 0) c.PutSingularityPoint(i);
            }
            
            //var t = new TrivialConnection(g);
            //var p = t.ComputeConnections(b);
            
            var t = new TrivialConnectionAlt(G);
            var p = t.ComputeConnections(b);

            var f = t.GetFaceVectorFromConnection(p);
            c.BuildFaceArrowBuffer(f);
            //c.BuildRibbonBuffer(f, colScheme);
        }

        void OnRenderObject() {
            if (generators == null) return;
            GL.PushMatrix();
            mat.SetPass(0);
            GL.Begin(GL.LINES);
            foreach (var g in generators) {
                foreach (var h in g)
                {
                    var c1 = G.Centroid(h.face);
                    var c2 = G.Centroid(h.twin.face);
                    var p1 = G.Pos[h.vid];
                    var p2 = G.Pos[h.twin.vid];
                    var m = (p1 + p2) * 0.5f; 
                    GL.Vertex(c1);
                    GL.Vertex(m);
                    GL.Vertex(m);
                    GL.Vertex(c2);
                }
            }
            GL.End();
            GL.PopMatrix();
        }
    }
}
