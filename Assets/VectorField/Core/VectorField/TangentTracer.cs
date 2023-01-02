using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace ddg {
    using f2 = float2;
    using f3 = float3;

    public class TangentTracer {
        HeGeom geom;
        float3[] tangents;
        int maxlength;

        public TangentTracer(HeGeom geom, f3[] tangents, int maxlength) {
            this.geom = geom;
            this.tangents = tangents;
            this.maxlength = maxlength;
        }

        public List<Vector3> GenTracer(Face bgn) {
            var f = bgn;
            var h = geom.halfedges[f.hid];
            var r = UnityEngine.Random.Range(0.1f, 0.9f);
            var fwd = GenTracer(f, h, r, 1);
            var bwd = GenTracer(h.twin.face, h.twin, 1 - r, -1);
            bwd.RemoveAt(0);
            bwd.Reverse();
            bwd.AddRange(fwd);
            return bwd;
        }


        List<Vector3> GenTracer(Face fbgn, HalfEdge hfeg, float rate, float sign) {
            var f = fbgn;
            var h = hfeg;
            var r = rate;
            var l = new List<Vector3>();
            for (var i = 0; i < maxlength; i++) {
                var t = tangents[f.fid] * sign;
                var p = geom.Pos[h.next.vid] * r + geom.Pos[h.vid] * (1 - r);
                l.Add(p + (Vector3)geom.FaceNormal(f).n * 0.01f);
                var (_f, _h, _r) = CrossHalfedge(t, f, h, r);
                if (!_f || h.onBoundary) break;
                h = _h.twin;
                f = h.face;
                r = Mathf.Clamp(1f - _r, 0.01f, 0.99f);
            }
            return l;
        }

        f2 OnFacePlane(f3 v, Face f) {
            var (a, b) = geom.OrthonormalBasis(f);
            return new f2(dot(v, a), dot(v, b));
        }

        float Cross(f2 a, f2 b) => b.x * a.y - b.y * a.x;

        bool Intersect(f2 a, f2 b, f2 c, f2 d, out float rate) {
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
}
