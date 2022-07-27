using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace ddg {
    public class DiscreteCurvature : MonoBehaviour {
        public enum CurvatureType { ScalarGaussCurvature, ScalarMeanCurvature, PrincipalCurvature, NormalCurvature };
        [SerializeField] protected Shader kd;
        [SerializeField] protected CurvatureType curvatureType;
        protected Mesh mesh;
        protected HalfEdgeGeom geom;
        protected GraphicsBuffer curvature;
        protected Material mat;
        protected bool started = false;

        protected virtual void Start() {
            mat = new Material(kd);
            var filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            mesh = MeshUtils.Weld(filt.sharedMesh);
            filt.sharedMesh = mesh;
            rend.material = mat;
            geom = new HalfEdgeGeom(mesh);
            SetCurvature(curvatureType);
            started = true;
        }

        protected void SetCurvature(CurvatureType t) {
            var n = geom.nVerts;
            var k = new Vector3[n];
            var max = 0f;
            var arr = new float[n];
            curvature?.Dispose();
            curvature = new GraphicsBuffer(Target.Structured, n, sizeof(float) * 3);

            for (var i = 0; i < n; i++) {
                var v = geom.Verts[i];
                var g = 0f;
                if      (t == CurvatureType.ScalarGaussCurvature) g = geom.ScalarGaussCurvature(v);
                else if (t == CurvatureType.ScalarMeanCurvature)  g = geom.ScalarMeanCurvature(v);
                else {
                    var area = geom.BarycentricDualArea(v);
                    var ks = geom.PrincipalCurvature(v);
                    if (t == CurvatureType.PrincipalCurvature) g = ks.y * area;
                    if (t == CurvatureType.NormalCurvature)    g = ks.x * area;
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

        void OnValidate()     { if (started) SetCurvature(curvatureType);  }
        void OnRenderObject() { mat.SetBuffer("_Color", curvature); }
        void OnDestroy()      { Destroy(mat); curvature?.Dispose(); }
    }
}
