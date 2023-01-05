using UnityEngine;
using static UnityEngine.GraphicsBuffer;
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
        [SerializeField] protected Material surfMat;
        protected GraphicsBuffer colBuf;
        protected HeGeom geom;
        protected Mesh mesh;

        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            mesh = HeComp.Weld(filt.sharedMesh);
            geom = new HeGeom(mesh, transform);
            filt.sharedMesh = mesh;
            rend.material = surfMat;
            colBuf = new GraphicsBuffer(Target.Structured, geom.nVerts,     12);
            UpdateCol(GenCurvatureCol());
        }
        void OnDestroy() {
            colBuf?.Dispose();
        }

        protected void UpdateCol(float[] values) {
            var n = geom.nVerts;
            var x = 0f;
            var vals = new float[n];
            var lrps = new Vector3[n];
            for (var i = 0; i < n; i++) {
                var vrt = geom.Verts[i];
                var val = values[vrt.vid];
                vals[i] = val;
                x = max(abs(val), x);
            }
            x = min(PI / 8, x);
            for (var i = 0; i < n; i++) lrps[i] = ColorMap.Color(vals[i], -x, x);
            colBuf.SetData(lrps);
            surfMat.SetBuffer("_Col", colBuf);
        }

        float[] GenCurvatureCol() {
            var c = new float[geom.nVerts];
            var t = curvType;
            for (var i = 0; i < geom.nVerts; i++) {
                var v = geom.Verts[i];
                if (t == CurvType.ScalarGaussCurvature) c[i] = geom.ScalarGaussCurvature(v);
                else if (t == CurvType.ScalarMeanCurvature) c[i] = geom.ScalarMeanCurvature(v);
                else {
                    var area = geom.BarycentricDualArea(v);
                    var ks = geom.PrincipalCurvature(v);
                    if (t == CurvType.PrincipalCurvature) c[i] = ks.y * (float)area;
                    if (t == CurvType.NormalCurvature) c[i] = ks.x * (float)area;
                }
            }
            return c;
        }
    }
}
