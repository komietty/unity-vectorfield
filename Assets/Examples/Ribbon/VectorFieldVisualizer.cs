using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;

namespace ddg {
    public class VectorFieldVisualizer : MonoMfdViewer {
        protected HodgeDecomposition hodge;
    
        protected override void Start() {
            base.Start();
            hodge = new HodgeDecomposition(geom);
            var omega = TangentBundle.GenRandomOneForm(geom);
            var omegaM = DenseMatrix.OfColumnMajor(omega.Length, 1, omega.Select(v => (double)v));
            var dAlpha = hodge.ComputeExactComponent(omegaM);
            //deltaBeta = hodge.ComputeCoExactComponent(omegaM);
            tngBuffer.SetData(DrawArrow(dAlpha));
            nrmMat.SetBuffer("_Tng", tngBuffer);
        }
    
        Vector3[] DrawArrow(double[] omega) {
            var n = geom.nFaces;
            var arws = new Vector3[n * 6];
            var mlen = 0.3f * geom.MeanEdgeLength();
            var omegaField = TangentBundle.InterpolateWhitney(omega, geom);
            for(var i = 0; i < n; i++){
                var face = geom.Faces[i];
                var field = omegaField[i] * mlen;
                var C = (float3)geom.Centroid(face);
                var (_, N) = geom.FaceNormal(face);
                field = ClampFieldLength(field, mlen);
                var fc1 = C - field + N * 0.005f;
                var fc2 = C + field + N * 0.005f;
                var v = fc2 - fc1;
                var vT = math.cross(N, v);
                arws[i * 6 + 0] = fc1;
                arws[i * 6 + 1] = fc2;
                arws[i * 6 + 2] = fc2;
                arws[i * 6 + 3] = fc2 - v * 0.2f + vT * 0.2f;
                arws[i * 6 + 4] = fc2;
                arws[i * 6 + 5] = fc2 - v * 0.2f - vT * 0.2f;
            }
            return arws;
        }
    
        protected override void OnRenderObject() {
            nrmMat.SetPass(1);
            Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nFaces * 6);
        }
    
        protected override float GetValueOnSurface(Vert v) => throw new System.Exception(); 
    
        Vector3 ClampFieldLength(Vector3 field, float len) {
            var norm = field.magnitude;
            return norm > len ? field * len / norm : field;
        }
    }
}