using UnityEngine;
using static Unity.Mathematics.math;

namespace VectorField {
    public class ScalarCurvatureViewer : MonoBehaviour {
        private enum CurvType {
            ScalarGaussCurvature,
            ScalarMeanCurvature,
            PrincipalCurvature,
            NormalCurvature
        };
        [SerializeField] private Gradient colScheme;
        [SerializeField] private CurvType curvType;

        void Start() {
            var C = GetComponent<GeomContainer>();
            var G = C.geom;
            var V = ComputeCurvatureColor(G);
            var x = 0f;
            var vals = new float[G.nVerts];
            var cols = new Color[G.nVerts];
            
            for (var i = 0; i < G.nVerts; i++) {
                var vrt = G.Verts[i];
                var val = V[vrt.vid];
                vals[i] = val;
                x = max(abs(val), x);
            }
            
            x = min(PI / 8, x);
            
            for (var i = 0; i < G.nVerts; i++) {
                var min = -x;
                var max =  x;
                var c = colScheme.Evaluate((clamp(vals[i], min, max) - min) / (max - min));
                cols[i] = c;
            }
            
            C.showVertArrow = false;
            C.showVertArrow = false;
            C.vertexColors = cols;
            C.surfMode = GeomContainer.SurfMode.vertexColorBase;
        }

        float[] ComputeCurvatureColor(HeGeom g) {
            var c = new float[g.nVerts];
            var t = curvType;
            for (var i = 0; i < g.nVerts; i++) {
                var v = g.Verts[i];
                if      (t == CurvType.ScalarGaussCurvature) c[i] = g.ScalarGaussCurvature(v);
                else if (t == CurvType.ScalarMeanCurvature)  c[i] = g.ScalarMeanCurvature(v);
                else {
                    var a = g.BarycentricDualArea(v);
                    var k = g.PrincipalCurvature(v);
                    if (t == CurvType.PrincipalCurvature) c[i] = k.y * (float)a;
                    if (t == CurvType.NormalCurvature)    c[i] = k.x * (float)a;
                }
            }
            return c;
        }
    }
}
