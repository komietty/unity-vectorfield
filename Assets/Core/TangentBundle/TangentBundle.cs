using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace ddg {
    public class TangentBundle {
        protected float[] tangentField;
        protected HeGeom geom;

        float3[] InterpolateWhitney(double[] oneForm) {
            var field = new float3[geom.nFaces];
            for (var i = 0; i < geom.nFaces; i++)
            {
                var f = geom.Faces[i];
                var h = geom.halfedges[f.hid];
                var pi = geom.Pos[h.vid];
                var pj = geom.Pos[h.next.vid];
                var pk = geom.Pos[h.prev.vid];
                var eij = pj - pi;
                var ejk = pk - pj;
                var eki = pi - pk;
                var cij = oneForm[h.edge.eid];
                var cjk = oneForm[h.next.edge.eid];
                var cki = oneForm[h.prev.edge.eid];
                if (h.edge.hid != h.id) cij *= -1;
                if (h.next.edge.hid != h.next.id) cjk *= -1;
                if (h.prev.edge.hid != h.prev.id) cki *= -1;
                var A = geom.Area(f);
                var (_, N) = geom.FaceNormal(f);
                var a = (eki - ejk) * (float)cij;
                var b = (eij - eki) * (float)cjk;
                var c = (ejk - eij) * (float)cki;
                field[i] = math.cross(N, (a + b + c)) / (float)(6 * A);
            }
            return field;
        }
    }
}