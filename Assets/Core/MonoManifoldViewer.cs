using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace ddg {
    public abstract class MonoMfdViewer : MonoBehaviour {
        [SerializeField] protected Shader shader;
        [SerializeField] protected Shader lineShader;
        [SerializeField] protected bool showNormal;
        [SerializeField] protected bool showTangent;
        protected Mesh mesh;
        protected Material mat;
        protected Material nrmMat;
        protected HalfEdgeGeom geom;
        protected GraphicsBuffer colBuffer;
        protected GraphicsBuffer nrmBuffer;
        protected GraphicsBuffer tngBuffer;

        protected virtual void Start() {
            mat = new Material(shader);
            nrmMat = new Material(lineShader);
            var filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            mesh = MeshUtils.Weld(filt.sharedMesh);
            filt.sharedMesh = mesh;
            rend.material = mat;
            geom = new HalfEdgeGeom(mesh);
            colBuffer = new GraphicsBuffer(Target.Structured, geom.nVerts, sizeof(float) * 3);
            nrmBuffer = new GraphicsBuffer(Target.Structured, geom.nVerts * 2, sizeof(float) * 3);
            tngBuffer = new GraphicsBuffer(Target.Structured, geom.nVerts * 2, sizeof(float) * 3);
        }

        protected void UpdateColor() {
            var n = geom.nVerts;
            var max = 0f;
            var vals = new float[n];
            var lrps = new Vector3[n];
            var nrms = new Vector3[n * 2];
            var tngs = new Vector3[n * 2];

            for (var i = 0; i < n; i++) {
                var vrt = geom.Verts[i];
                var val = GetValueOnSurface(vrt);
                vals[i] = val;
                max = Mathf.Max(Mathf.Abs(val), max);
            }

            max = Mathf.Min(Mathf.PI / 8, max);

            for (var i = 0; i < n; i++)     {
                lrps[i] = ColorMap.Color(vals[i], -max, max);
                nrms[i * 2 + 0] = geom.Pos[i];
                nrms[i * 2 + 1] = geom.Pos[i] + geom.Nrm[i] * 0.05f;
            }

            colBuffer.SetData(lrps);
            nrmBuffer.SetData(nrms);
            tngBuffer.SetData(lrps);
            mat.SetBuffer("_Col", colBuffer);
            nrmMat.SetBuffer("_Nrm", nrmBuffer);
            nrmMat.SetBuffer("_Tng", tngBuffer);
        }

        void OnRenderObject() {
            if (showNormal) {
                nrmMat.SetPass(0);
                Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nVerts * 2);
            }
        }

        protected abstract float GetValueOnSurface(Vert v);

        void OnDestroy() {
            Destroy(mat);
            Destroy(nrmMat);
            colBuffer?.Dispose();
            nrmBuffer?.Dispose();
            tngBuffer?.Dispose();
        }
    }
}
