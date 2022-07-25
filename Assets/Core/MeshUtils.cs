using UnityEngine;
using System.Linq;

namespace ddg {
    public class MeshUtils {
        public static Mesh Weld(Mesh original) {
            var ogl_vrts = original.vertices;
            var ogl_idcs = original.triangles;
            var alt_mesh = new Mesh();
            var alt_vrts = ogl_vrts.Distinct().ToArray();
            var alt_idcs = new int[ogl_idcs.Length];
            var vrt_rplc = new int[ogl_vrts.Length];
            for (var i = 0; i < ogl_vrts.Length; i++) {
                var o = -1;
                for (var j = 0; j < alt_vrts.Length; j++) {
                    if (alt_vrts[j] == ogl_vrts[i]) { o = j; break; }
                }
                vrt_rplc[i] = o;
            }

            for (var i = 0; i < alt_idcs.Length; i++) {
                alt_idcs[i] = vrt_rplc[ogl_idcs[i]];
            }
            alt_mesh.SetVertices(alt_vrts);
            alt_mesh.SetTriangles(alt_idcs, 0);
            return alt_mesh;
        }

        public static void Normalize(Vector3[] ps, bool rescale = true) {
            var n = ps.Length;
            var c = new Vector3();
            foreach (var p in ps) { c += p; }
            c /= n;

            var rad = -1f;
            for (int i = 0; i < n; i++) {
                ps[i] -= c;
                rad = Mathf.Max(rad, ps[i].magnitude);
            }

            if (rescale) {
                for (int i = 0; i < n; i++) { ps[i] /= rad; }
            }
        }
    }
}