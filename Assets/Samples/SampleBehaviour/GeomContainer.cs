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

        public void PaintVerts() { }

        private GraphicsBuffer vertTangetArrow;
        public void BuildVertArrowBuffer(float3[] vertVector) {
            var tngs = new Vector3[geom.nVerts * 6];
            var mlen = geom.MeanEdgeLength();
            for(var i = 0; i < geom.nVerts; i++){
                var vert = geom.Verts[i];
                var field = vertVector[i] * mlen * 0.3f;
                var C = geom.Pos[i];
                var N = geom.Nrm[i];
                field = ClampFieldLength(field, mlen * 0.3f);
                var fc1 = C + N * mlen * 0.1f;
                var fc2 = C + field * 2 + N * mlen * 0.1f;
                var v = fc2 - fc1;
                var vT = math.cross(N, v);
                tngs[i * 6 + 0] = fc1;
                tngs[i * 6 + 1] = fc2;
                tngs[i * 6 + 2] = fc2;
                tngs[i * 6 + 3] = fc2 - v * 0.2f + vT * 0.2f;
                tngs[i * 6 + 4] = fc2;
                tngs[i * 6 + 5] = fc2 - v * 0.2f - vT * 0.2f;
            }
            vertTangetArrow = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tngs.Length, 12);
            vertTangetArrow.SetData(tngs);
        }
        
        Vector3 ClampFieldLength(Vector3 field, float len) {
            var m = field.magnitude;
            return m > len ? field * len / m : field;
        }
        
        public void DrawVertArrows() {
            arrowMat.SetBuffer("_Line", vertTangetArrow);
            arrowMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nVerts * 6);
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
