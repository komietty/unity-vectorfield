using UnityEngine;
using System.Collections;
using System;
using static UnityEngine.GraphicsBuffer;

namespace ddg {
    public class CurvatureView : MonoBehaviour {
        public enum CurvType { ScalarGaussCurvature, ScalarMeanCurvature, PrincipalCurvature, NormalCurvature };
        [SerializeField] protected Shader kd;
        [SerializeField] protected CurvType curvType;
        protected Mesh mesh;
        protected HalfEdgeGeom geom;
        protected GraphicsBuffer buff;
        protected Material mat;
        protected GUIStyle style;

        void Start() {
            Init();
            StartCoroutine(ChangeCurvature());
        }
        void OnGUI() {
            Rect rect1 = new Rect(10, 10, 600, 100);
            GUI.Label(rect1, curvType.ToString(), style);
        }

        IEnumerator ChangeCurvature() {
            while(true) {
                SetCurvature(curvType);
                yield return new WaitForSeconds(2);
                curvType = (CurvType)(((int)curvType + 1) % Enum.GetNames(typeof(CurvType)).Length);
            }
        }

        void OnDestroy() { Destroy(mat); buff?.Dispose(); }

        protected void Init() {
            mat = new Material(kd);
            var filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            mesh = MeshUtils.Weld(filt.sharedMesh);
            filt.sharedMesh = mesh;
            rend.material = mat;
            geom = new HalfEdgeGeom(mesh);
            buff = new GraphicsBuffer(Target.Structured, geom.nVerts, sizeof(float) * 3);
            style = new GUIStyle();
            var n = new GUIStyleState();
            n.textColor = Color.white;
            style.normal = n;
            style.fontSize = 25;
        }

        protected void SetCurvature(CurvType t) {
            var n = geom.nVerts;
            var k = new Vector3[n];
            var max = 0f;
            var arr = new float[n];

            for (var i = 0; i < n; i++) {
                var v = geom.Verts[i];
                var g = 0f;
                if      (t == CurvType.ScalarGaussCurvature) g = geom.ScalarGaussCurvature(v);
                else if (t == CurvType.ScalarMeanCurvature)  g = geom.ScalarMeanCurvature(v);
                else {
                    var area = geom.BarycentricDualArea(v);
                    var ks = geom.PrincipalCurvature(v);
                    if (t == CurvType.PrincipalCurvature) g = ks.y * area;
                    if (t == CurvType.NormalCurvature)    g = ks.x * area;
                }
                arr[i] = g;
                max = Mathf.Max(Mathf.Abs(g), max);
            }

            max = Mathf.Min(Mathf.PI / 8, max);
            for (var i = 0; i < n; i++) { k[i] = ColorMap.Color(arr[i], -max, max); }
            buff.SetData(k);
            mat.SetBuffer("_Color", buff);
        }
    }
}
