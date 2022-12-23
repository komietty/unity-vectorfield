using UnityEngine;
using Unity.Mathematics;
using MathNet.Numerics.LinearAlgebra;
using static UnityEngine.GraphicsBuffer;
using static Unity.Mathematics.math;

namespace ddg {
    using V = Vector<double>;

    public abstract class TangentBundleBehaviour : MonoBehaviour {
        [SerializeField] protected Material surfaceMat;
        [SerializeField] protected Shader lineShader;
        [SerializeField] protected bool showNormal;
        [SerializeField] protected bool showTangent;
        protected Mesh mesh;
        protected TangentBundle bundle;
        protected Material nrmMat, tngMat;
        protected GraphicsBuffer colBuf, nrmBuf, tngBuf;

        protected virtual void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            mesh = MeshUtils.Weld(filt.sharedMesh);
            nrmMat = new Material(lineShader);
            tngMat = new Material(lineShader);
            filt.sharedMesh = mesh;
            rend.material = surfaceMat;
            bundle = new TangentBundle(mesh);
            colBuf = new GraphicsBuffer(Target.Structured, bundle.geom.nVerts,     12);
            nrmBuf = new GraphicsBuffer(Target.Structured, bundle.geom.nVerts * 2, 12);
            tngBuf = new GraphicsBuffer(Target.Structured, bundle.geom.nFaces * 6, 12);
            UpdateNrm();
        }

        protected void UpdateCol(float[] values) {
            var g = bundle.geom;
            var n = g.nVerts;
            var x = 0f;
            var vals = new float[n];
            var lrps = new Vector3[n];
            for (var i = 0; i < n; i++) {
                var vrt = g.Verts[i];
                var val = values[vrt.vid];
                vals[i] = val;
                x = max(abs(val), x);
            }
            x = min(PI / 8, x);
            for (var i = 0; i < n; i++) lrps[i] = ColorMap.Color(vals[i], -x, x);
            colBuf.SetData(lrps);
            surfaceMat.SetBuffer("_Col", colBuf);
        }

        protected void UpdateNrm() {
            var g = bundle.geom;
            var n = g.nVerts;
            var l = g.MeanEdgeLength();
            var nrms = new Vector3[n * 2];
            for (var i = 0; i < n; i++)     {
                nrms[i * 2 + 0] = g.Pos[i];
                nrms[i * 2 + 1] = g.Pos[i] + g.Nrm[i] * l;
            }
            nrmBuf.SetData(nrms);
            nrmMat.SetBuffer("_Line", nrmBuf);
        }

        protected void UpdateTng(V omega) {
            var g = bundle.geom;
            var n = g.nFaces;
            var tngs = new Vector3[n * 6];
            var mlen = 0.3f * g.MeanEdgeLength();
            var omegaField = TangentBundle.InterpolateWhitney(omega, g);
            for(var i = 0; i < n; i++){
                var face = g.Faces[i];
                var field = omegaField[i] * mlen;
                var C = (float3)g.Centroid(face);
                var (_, N) = g.FaceNormal(face);
                field = ClampFieldLength(field, mlen);
                var fc1 = C - field + N * 0.005f;
                var fc2 = C + field + N * 0.005f;
                var v = fc2 - fc1;
                var vT = math.cross(N, v);
                tngs[i * 6 + 0] = fc1;
                tngs[i * 6 + 1] = fc2;
                tngs[i * 6 + 2] = fc2;
                tngs[i * 6 + 3] = fc2 - v * 0.2f + vT * 0.2f;
                tngs[i * 6 + 4] = fc2;
                tngs[i * 6 + 5] = fc2 - v * 0.2f - vT * 0.2f;
            }
            tngBuf.SetData(tngs);
            tngMat.SetBuffer("_Line", tngBuf);
        }
    
        protected Vector3 ClampFieldLength(Vector3 field, float len) {
            var m = field.magnitude;
            return m > len ? field * len / m : field;
        }

        protected virtual void OnRenderObject() {
            if (showNormal) {
                var n = bundle.geom.nVerts * 2;
                nrmMat.SetPass(0);
                Graphics.DrawProceduralNow(MeshTopology.Lines, n);
            }
            if (showTangent) {
                var n = bundle.geom.nFaces * 6;
                tngMat.SetPass(0);
                Graphics.DrawProceduralNow(MeshTopology.Lines, n);
            }
        }

        protected virtual void OnDestroy() {
            Destroy(nrmMat);
            Destroy(tngMat);
            colBuf?.Dispose();
            nrmBuf?.Dispose();
            tngBuf?.Dispose();
        }
    }
}
