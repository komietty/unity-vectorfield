using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace VectorField.Demo {
    using C = MathNet.Numerics.Complex32;
    using V = MathNet.Numerics.LinearAlgebra.Vector<MathNet.Numerics.Complex32>;

    public class VectorHeatMethodViewer : MonoBehaviour {
        protected GraphicsBuffer vertTangentArrows;
        [SerializeField] protected Material vectorSpaceMat;
        protected HeGeom geom;

        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var mesh = HeComp.Weld(filt.sharedMesh); 
            filt.sharedMesh = mesh;
            geom = new HeGeom(mesh, transform);
            var vhmd = new VectorHeatMethod(geom);
            var bundle = new VectorBundle(geom);

            var s = new C[geom.nVerts];
            
            var sources = new List<(int vid, float value)>();
            var i0 = 0;
            var i1 = 300;
            s[i0] = new C(1 / math.sqrt(2), 1 / math.sqrt(2));
            s[i1] = new C(1 / math.sqrt(2), 1 / math.sqrt(2));
            var g1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var g2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g1.transform.position = geom.Pos[i0];
            g2.transform.position = geom.Pos[i1];
            g1.transform.localScale *= 0.01f;
            g2.transform.localScale *= 0.01f;
            GameObject.CreatePrimitive(PrimitiveType.Cube);
            sources.Add((i0, 3));
            sources.Add((i1, 100));

            var field = vhmd.GenField(V.Build.DenseOfArray(s), sources);
            vertTangentArrows = bundle.GenVertTangentArrows(field);
        }
    
        void OnRenderObject() {
            vectorSpaceMat.SetPass(1);
            vectorSpaceMat.SetBuffer("_Lines",vertTangentArrows);
            Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nVerts * 6);
        }

        void OnDestroy() {
            vertTangentArrows?.Dispose();
        }
    }
}