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
            //var mesh = GenMesh(HeComp.Weld(filt.sharedMesh));
            filt.sharedMesh = mesh;
            geom = new HeGeom(mesh, transform);
            var vhmd = new VectorHeatMethod(geom);
            var bundle = new VectorBundle(geom);

            var s = new C[geom.nVerts];
            s[0] = new C(1 / math.sqrt(2), 1 / math.sqrt(2));

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

        Mesh GenMesh(Mesh mesh){
            var m = new Mesh();
            var vs = mesh.vertices;
            var ts = mesh.triangles;
            m.vertices = new [] {
                mesh.vertices[0],
                mesh.vertices[3],
                mesh.vertices[2],
                mesh.vertices[1]
            };
            
            m.triangles = new []
            {
                0, 3, 2,
                0, 1, 3, 
                3, 1, 2,
                1, 0, 2
            };
            m.RecalculateNormals();
            return m;
        }
    }
}