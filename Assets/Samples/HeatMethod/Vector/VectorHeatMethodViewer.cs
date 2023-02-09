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
            //var m = GenMesh();
            //var mesh = HeComp.Weld(m);
            //filt.sharedMesh = mesh;
            geom = new HeGeom(mesh, transform);
            var vhmd = new VectorHeatMethod(geom);
            var bundle = new VectorBundle(geom);

            var s = new C[geom.nVerts];
            s[0] = new C(1, 0);

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

        Mesh GenMesh(){
            var m = new Mesh();
            m.SetVertices(new Vector3[]{
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0.5f, Mathf.Sqrt(0.75f), 0),
            });
            m.SetIndices(new int[] { 0, 1, 2 }, MeshTopology.Triangles, 0);
            m.RecalculateNormals();
            return m;
        }
    }
}