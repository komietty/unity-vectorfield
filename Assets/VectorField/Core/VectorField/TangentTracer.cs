using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace ddg {
    using f2 = float2;
    using f3 = float3;
    using d2 = double2;
    using d3 = double3;
    public class TangentTracer {
        public static (float tray, float tline) RayLineIntersection(f2 rayBgn, f2 rayDir, f2 lineA, f2 lineB) {
            var v1 = rayBgn - lineA;
            var v2 = lineB - lineA;
            var v3 = new f2(-rayDir.y, rayDir.x);
            var cross21 = v2.x * v1.y - v2.y * v1.x;
            var tray = cross21 / dot(v2, v3);
            var tline = dot(v1, v3) / dot(v2, v3);
            if (tray < 0) tray = float.PositiveInfinity;
            return (tray, tline);
        }

        public static d2 TangentOnFacePlane(f3 v, Face f, HeGeom g) {
                var o = g.OrthonormalBasis(f);
                return new double2(
                    dot((d3)v, (d3)o.Item1),
                    dot((d3)v, (d3)o.Item2)
                );
        }

        public static (bool flag, float r) Intersect(d2 dir, d2 pos, d2 p1, d2 p2) {
            var a = pos;
            var b = pos + dir;
            var c = p1;
            var d = p2;
            var deno = cross(new d3(b - a, 0), new d3(d - c, 0)).z;
            if (deno == 0) { return (false, 0); }
            var s = cross(new d3(c - a, 0), new d3(d - c, 0)).z / deno;
            var t = cross(new d3(b - a, 0), new d3(a - c, 0)).z / deno;
            var e = 1e-5;
            if (s < -e || t < -e || 1 + e < t) {
//                Debug.Log("s:" + s);
//                Debug.Log("t:" + t);
                return (false, 0);
            }
            return (true, (float)t);
        }

        public static (bool f, int hid, float r) CrossHe (float3 tng, Face f, HalfEdge h, float r, HeGeom g) {
            var h0 = g.halfedges[f.hid];
            var h1 = h0.next;
            var h2 = h0.prev;

            var dir = TangentOnFacePlane(tng, f, g);
            var v0 = new float2();
            var v1 = TangentOnFacePlane(g.Pos[h1.vid] - g.Pos[h0.vid], f, g);
            var v2 = TangentOnFacePlane(g.Pos[h2.vid] - g.Pos[h0.vid], f, g);
            if (h.id == h0.id) {
                var pBgn = v1 * r;
                var intersect_h1 = Intersect(dir, pBgn, v1, v2);
                var intersect_h2 = Intersect(dir, pBgn, v2, v0);
                if (intersect_h1.flag) { return (true, h1.id, intersect_h1.r); }
                if (intersect_h2.flag) { return (true, h2.id, intersect_h2.r); }
            }
            if (h.id == h1.id) {
                var pBgn = v1 * (1f - r) + v2 * r;
                var intersect_h2 = Intersect(dir, pBgn, v2, v0);
                var intersect_h0 = Intersect(dir, pBgn, v0, v1);
                if (intersect_h2.flag) { return (true, h2.id, intersect_h2.r); }
                if (intersect_h0.flag) { return (true, h0.id, intersect_h0.r); }
            }
            if (h.id == h2.id) {
                var pBgn = v2 * (1f - r);
                var intersect_h0 = Intersect(dir, pBgn, v0, v1);
                var intersect_h1 = Intersect(dir, pBgn, v1, v2);
                if (intersect_h0.flag) { return (true, h0.id, intersect_h0.r); }
                if (intersect_h1.flag) { return (true, h1.id, intersect_h1.r); }
            }

            Debug.LogWarning("not found");
            return (false, 0, 0);
        }
    }
}
