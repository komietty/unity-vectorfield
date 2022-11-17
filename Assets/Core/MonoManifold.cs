using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace ddg {
    public abstract class MonoMfdViewer : MonoBehaviour {
        [SerializeField] protected Shader shader;
        protected Mesh mesh;
        protected Material mat;
        protected HalfEdgeGeom geom;
        protected GraphicsBuffer colorBuffer;

        protected virtual void Start() {
            mat = new Material(shader);
            var filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            mesh = MeshUtils.Weld(filt.sharedMesh);
            filt.sharedMesh = mesh;
            rend.material = mat;
            geom = new HalfEdgeGeom(mesh);
            colorBuffer = new GraphicsBuffer(Target.Structured, geom.nVerts, sizeof(float) * 3);
        }

        protected void UpdateColor() {
            var n = geom.nVerts;
            var max = 0f;
            var vals = new float[n];
            var lrps = new Vector3[n];
            for (var i = 0; i < n; i++) {
                var vrt = geom.Verts[i];
                var val = GetValueOnSurface(vrt);
                vals[i] = val;
                max = Mathf.Max(Mathf.Abs(val), max);
            }
            max = Mathf.Min(Mathf.PI / 8, max);
            for (var i = 0; i < n; i++) { lrps[i] = ColorMap.Color(vals[i], -max, max); }
            colorBuffer.SetData(lrps);
            mat.SetBuffer("_Color", colorBuffer);
        }
        protected abstract float GetValueOnSurface(Vert v);

        void OnDestroy() {
            Destroy(mat);
            colorBuffer?.Dispose();
        }
    }
}
