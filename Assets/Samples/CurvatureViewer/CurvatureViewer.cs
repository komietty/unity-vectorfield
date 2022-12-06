using UnityEngine;
using System.Collections;
using System;

namespace ddg {
    public class CurvatureViewer : TangentBundleBehaviour {
        public enum CurvType {
            ScalarGaussCurvature,
            ScalarMeanCurvature,
            PrincipalCurvature,
            NormalCurvature
        };
        [SerializeField] protected CurvType curvType;
        protected GUIStyle style;

        protected override void Start() {
            base.Start();
            style = new GUIStyle();
            var n = new GUIStyleState();
            n.textColor = Color.white;
            style.normal = n;
            style.fontSize = 25;
            StartCoroutine(ChangeCurvature());
        }

        void OnGUI() {
            Rect rect1 = new Rect(10, 10, 600, 100);
            GUI.Label(rect1, curvType.ToString(), style);
        }

        IEnumerator ChangeCurvature() {
            while(true) {
                UpdateCol(GenCurvatureCol());
                yield return new WaitForSeconds(2);
                curvType = (CurvType)(((int)curvType + 1) % Enum.GetNames(typeof(CurvType)).Length);
            }
        }

        float[] GenCurvatureCol() {
            var g = bundle.Geom;
            var c = new float[g.nVerts];
            var t = curvType;
            for (var i = 0; i < g.nVerts; i++) {
                var v = g.Verts[i];
                if (t == CurvType.ScalarGaussCurvature) c[i] = g.ScalarGaussCurvature(v);
                else if (t == CurvType.ScalarMeanCurvature) c[i] = g.ScalarMeanCurvature(v);
                else {
                    var area = g.BarycentricDualArea(v);
                    var ks = g.PrincipalCurvature(v);
                    if (t == CurvType.PrincipalCurvature) c[i] = ks.y * (float)area;
                    if (t == CurvType.NormalCurvature) c[i] = ks.x * (float)area;
                }
            }
            return c;
        }
    }
}
