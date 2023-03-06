using UnityEngine;
using static Unity.Mathematics.math;

namespace VectorField {
    public class CurvatureViewer : MonoBehaviour {
        public enum CurvType {
            ScalarGaussCurvature,
            ScalarMeanCurvature,
            PrincipalCurvature,
            NormalCurvature
        };
        [SerializeField] protected CurvType curvType;
        [SerializeField] protected Gradient colScheme;
        protected HeGeom geom;
        protected Mesh mesh;

        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            mesh = HeComp.Weld(filt.sharedMesh);
            geom = new HeGeom(mesh, transform);
            filt.sharedMesh = mesh;
            UpdateCol(GenCurvatureCol());
        }

        protected void UpdateCol(float[] values) {
            var n = geom.nVerts;
            var x = 0f;
            var vals = new float[n];
            var lrps = new Color[n];
            for (var i = 0; i < n; i++) {
                var vrt = geom.Verts[i];
                var val = values[vrt.vid];
                vals[i] = val;
                x = max(abs(val), x);
            }
            x = min(PI / 8, x);
            for (var i = 0; i < n; i++) {
                var min = -x;
                var max =  x;
                var c = colScheme.Evaluate((Mathf.Clamp(vals[i], min, max) - min) / (max - min));
                lrps[i] = c;
            }
            mesh.SetColors(lrps);
        }

        float[] GenCurvatureCol() {
            var c = new float[geom.nVerts];
            var t = curvType;
            for (var i = 0; i < geom.nVerts; i++) {
                var v = geom.Verts[i];
                if      (t == CurvType.ScalarGaussCurvature) c[i] = geom.ScalarGaussCurvature(v);
                else if (t == CurvType.ScalarMeanCurvature)  c[i] = geom.ScalarMeanCurvature(v);
                else {
                    var a = geom.BarycentricDualArea(v);
                    var k = geom.PrincipalCurvature(v);
                    if (t == CurvType.PrincipalCurvature) c[i] = k.y * (float)a;
                    if (t == CurvType.NormalCurvature)    c[i] = k.x * (float)a;
                }
            }
            return c;
        }
    }
}
