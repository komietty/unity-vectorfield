using UnityEngine;
using System.Collections;
using System;

namespace ddg {
    public class CurvatureView : MonoMfdViewer {
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
            UpdateColor();
            //StartCoroutine(ChangeCurvature());
        }

        void OnGUI() {
            Rect rect1 = new Rect(10, 10, 600, 100);
            GUI.Label(rect1, curvType.ToString(), style);
        }

        IEnumerator ChangeCurvature() {
            while(true) {
                UpdateColor();
                yield return new WaitForSeconds(2);
                curvType = (CurvType)(((int)curvType + 1) % Enum.GetNames(typeof(CurvType)).Length);
            }
        }

        protected override float GetValueOnSurface(Vert v) {
            var t = curvType;
            var g = 0f;
            if      (t == CurvType.ScalarGaussCurvature) g = geom.ScalarGaussCurvature(v);
            else if (t == CurvType.ScalarMeanCurvature)  g = geom.ScalarMeanCurvature(v);
            else {
                var area = geom.BarycentricDualArea(v);
                var ks = geom.PrincipalCurvature(v);
                if (t == CurvType.PrincipalCurvature) g = ks.y * area;
                if (t == CurvType.NormalCurvature)    g = ks.x * area;
            }
            return g;
        }
    }
}
