using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace ddg {
    using f2 = float2;
    using f3 = float3;
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

        public static f2 TangentOnFacePlane(float3 v, Face f, HeGeom g) {
                var o = g.OrthonormalBasis(f);
                return normalize(new f2(dot(v, o.Item1), dot(v, o.Item2)));
        }

        public static (bool flag, float r) Intersect(float2 dir, float2 pos, float2 p1, float2 p2) {
            var a = pos;
            var b = pos + dir * 1000;
            var c = p1;
            var d = p2;
            var deno = cross(new f3(b - a, 0), new f3(d - c, 0)).z;
            if (deno == 0) return (false, 0);
            var s = cross(float3(c - a, 0), float3(d - c, 0)).z / deno;
            var t = cross(float3(b - a, 0), float3(a - c, 0)).z / deno;
            if (s < 0.0 || 1.0 < s || t < 0.0 || 1.0 < t) { return (false, 0); }
            return (true, t);
        }

        public static (HalfEdge h, float r) CrossHe (float3 tng, Face f, HalfEdge h, float r, HeGeom g) {
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
                if(intersect_h1.flag) return (h1, intersect_h1.r);
                if(intersect_h2.flag) return (h2, intersect_h2.r);
            }
            if (h.id == h1.id) {
                var pBgn = v1 * r + v2 * (1f - r);
                var intersect_h2 = Intersect(dir, pBgn, v2, v0);
                var intersect_h0 = Intersect(dir, pBgn, v0, v1);
                if(intersect_h2.flag) return (h2, intersect_h2.r);
                if(intersect_h0.flag) return (h0, intersect_h0.r);
            }
            if (h.id == h2.id) {
                var pBgn = v2 * (1f - r);
                var intersect_h0 = Intersect(dir, pBgn, v0, v1);
                var intersect_h1 = Intersect(dir, pBgn, v1, v2);
                if(intersect_h0.flag) return (h0, intersect_h0.r);
                if(intersect_h1.flag) return (h1, intersect_h1.r);
            }

            throw new System.Exception();
        }
    }
}
