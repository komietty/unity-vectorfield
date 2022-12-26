using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace ddg {
    using f2 = float2;
    public class TangentTracer {
        public (float tray, float tline) RayLineIntersection(f2 rayBgn, f2 rayDir, f2 lineA, f2 lineB) {
            var v1 = rayBgn - lineA;
            var v2 = lineB - lineA;
            var v3 = new f2(-rayDir.y, rayDir.x);
            var cross21 = v2.x * v1.y - v2.y * v1.x;
            var tray = cross21 / dot(v2, v3);
            var tline = dot(v1, v3) / dot(v2, v3);
            if (tray < 0) tray = float.PositiveInfinity;
            return (tray, tline);
        }
    }
}
