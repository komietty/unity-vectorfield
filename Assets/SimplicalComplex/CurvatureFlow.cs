using System.Linq;
using UnityEngine;

namespace ddg {
    public class CurvatureFlow : MonoBehaviour {
        [SerializeField, Range(0.001f, 0.1f)] protected float delta = 0.001f;
        HalfEdgeGeom geom;
        MeanCurvatureFlow flow;
        MeshFilter filt;

        void Start() {
            filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            //var mesh = Weld(filt.sharedMesh);
            var mesh = new Mesh();
            mesh.SetVertices(new Vector3[]{
                new Vector3(0, -0.5f, -1),
                new Vector3(0.866025f, -0.5f, 0.5f),
                new Vector3(-0.866025f, -0.5f, 0.5f),
                new Vector3(0, 0.5f, 0),
                });
            mesh.SetIndices(new int[]{0, 3, 1, 0, 1, 2, 1, 3, 2, 2, 3, 0}, MeshTopology.Triangles, 0);
            filt.sharedMesh = mesh;
            geom = new HalfEdgeGeom(mesh);
            flow = new MeanCurvatureFlow(geom);
        }

        void Update() {
            if(Input.GetKeyDown(KeyCode.Space)){
                flow.Integrate(delta);
                filt.sharedMesh.SetVertices(geom.mesh.verts.Select(v => v.pos).ToArray());
            }
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


