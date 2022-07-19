using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System.Linq;

namespace ddg {
    public class DiscreteCurvature : MonoBehaviour {
        public enum CurvatureType { ScalarGaussCurvature, ScalarMeanCurvature, PrincipalCurvatureMain, PrincipalCurvatureNorm };
        [SerializeField] protected Shader kd;
        [SerializeField] protected CurvatureType type;
        HalfEdgeGeom geom;
        GraphicsBuffer curvature;
        Material mat;
        bool started = false;

        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            var mesh = Weld(filt.sharedMesh);
            filt.sharedMesh = mesh;
            mat = new Material(kd);
            rend.material = mat;
            geom = new HalfEdgeGeom(mesh);
            SetCurvature(type);
            Debug.Log(geom.mesh.eulerCharactaristics);
            started = true;
        }


        void SetCurvature(CurvatureType t) {
            var n = geom.mesh.verts.Length;
            var k = new Vector3[n];
            var max = 0f;
            var arr = new float[n];
            curvature?.Dispose();
            curvature = new GraphicsBuffer(Target.Structured, n, sizeof(float) * 3);

            for (var i = 0; i < n; i++) {
                var v = geom.mesh.verts[i];
                var g = 0f;
                if (t == CurvatureType.ScalarGaussCurvature) g = geom.ScalarGaussCurvature(v);
                if (t == CurvatureType.ScalarMeanCurvature)  g = geom.ScalarMeanCurvature(v);
                if (t == CurvatureType.PrincipalCurvatureMain || t == CurvatureType.PrincipalCurvatureNorm) {
                    var area = geom.BarycentricDualArea(v);
                    var ks = geom.PrincipalCurvature(v);
                    if (t == CurvatureType.PrincipalCurvatureMain) g = ks.y * area;
                    if (t == CurvatureType.PrincipalCurvatureNorm) g = ks.x * area;
                }
                arr[i] = g;
                max = Mathf.Max(Mathf.Abs(g), max);
            }

            max = Mathf.Min(Mathf.PI / 8, max);
            for (var i = 0; i < n; i++) {
                k[i] = ColorMap.Color(arr[i], -max, max);
            }
            curvature.SetData(k);
        }

        void OnValidate()     { if (started) SetCurvature(type);  }
        void OnRenderObject() { mat.SetBuffer("_Color", curvature); }
        void OnDestroy()      { Destroy(mat); curvature?.Dispose(); }


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
