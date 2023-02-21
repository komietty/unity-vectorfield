using UnityEngine;
using Unity.Mathematics;

namespace VectorField {
    public class TangentArrow : System.IDisposable {
        public GraphicsBuffer tangentBuff { get; }
        
        public TangentArrow(float3[] faceVector, HeGeom geom) {
            var tngs = new Vector3[geom.nFaces * 6];
            var mlen = geom.MeanEdgeLength();
            for(var i = 0; i < geom.nFaces; i++){
                var face = geom.Faces[i];
                var field = faceVector[i] * mlen * 0.3f;
                var C = geom.Centroid(face);
                var (_, N) = geom.FaceNormal(face);
                field = ClampFieldLength(field, mlen * 0.3f);
                var fc1 = C - field + N * mlen * 0.1f;
                var fc2 = C + field + N * mlen * 0.1f;
                var v = fc2 - fc1;
                var vT = math.cross(N, v);
                tngs[i * 6 + 0] = fc1;
                tngs[i * 6 + 1] = fc2;
                tngs[i * 6 + 2] = fc2;
                tngs[i * 6 + 3] = fc2 - v * 0.2f + vT * 0.2f;
                tngs[i * 6 + 4] = fc2;
                tngs[i * 6 + 5] = fc2 - v * 0.2f - vT * 0.2f;
            }
            tangentBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tngs.Length, 12);
            tangentBuff.SetData(tngs);
        }
        
        Vector3 ClampFieldLength(Vector3 field, float len) {
            var m = field.magnitude;
            return m > len ? field * len / m : field;
        }

        public void Dispose() {
            tangentBuff?.Dispose();
        }
    }
}
