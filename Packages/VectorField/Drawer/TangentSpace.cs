using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace VectorField {
    using RV = Vector<double>;

    public static class TangentSpace {

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
    }
}
