using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ddg {
    public class HalfEdgeGeom {
        public HalfEdgeMesh mesh;
        public HalfEdge[] halfedges => mesh.halfedges;

        public HalfEdgeGeom(Mesh mesh) {
            this.mesh = new HalfEdgeMesh(mesh);
        }

        public Vector3 Vector(HalfEdge h) {
            return mesh.verts[h.next.vid].pos - mesh.verts[h.vid].pos;
        }

        public float Length(HalfEdge h) {
            return Vector(h).magnitude;
        }

        public float Cotan(HalfEdge h){
            var p = Vector(h.prev);
            var n = Vector(h.next) * -1;
            return Vector3.Dot(p, n) / Vector3.Cross(p, n).magnitude;
        }

        float Area(Face f) {
            var h = halfedges[f.hid];
            if (h.onBoundary) return 0f;
            var u = Vector(h);
            var v = Vector(h.prev) * -1;
            return Vector3.Cross(u, v).magnitude * 0.5f;
        }

        public bool FaceNormal(Face f, out Vector3 o){
            o = new Vector3();
            var h = halfedges[f.hid];
            if(h.onBoundary) return false;
            var u = Vector(h);
            var v = Vector(h.prev) * -1;
            o = Vector3.Cross(u, v).normalized;
            return true;
        }

        public float Angle(Corner c) {
            var v1 = Vector(halfedges[c.hid].next).normalized;
            var v2 = Vector(halfedges[c.hid].prev).normalized * -1;
            return Mathf.Acos(Vector3.Dot(v1, v2));
        }

        public float DihedralAngle(HalfEdge h) {
            FaceNormal(h.face, out Vector3 n_ijk);
            FaceNormal(h.twin.face, out Vector3 n_jil);
            var vec = Vector(h) / Length(h);
            var crs = Vector3.Cross(n_ijk, n_jil);
            var dot = Vector3.Dot(n_ijk, n_jil);
            return Mathf.Atan2(Vector3.Dot(vec, crs), dot);
        }

        public float AngleDefect(Vert v) {
            var o = Mathf.PI * 2;
            foreach (var c in v.GetAdjacentConers(halfedges)) o -= Angle(c);
            return o;
        }

        public float BarycentricDualArea(Vert v) {
            var o = 0f;
            foreach (var f in v.GetAdjacentFaces(halfedges)) { o += Area(f); }
            return o / 3;
        } 

        public float CircumcentricDualArea(Vert v) {
            var sum = 0f;
            foreach (var h in v.GetAdjacentHalfedges(halfedges)) {
                var v0 = Cotan(h);
                var v1 = Cotan(h.prev);
                var l0 = Vector(h).sqrMagnitude;
                var l1 = Vector(h.prev).sqrMagnitude;
                sum += v0 * l0 + v1 * l1;
            }
            return sum * 0.125f;
        }

        public float ScalarGaussCurvature(Vert v) {
            return AngleDefect(v);
        }

        public float ScalarMeanCurvature(Vert v) {
            var o = 0f;
            foreach (var h in v.GetAdjacentHalfedges(halfedges))
                o += DihedralAngle(h) * Length(h);
            return o * 0.5f;
        }
        
        public Vector2 PrincipalCurvature(Vert v) {
            var A = CircumcentricDualArea(v);
            var H = ScalarMeanCurvature(v) / A;
            var K = ScalarGaussCurvature(v) / A;
            var D = Mathf.Sqrt(H * H - K);
            return new Vector2(H - D, H + D);
        } 
    }
}