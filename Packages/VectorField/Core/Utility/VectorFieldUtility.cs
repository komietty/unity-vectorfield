using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static UnityEngine.GraphicsBuffer;

namespace VectorField {
    using RV = Vector<double>;

    public static class VectorFieldUtility {

        public static GraphicsBuffer GenVertTangeSpacesBuffer(HeGeom g) {
            var l = g.nVerts;
            var b = new GraphicsBuffer(Target.Structured, l * 6, 12);
            var a = new Vector3[l * 6];
            var m = g.MeanEdgeLength();
            for (var i = 0; i < l; i++)
            {
                var n = g.Nrm[i];
                var p = g.Pos[i] + n * m * 0.01f;
                var (ta, tb) = g.OrthonormalBasis(g.Verts[i]);
                a[i * 6 + 0] = p;
                a[i * 6 + 2] = p;
                a[i * 6 + 4] = p;
                a[i * 6 + 1] = p + ta * m * 0.3f;
                a[i * 6 + 3] = p + tb * m * 0.3f;
                a[i * 6 + 5] = p + n * m * 0.3f;
            }

            b.SetData(a);
            return b;
        }

        public static GraphicsBuffer GenFaceTangeSpacesBuffer(HeGeom g) {
            var l = g.nFaces;
            var b = new GraphicsBuffer(Target.Structured, l * 6, 12);
            var a = new Vector3[l * 6];
            var m = g.MeanEdgeLength();
            for (var i = 0; i < l; i++)
            {
                var f = g.Faces[i];
                var n = g.FaceNormal(f).n;
                var p = g.Centroid(f) + n * m * 0.01f;
                var (ta, tb) = g.OrthonormalBasis(f);
                a[i * 6 + 0] = p;
                a[i * 6 + 2] = p;
                a[i * 6 + 4] = p;
                a[i * 6 + 1] = p + ta * m * 0.3f;
                a[i * 6 + 3] = p + tb * m * 0.3f;
                a[i * 6 + 5] = p + n * m * 0.3f;
            }

            b.SetData(a);
            return b;
        }

        public static GraphicsBuffer GenVertTangentArrowsBuffer(HeGeom g, float3[] omega) {
            var l = g.nVerts;
            var b = new GraphicsBuffer(Target.Structured, l * 6, 12);
            var a = new Vector3[l * 6];
            var m = g.MeanEdgeLength();
            for (var i = 0; i < l; i++) {
                var n = g.Nrm[i];
                var p = g.Pos[i] + n * m * 0.01f;
                var field = (float3)ClampFieldLength(omega[i], 1) * m * 0.3f;
                var fc1 = p - field;
                var fc2 = p + field;
                var v = fc2 - fc1;
                var vT = cross(n, v);
                a[i * 6 + 0] = fc1;
                a[i * 6 + 1] = fc2;
                a[i * 6 + 2] = fc2;
                a[i * 6 + 3] = fc2 - v * 0.2f + vT * 0.2f;
                a[i * 6 + 4] = fc2;
                a[i * 6 + 5] = fc2 - v * 0.2f - vT * 0.2f;
            }

            b.SetData(a);
            return b;
        }

        public static GraphicsBuffer GenFaceTangentArrowsBuffer(HeGeom g, float3[] omega) {
            var l = g.nFaces;
            var b = new GraphicsBuffer(Target.Structured, l * 6, 12);
            var a = new Vector3[l * 6];
            var m = g.MeanEdgeLength();
            for (var i = 0; i < l; i++) {
                var f = g.Faces[i];
                var n = g.FaceNormal(f).n;
                var p = g.Centroid(f) + n * m * 0.01f;
                var field = (float3)ClampFieldLength(omega[i], 1) * m * 0.3f;
                var fc1 = p - field;
                var fc2 = p + field;
                var v = fc2 - fc1;
                var vT = cross(n, v);
                a[i * 6 + 0] = fc1;
                a[i * 6 + 1] = fc2;
                a[i * 6 + 2] = fc2;
                a[i * 6 + 3] = fc2 - v * 0.2f + vT * 0.2f;
                a[i * 6 + 4] = fc2;
                a[i * 6 + 5] = fc2 - v * 0.2f - vT * 0.2f;
            }

            b.SetData(a);
            return b;
        }

        static Vector3 ClampFieldLength(Vector3 field, float len) {
            var m = field.magnitude;
            return m > len ? field * len / m : field;
        }
    }
}
