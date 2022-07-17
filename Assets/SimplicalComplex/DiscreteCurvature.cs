using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace ddg {
    public class DiscreteCurvature : MonoBehaviour {
        [SerializeField] protected Shader kd;
        HalfEdgeGeom geom;
        GraphicsBuffer curvature;
        Material mat;

        void Start() {
            var mesh = GetComponentInChildren<MeshFilter>().sharedMesh;
            var rend = GetComponentInChildren<MeshRenderer>();
            mat = new Material(kd);
            rend.material = mat;
            geom = new HalfEdgeGeom(mesh);
            var n = geom.mesh.verts.Length;
            var k = new Vector3[n];
            var max = 0f;

            for (var i = 0; i < n; i++) {
                var v = geom.mesh.verts[i];
                //var g = geom.ScalarGaussCurvature(v);
                var g = geom.ScalarMeanCurvature(v);
                max = Mathf.Max(Mathf.Abs(g), max);
            }

            max = Mathf.Min(Mathf.PI / 8, max);
            for (var i = 0; i < n; i++) {
                var v = geom.mesh.verts[i];
                var g = geom.ScalarGaussCurvature(v);
                k[i] = ColorMap.Color(g, -max, max);
            }
            curvature = new GraphicsBuffer(Target.Structured, n, sizeof(float) * 3);
            curvature.SetData(k);
            Debug.Log(geom.mesh.eulerCharactaristics);
        }

        void OnRenderObject() {
            mat.SetBuffer("_Color", curvature);
        }

        void OnDestroy() {
            Destroy(mat);
            curvature.Dispose();
        }
    }
}
