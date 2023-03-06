using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace VectorField {
    public class TangentRibbon: System.IDisposable {
        public GraphicsBuffer tracerBuff { get; }
        public GraphicsBuffer colourBuff { get; }
        public GraphicsBuffer normalBuff { get; }
        public int nTracers { get; }
        
        public TangentRibbon(float3[] faceVector, HeGeom geom, int num, int len, Gradient col) {
            var tracer = new TangentTracer(geom, faceVector, len);
            var tracers = new List<Vector3>();
            var colours = new List<Vector3>();
            var normals = new List<Vector3>();
            for (var i = 0; i < num; i++) {
                var f = geom.Faces[UnityEngine.Random.Range(0, geom.nFaces)];
                var m = geom.MeanEdgeLength();
                var r = tracer.GenTracer(f);
                //var c = col.Evaluate(UnityEngine.Random.Range(0, 11) * 0.1f);
                var c = Color.HSVToRGB(0.0f + (i % 10) * 0.01f, UnityEngine.Random.Range(0.5f, 1f), 1);
                for (var j = 0; j < r.Count - 1; j++) {
                    var tr0 = r[j];
                    var tr1 = r[j + 1];
                    tracers.Add(tr0.p + tr0.n * m * 0.1f);
                    tracers.Add(tr1.p + tr1.n * m * 0.1f);
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
            nTracers = tracers.Count;
        }

        public void Dispose() {
            tracerBuff?.Dispose();
            normalBuff?.Dispose();
            colourBuff?.Dispose();
        }
    }
}
