using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace  VectorField {
    public class GeomContainer : MonoBehaviour {
        [SerializeField] protected Material lineMat;
        [SerializeField] protected Gradient ribbonColors;
        [SerializeField] protected Gradient vertexColors;
        public HeGeom geom { get; private set; }
        public Mesh mesh { get; private set; }
        public Material LineMat => lineMat;
        TangentRibbon ribbon;

        void Awake() {
            var f = GetComponentInChildren<MeshFilter>();
            mesh = HeComp.Weld(f.sharedMesh);
            geom = new HeGeom(mesh, transform);
            f.sharedMesh = mesh;
        }
        
        public void PaintVerts() {
        }
        
        public void DrawArrows() {
        }
        
        public void BuildRibbonBuffer(float3[] tangets) {
            var n = (int)(geom.nFaces * 0.1f);
            var l = (int)(geom.nFaces * 0.1f);
            ribbon = new TangentRibbon(tangets, geom, n, l, ribbonColors);
        }
        public void DrawRibbons() {
            lineMat.SetBuffer("_Line", ribbon.tracerBuff);
            lineMat.SetBuffer("_Norm", ribbon.normalBuff);
            lineMat.SetBuffer("_Col",  ribbon.colourBuff);
            lineMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, ribbon.nTracers);
        }

        private void OnDestroy() {
            ribbon?.Dispose();
        }
    }
}
