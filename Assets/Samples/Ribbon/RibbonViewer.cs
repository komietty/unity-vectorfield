using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace VectorField.Demo {
    public class RibbonViewer : TangentBundle {
        [SerializeField] protected Material tracerMat;
        [SerializeField] protected int tracerNum;
        [SerializeField] protected int tracerLen;
        [SerializeField] protected float tracerOffset;
        TangentTracer tracer;
        GraphicsBuffer tracerBuff;
        GraphicsBuffer colourBuff;
        GraphicsBuffer normalBuff;
        List<Vector3> tracers = new List<Vector3>();
        List<Vector3> colours = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
    
        protected override void Start() {
            base.Start();
            var t = new TrivialConnection(geom, new HodgeDecomposition(geom));
            var s = new float[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
            s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            var tngs = t.GenField(t.ComputeConnections(s));
            tracer = new TangentTracer(geom, tngs, tracerLen);
            UpdateTng(tngs);
    
            for (var i = 0; i < tracerNum; i++) {
                var f = geom.Faces[UnityEngine.Random.Range(0, geom.nFaces)];
                var r = tracer.GenTracer(f);
                var c = Color.HSVToRGB(0.0f + (i % 10) * 0.01f, UnityEngine.Random.Range(0.5f, 1f), 1);
                for (var j = 0; j < r.Count - 1; j++) {
                    var tr0 = r[j];
                    var tr1 = r[j + 1];
                    var bt0 = math.cross(tr1.p - tr0.p, tr0.n); 
                    var bt1 = math.cross(tr1.p - tr0.p, tr1.n); 
                    tracers.Add(tr0.p + tr0.n * tracerOffset);
                    tracers.Add(tr1.p + tr1.n * tracerOffset);
                    normals.Add(tr0.n);
                    normals.Add(tr1.n);
                    colours.Add((Vector4)c);
                    colours.Add((Vector4)c);
                }
            }
            tracerBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tracers.Count, 12);
            colourBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tracers.Count, 12);
            normalBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tracers.Count, 12);
            tracerBuff.SetData(tracers);
            normalBuff.SetData(normals);
            colourBuff.SetData(colours);
        }
    
        protected override void OnRenderObject() {
            base.OnRenderObject();
            tracerMat.SetPass(0);
            tracerMat.SetBuffer("_Line", tracerBuff);
            tracerMat.SetBuffer("_Norm", normalBuff);
            tracerMat.SetBuffer("_Color", colourBuff);
            Graphics.DrawProceduralNow(MeshTopology.Lines, tracers.Count);
        }
    
        protected override void OnDestroy() {
            base.OnDestroy();
            tracerBuff.Dispose();
            colourBuff.Dispose();
        }
    }
}