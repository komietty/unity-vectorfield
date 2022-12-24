using UnityEngine;
using System.Collections;
using System;

namespace ddg {
    public class CurvatureViewer : TangentBundle {
        public enum CurvType {
            ScalarGaussCurvature,
            ScalarMeanCurvature,
            PrincipalCurvature,
            NormalCurvature
        };
        [SerializeField] protected CurvType curvType;

        protected override void Start() {
            base.Start();
            var n = new GUIStyleState();
            n.textColor = Color.white;
            StartCoroutine(ChangeCurvature());
        }

        IEnumerator ChangeCurvature() {
            while(true) {
                UpdateCol(GenCurvatureCol());
                yield return new WaitForSeconds(2);
                curvType = (CurvType)(((int)curvType + 1) % Enum.GetNames(typeof(CurvType)).Length);
            }
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
