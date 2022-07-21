using System.Linq;
using UnityEngine;

namespace ddg {
    public class CurvatureFlow : MonoBehaviour {
        HalfEdgeGeom geom;
        MeanCurvatureFlow flow;

        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            var mesh = Weld(filt.sharedMesh);
            filt.sharedMesh = mesh;
            geom = new HalfEdgeGeom(mesh);
            flow = new MeanCurvatureFlow(geom);
        }

        void OnDestroy(){
            flow.Dispose();
        }

        Mesh Weld(Mesh original) {
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

    }
}


