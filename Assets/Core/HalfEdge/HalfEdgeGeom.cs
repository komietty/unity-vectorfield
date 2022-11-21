using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;

namespace ddg {
    using static Unity.Mathematics.math;
    public class HalfEdgeGeom: HalfEdgeMesh {

        public HalfEdgeGeom(UnityEngine.Mesh mesh) : base(mesh) { }

        public float3 Vector(HalfEdge h) {
            return Pos[h.next.vid] - Pos[h.vid];
        }

        public float Length(HalfEdge h) {
            return length(Vector(h));
        }

        public float Cotan(HalfEdge h){
            var p = Vector(h.prev);
            var n = Vector(h.next) * -1;
            return dot(p, n) / length(cross(p, n));
        }

        public Vector3 Centroid(Face f){
            var h = halfedges[f.hid];
            var a = Pos[h.vid];
            var b = Pos[h.next.vid];
            var c = Pos[h.prev.vid];
            if(h.onBoundary) return (a + b) / 2;
            return (a + b + c) / 3;
        }

        public float Area(Face f) {
            var h = halfedges[f.hid];
            if (h.onBoundary) return 0f;
            var u = Vector(h);
            var v = Vector(h.prev) * -1;
            return length(cross(u, v)) * 0.5f;
        }
        
        public float TotalArea(){
            var sum = 0f;
            foreach (var f in this.Faces) sum += Area(f);
            return sum;
        }

        public bool FaceNormal(Face f, out float3 o){
            o = new float3();
            var h = halfedges[f.hid];
            if(h.onBoundary) return false;
            var u = Vector(h);
            var v = Vector(h.prev) * -1;
            o = normalize(cross(u, v));
            return true;
        }

        public float Angle(Corner c) {
            var v1 = normalize(Vector(halfedges[c.hid].next));
            var v2 = normalize(Vector(halfedges[c.hid].prev)) * -1;
            return acos(dot(v1, v2));
        }

        public float DihedralAngle(HalfEdge h) {
            FaceNormal(h.face, out float3 n_ijk);
            FaceNormal(h.twin.face, out float3 n_jil);
            var v = Vector(h) / Length(h);
            var c = cross(n_ijk, n_jil);
            var d = dot(n_ijk, n_jil);
            return atan2(dot(v, c), d);
        }

        public float AngleDefect(Vert v) {
            var o = PI * 2;
            foreach (var c in GetAdjacentConers(v)) o -= Angle(c);
            return o;
        }

        public float BarycentricDualArea(Vert v) {
            var o = 0f;
            foreach (var f in GetAdjacentFaces(v)) { o += Area(f); }
            return o / 3;
        } 

        public float CircumcentricDualArea(Vert v) {
            var sum = 0f;
            foreach (var h in GetAdjacentHalfedges(v)) {
                var v0 = Cotan(h);
                var v1 = Cotan(h.prev);
                var l0 = lengthsq(Vector(h));
                var l1 = lengthsq( Vector(h.prev));
                sum += v0 * l0 + v1 * l1;
            }
            return sum * 0.125f;
        }

        public float ScalarGaussCurvature(Vert v) {
            return AngleDefect(v);
        }

        public float ScalarMeanCurvature(Vert v) {
            var o = 0f;
            foreach (var h in GetAdjacentHalfedges(v))
                o += DihedralAngle(h) * Length(h);
            return o * 0.5f;
        }
        
        public float2 PrincipalCurvature(Vert v) {
            var A = CircumcentricDualArea(v);
            var H = ScalarMeanCurvature(v) / A;
            var K = ScalarGaussCurvature(v) / A;
            var D = sqrt(H * H - K);
            return new float2(H - D, H + D);
        } 

        public IEnumerable<Face> GetAdjacentFaces(Vert v) {
            var tgt = halfedges[v.hid];
            while(tgt.onBoundary){ tgt = tgt.twin.next; }
            var curr = tgt;
            var endId = tgt.id;
            var once = false;
            while (true) {
                while (curr.onBoundary) { curr = curr.twin.next; }
                if (once && curr.id == tgt.id) break;
                once = true;
                yield return curr.next.face;
                curr = curr.twin.next;
            };
        }

        public IEnumerable<Corner> GetAdjacentConers(Vert v) {
            var tgt = halfedges[v.hid];
            while(tgt.onBoundary){ tgt = tgt.twin.next; }
            var curr = tgt;
            var endId = tgt.id;
            var once = false;
            while (true) {
                while (curr.onBoundary) { curr = curr.twin.next; }
                if (once && curr.id == tgt.id) break;
                once = true;
                yield return curr.next.corner;
                curr = curr.twin.next;
            };
        }

        public IEnumerable<HalfEdge> GetAdjacentHalfedges(Vert v) {
            var curr = halfedges[v.hid];
            var endId = curr.id;
            var once = false;
            while (true) {
                if (once && curr.id == endId) break;
                yield return curr;
                curr = curr.twin.next;
                once = true;
            };
        }

        public IEnumerable<HalfEdge> GetAdjacentHalfedges(Face f) {
            var curr = halfedges[f.hid];
            var endId = curr.id;
            var once = false;
            while (true) {
                if (once && curr.id == endId) break;
                yield return curr;
                curr = curr.twin.next;
                once = true;
            };
        }
    }
}