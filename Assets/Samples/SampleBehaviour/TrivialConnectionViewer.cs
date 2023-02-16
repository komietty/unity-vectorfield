using UnityEngine;
using Unity.Mathematics;
using MathNet.Numerics.LinearAlgebra;
using static UnityEngine.GraphicsBuffer;

namespace VectorField {
    using V = Vector<double>;
    
    public class TrivialConnectionViewer : MonoBehaviour {
        [SerializeField] protected Material tngtMat;
        protected GraphicsBuffer tngBuf;
        protected HeGeom geom;

        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var mesh = HeComp.Weld(filt.sharedMesh);
            geom = new HeGeom(mesh, transform);
            filt.sharedMesh = mesh;
            tngBuf = new GraphicsBuffer(Target.Structured, geom.nFaces * 6, 12);
            var t = new TrivialConnection(geom, new HodgeDecomposition(geom));
            var s = new float[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
            s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            UpdateTng(t.GenField(t.ComputeConnections(s)));
        }
        
        protected void UpdateTng(float3[] omega) {
            var n = geom.nFaces;
            var tngs = new Vector3[n * 6];
            var mlen = 0.3f * geom.MeanEdgeLength();
            for(var i = 0; i < n; i++){
                var face = geom.Faces[i];
                var field = omega[i] * mlen;
                var C = geom.Centroid(face);
                var (_, N) = geom.FaceNormal(face);
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
            tngtMat.SetBuffer("_Line", tngBuf);
        }
    
        protected Vector3 ClampFieldLength(Vector3 field, float len) {
            var m = field.magnitude;
            return m > len ? field * len / m : field;
        }

        protected virtual void OnRenderObject() {
            tngtMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nFaces * 6);
        }

        protected virtual void OnDestroy() {
            tngBuf?.Dispose();
        }
    }
}