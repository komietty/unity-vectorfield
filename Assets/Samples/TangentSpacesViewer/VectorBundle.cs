using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static UnityEngine.GraphicsBuffer;

namespace VectorField {

    public class VectorBundle {
        protected HeGeom geom;
        protected float meanLength; 

        public VectorBundle(HeGeom geom) {
            this.geom = geom;
            this.meanLength = geom.MeanEdgeLength();
        }

        public GraphicsBuffer GenVertTangeSpaces() {
            var l = geom.nVerts;
            var b = new GraphicsBuffer(Target.Structured, l * 6, 12);
            var a = new Vector3[l * 6]; 
            for (var i = 0; i < l; i++) {
                var n = geom.Nrm[i];
                var p = geom.Pos[i] + n * meanLength * 0.01f;
                var v = geom.Vector(geom.halfedges[geom.Verts[i].hid]);
                var ta = normalize(v - dot(v, n));
                var tb = cross(n, ta);
                a[i * 6 + 0] = p;
                a[i * 6 + 2] = p;
                a[i * 6 + 4] = p;
                a[i * 6 + 1] = p + ta * meanLength * 0.3f;
                a[i * 6 + 3] = p + tb * meanLength * 0.3f;
                a[i * 6 + 5] = p + n  * meanLength * 0.3f;
            }
            b.SetData(a);
            return b;
        }

        public GraphicsBuffer GenFaceTangeSpaces() {
            var l = geom.nFaces;
            var b = new GraphicsBuffer(Target.Structured, l * 6, 12);
            var a = new Vector3[l * 6]; 
            for (var i = 0; i < l; i++) {
                var f = geom.Faces[i];
                var n = geom.FaceNormal(f).n;
                var p = geom.Centroid(f) + n * meanLength * 0.01f;
                var (ta, tb) = geom.OrthonormalBasis(f);
                a[i * 6 + 0] = p;
                a[i * 6 + 2] = p;
                a[i * 6 + 4] = p;
                a[i * 6 + 1] = p + ta * meanLength * 0.3f;
                a[i * 6 + 3] = p + tb * meanLength * 0.3f;
                a[i * 6 + 5] = p + n  * meanLength * 0.3f;
            }
            b.SetData(a);
            return b;
        }

        public GraphicsBuffer GenVertTangentArrows(float3[] omega) {
            throw new System.Exception();
        }

        public GraphicsBuffer GenFaceTangentArrows(float3[] omega) {
            var l = geom.nFaces;
            var b = new GraphicsBuffer(Target.Structured, l * 6, 12);
            var a = new Vector3[l * 6];
            for(var i = 0; i < l; i++){
                var f = geom.Faces[i];
                var n = geom.FaceNormal(f).n;
                var p = geom.Centroid(f) + n * meanLength * 0.01f;
                var field = (float3)ClampFieldLength(omega[i], 1) * meanLength * 0.3f;
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

        protected Vector3 ClampFieldLength(Vector3 field, float len) {
            var m = field.magnitude;
            return m > len ? field * len / m : field;
        }
    }

    public struct Complex {
        float magnitude;
        float radius;
    }
}
