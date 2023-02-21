using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace  VectorField {
    public class GeomContainer : MonoBehaviour {
        [SerializeField] protected Material lineMat;
        [SerializeField] protected Material arrowMat;
        [SerializeField] protected Material pointMat;
        [SerializeField] protected Gradient ribbonColors;
        [SerializeField] protected Gradient vertexColors;
        public HeGeom geom { get; private set; }
        public Mesh mesh { get; private set; }
        public Material LineMat => lineMat;
        TangentRibbon ribbon;
        TangentArrow  arrow;

        void Awake() {
            var f = GetComponentInChildren<MeshFilter>();
            mesh = HeComp.Weld(f.sharedMesh);
            geom = new HeGeom(mesh, transform);
            f.sharedMesh = mesh;
        }

        public void PutSingularityPoint(int vid) {
            var o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var r = o.GetComponent<MeshRenderer>();
            o.transform.position = geom.Pos[vid];
            o.transform.localScale *= geom.MeanEdgeLength() * 0.3f;
            //r.sharedMaterial = new Material(pointMat);
        }

        public void PaintVerts() {
        }
        
        public void BuildArrowBuffer(float3[] faceVector) {
            arrow = new TangentArrow(faceVector, geom);
        }

        public void DrawArrows() {
            arrowMat.SetBuffer("_Line", arrow.tangentBuff);
            arrowMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nFaces * 6);
        }
        
        public void BuildRibbonBuffer(float3[] faceVector) {
            var n = (int)(geom.nFaces * 0.05f);
            var l = (int)(geom.nFaces * 0.1f);
            ribbon = new TangentRibbon(faceVector, geom, n, l, ribbonColors);
        }
        public void DrawRibbons() {
            lineMat.SetBuffer("_Line", ribbon.tracerBuff);
            lineMat.SetBuffer("_Norm", ribbon.normalBuff);
            lineMat.SetBuffer("_Col",  ribbon.colourBuff);
            lineMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, ribbon.nTracers);
        }

        private void OnDestroy() {
            arrow?.Dispose();
            ribbon?.Dispose();
        }
    }
}
