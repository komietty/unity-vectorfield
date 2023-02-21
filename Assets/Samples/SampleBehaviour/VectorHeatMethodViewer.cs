using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace VectorField.Demo {
    using C = System.Numerics.Complex;
    using V = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;

    public class VectorHeatMethodViewer : MonoBehaviour {
        protected GraphicsBuffer vertTangentArrows;
        [SerializeField] protected Material vectorSpaceMat;
        [SerializeField] protected Gradient colScheme;
        protected HeGeom geom;

        void Start() {
            var container = GetComponent<GeomContainer>();
            geom = container.geom;
            var vhmd = new VectorHeatMethod(geom);

            var s = new C[geom.nVerts];
            
            var sources = new List<(int vid, double value)>();
            var i0 = 0;
            var i1 = 2;
            s[i0] = new C(1 / math.sqrt(2), 1 / math.sqrt(2));
            s[i1] = new C(1 / math.sqrt(2), 1 / math.sqrt(2));
            var g1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //var g2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g1.transform.position = geom.Pos[i0];
            //g2.transform.position = geom.Pos[i1];
            g1.transform.localScale *= 0.01f;
            //g2.transform.localScale *= 0.01f;
            sources.Add((i0, 1));
            //sources.Add((i1, 1));

            var connection = vhmd.ComputeVectorHeatFlow(V.Build.DenseOfArray(s));
            var magnitude  = vhmd.ExtendScaler(sources);
            var field = vhmd.GenField(connection, magnitude);
            vertTangentArrows = VectorFieldUtility.GenVertTangentArrowsBuffer(geom, field);
        }
    
        void OnRenderObject() {
            vectorSpaceMat.SetBuffer("_Lines",vertTangentArrows);
            vectorSpaceMat.SetPass(1);
            Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nVerts * 6);
        }

        void OnDestroy() {
            vertTangentArrows?.Dispose();
        }
    }
}