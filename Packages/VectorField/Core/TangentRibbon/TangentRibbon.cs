using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace VectorField {
    public class TangentRibbon: System.IDisposable {
        public GraphicsBuffer tracerBuff { get; }
        public GraphicsBuffer colourBuff { get; }
        public GraphicsBuffer normalBuff { get; }
        public int nTracers { get; }
        
        public TangentRibbon(float3[] tangents, HeGeom geom, int num, int len, Gradient col) {
            var tracer = new TangentTracer(geom, tangents, len);
            var tracers = new List<Vector3>();
            var colours = new List<Vector3>();
            var normals = new List<Vector3>();
            for (var i = 0; i < num; i++) {
                var f = geom.Faces[UnityEngine.Random.Range(0, geom.nFaces)];
                var m = geom.MeanEdgeLength();
                var r = tracer.GenTracer(f);
                var c = col.Evaluate(UnityEngine.Random.Range(0, 1f));
                for (var j = 0; j < r.Count - 1; j++) {
                    var tr0 = r[j];
                    var tr1 = r[j + 1];
                    tracers.Add(tr0.p + tr0.n * m / 3);
                    tracers.Add(tr1.p + tr1.n * m / 3);
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
