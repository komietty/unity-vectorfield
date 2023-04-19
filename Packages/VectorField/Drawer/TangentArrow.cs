using System.Linq;
using UnityEngine;
using Unity.Mathematics;

namespace VectorField {
    using static GraphicsBuffer.Target;
    
    public abstract class TangentArrow : System.IDisposable {
        public GraphicsBuffer buff { get; protected set; }
        public void Dispose() { buff?.Dispose(); }
        
        protected float offset = 0.05f;
        
        protected Vector3 ClampField(Vector3 field, float len) {
            var m = field.magnitude;
            return m > len ? field * len / m : field;
        }
    }

    public class TangentVertArrow : TangentArrow {
        public TangentVertArrow(float3[] vertVector, HeGeom geom, bool clamp) {
            var tngs = new Vector3[geom.nVerts * 6];
            var mlen = geom.MeanEdgeLength();
            var mfld = vertVector.Select(v => math.length(v)).Average();
            for(var i = 0; i < geom.nVerts; i++) {
                var field = vertVector[i];// * mlen * 0.3f;
                var C = geom.Pos[i];
                var N = geom.Nrm[i];
                //if(clamp) field = ClampField(field, mlen * 0.3f);
                if(clamp) field = field / mfld * mlen * 0.3f;
                var fc1 = C - field + N * mlen * offset;
                var fc2 = C + field + N * mlen * offset;
                var v = fc2 - fc1;
                var vT = math.cross(N, v);
                tngs[i * 6 + 0] = fc1;
                tngs[i * 6 + 1] = fc2;
                tngs[i * 6 + 2] = fc2;
                tngs[i * 6 + 3] = fc2 - v * 0.2f + vT * 0.2f;
                tngs[i * 6 + 4] = fc2;
                tngs[i * 6 + 5] = fc2 - v * 0.2f - vT * 0.2f;
            }
            buff = new GraphicsBuffer(Structured, tngs.Length, 12);
            buff.SetData(tngs);
        }
        
    }
    public class TangentFaceArrow : TangentArrow {
        public TangentFaceArrow(float3[] faceVector, HeGeom geom, bool clamp) {
            var tngs = new Vector3[geom.nFaces * 6];
            var mlen = geom.MeanEdgeLength();
            for(var i = 0; i < geom.nFaces; i++){
                var face = geom.Faces[i];
                var field = faceVector[i] * mlen * 0.3f;
                var C = geom.Centroid(face);
                var N = geom.FaceNormal(face).n;
                if(clamp) field = ClampField(field, mlen * 0.3f);
                var fc1 = C - field + N * mlen * offset;
                var fc2 = C + field + N * mlen * offset;
                var v = fc2 - fc1;
                var vT = math.cross(N, v);
                tngs[i * 6 + 0] = fc1;
                tngs[i * 6 + 1] = fc2;
                tngs[i * 6 + 2] = fc2;
                tngs[i * 6 + 3] = fc2 - v * 0.2f + vT * 0.2f;
                tngs[i * 6 + 4] = fc2;
                tngs[i * 6 + 5] = fc2 - v * 0.2f - vT * 0.2f;
            }
            buff = new GraphicsBuffer(Structured, tngs.Length, 12);
            buff.SetData(tngs);
        }
    }
}
