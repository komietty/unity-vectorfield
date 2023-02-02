using System.Collections;
using System.Collections.Generic;
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
            var rend = GetComponentInChildren<MeshRenderer>();
            var mesh = HeComp.Weld(filt.sharedMesh);
            geom = new HeGeom(mesh, transform);
            var vhmd = new VectorHeatMethod(geom);
            var bundle = new VectorBundle(geom);

            var s = new C[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
            s[UnityEngine.Random.Range(0, geom.nVerts)] = new C(1, 0);

            var field = vhmd.GenField(V.Build.DenseOfArray(s));
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