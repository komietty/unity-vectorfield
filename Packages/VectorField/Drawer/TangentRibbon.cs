using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace VectorField {
    using f2 = float2;
    using f3 = float3;
    
    public class TangentTracer {
        private readonly HeGeom geom;
        private readonly f3[] tangents;
        private readonly int maxlength;

        public TangentTracer(HeGeom geom, f3[] tangents, int maxlength) {
            this.geom = geom;
            this.tangents = tangents;
            this.maxlength = maxlength;
        }

        public List<(f3 p, f3 n)> GenTracer(Face f) {
            var h = geom.halfedges[f.hid];
            var r = UnityEngine.Random.Range(0.1f, 0.9f);
            var fwd = GenTracer(f, h, r, 1);
            var bwd = GenTracer(h.twin.face, h.twin, 1 - r, -1);
            bwd.RemoveAt(0);
            bwd.Reverse();
            bwd.AddRange(fwd);
            return bwd;
        }

        List<(f3 p, f3 n)> GenTracer(Face fbgn, HalfEdge hfeg, float rate, float sign) {
            var f = fbgn;
            var h = hfeg;
            var r = rate;
            var l = new List<(f3, f3)>();
            for (var i = 0; i < maxlength; i++) {
                var t = tangents[f.fid] * sign;
                var p = geom.Pos[h.next.vid] * r + geom.Pos[h.vid] * (1 - r);
                l.Add((p, geom.FaceNormal(f).n));
                var (_f, _h, _r) = CrossHalfedge(t, f, h, r);
                if (!_f || h.onBoundary) break;
                h = _h.twin;
                f = h.face;
                r = math.clamp(1f - _r, 0.01f, 0.99f);
            }
            return l;
        }

        f2 OnFacePlane(f3 v, Face f) {
            var (a, b) = geom.OrthonormalBasis(f);
            return new f2(math.dot(v, a), math.dot(v, b));
        }

        bool Intersect(f2 a, f2 b, f2 c, f2 d, out float rate) {
            float Cross(f2 a, f2 b) => b.x * a.y - b.y * a.x;
            var deno = Cross(b - a, d - c);
            if (deno == 0) { rate = 0; return false; }
            var s = Cross(c - a, d - c) / deno;
            var t = Cross(b - a, a - c) / deno;
            if (s < 0 || t < 0 || 1 < t) { rate = 0; return false; }
            rate = t;
            return true;
        }

        (bool, HalfEdge, float) CrossHalfedge(f3 t, Face f, HalfEdge h, float r) {
            var h0 = geom.halfedges[f.hid];
            var h1 = h0.next;
            var h2 = h0.prev;

            var v0  = new f2();
            var v1  = OnFacePlane(geom.Pos[h1.vid] - geom.Pos[h0.vid], f);
            var v2  = OnFacePlane(geom.Pos[h2.vid] - geom.Pos[h0.vid], f);
            var dir = OnFacePlane(t, f);
            if (h == h0) {
                var bgn = v1 * r;
                if (Intersect(bgn, bgn + dir, v1, v2, out float r1)) return (true, h1, r1);
                if (Intersect(bgn, bgn + dir, v2, v0, out float r2)) return (true, h2, r2);
            }
            else if (h == h1) {
                var bgn = v1 * (1f - r) + v2 * r;
                if (Intersect(bgn, bgn + dir, v2, v0, out float r2)) return (true, h2, r2);
                if (Intersect(bgn, bgn + dir, v0, v1, out float r0)) return (true, h0, r0);
            }
            else if (h == h2) {
                var bgn = v2 * (1f - r);
                if (Intersect(bgn, bgn + dir, v0, v1, out float r0)) return (true, h0, r0);
                if (Intersect(bgn, bgn + dir, v1, v2, out float r1)) return (true, h1, r1);
            }
            return (false, new HalfEdge(-1), 0);
        }
    }
    
    public class TangentRibbon: System.IDisposable {
        public GraphicsBuffer tracerBuff { get; }
        public GraphicsBuffer colourBuff { get; }
        public GraphicsBuffer normalBuff { get; }
        public int nTracers { get; }
        
        public TangentRibbon(f3[] faceVector, HeGeom geom, int num, int len) {
            var tracer = new TangentTracer(geom, faceVector, len);
            var tracers = new List<Vector3>();
            var colours = new List<Vector3>();
            var normals = new List<Vector3>();
            for (var i = 0; i < num; i++) {
                var f = geom.Faces[UnityEngine.Random.Range(0, geom.nFaces)];
                var m = geom.MeanEdgeLength();
                var r = tracer.GenTracer(f);
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
