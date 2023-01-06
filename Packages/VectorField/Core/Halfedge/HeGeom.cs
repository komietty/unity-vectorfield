using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace VectorField {
    public class HeGeom: HeComp {
        public System.Span<Vector3> Pos => pos.AsSpan();
        public System.Span<Vector3> Nrm => nrm.AsSpan();
        public Vector3[] pos;
        public Vector3[] nrm;

        public HeGeom(UnityEngine.Mesh mesh, Transform trs) : base(mesh) {
            pos = mesh.vertices.Select(v => trs.TransformPoint(v)).ToArray();
            nrm = mesh.normals.Select(n => trs.TransformDirection(n)).ToArray();
        }

        public float3 Vector(HalfEdge h) {
            return Pos[h.next.vid] - Pos[h.vid];
        }

        public float Length(HalfEdge h) {
            return length(Vector(h));
        }

        public float Length(Edge e) {
            return length(Vector(halfedges[e.hid]));
        }

        public float Cotan(HalfEdge h){
            var p = Vector(h.prev);
            var n = Vector(h.next) * -1;
            return dot(p, n) / length(cross(p, n));
        }

        public float3 Centroid(Face f){
            var h = halfedges[f.hid];
            var a = Pos[h.vid];
            var b = Pos[h.next.vid];
            var c = Pos[h.prev.vid];
            if(h.onBoundary) return (a + b) / 2;
            return (a + b + c) / 3;
        }

        public (float3, float3) OrthonormalBasis(Face f) {
            var e1 = normalize(Vector(halfedges[f.hid]));
            var e2 = cross(FaceNormal(f).n, e1);
            return (e1, e2);
        }

        public double Area(Face f) {
            var h = halfedges[f.hid];
            if (h.onBoundary) return 0;
            var u = Vector(h);
            var v = Vector(h.prev) * -1;
            return length(cross(u, v)) * 0.5;
        }
        
        public double TotalArea(){
            var sum = 0.0;
            foreach (var f in this.Faces) sum += Area(f);
            return sum;
        }

        public float MeanEdgeLength(){
            var sum = 0f;
            foreach (var e in this.Edges) sum += Length(e);
            return sum / this.nEdges;
        }

        public (bool b, float3 n) FaceNormal(Face f){
            var n = new float3();
            var h = halfedges[f.hid];
            if(h.onBoundary) return (false, n);
            var u = Vector(h);
            var v = Vector(h.prev) * -1;
            n = normalize(cross(u, v));
            return (true, n);
        }

        public float Angle(Corner c) {
            var v1 = normalize(Vector(halfedges[c.hid].next));
            var v2 = normalize(Vector(halfedges[c.hid].prev)) * -1;
            return acos(dot(v1, v2));
        }

        public float DihedralAngle(HalfEdge h) {
            var (_, n_ijk) = FaceNormal(h.face);
            var (_, n_jil) = FaceNormal(h.twin.face);
            var v = Vector(h) / Length(h);
            var c = cross(n_ijk, n_jil);
            var d = dot(n_ijk, n_jil);
            return atan2(dot(v, c), d);
        }

        public float AngleDefect(Vert v) {
            var sum = PI * 2;
            foreach (var c in GetAdjacentConers(v)) sum -= Angle(c);
            return sum;
        }

        public double BarycentricDualArea(Vert v) {
            var sum = 0.0;
            foreach (var f in GetAdjacentFaces(v)) sum += Area(f);
            return sum / 3.0;
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

        public IEnumerable<Vert> GetAdjacentVerts(Vert v) {
            var curr = halfedges[v.hid];
            var goal = curr;
            var once = false;
            while (true) {
                if (once && curr == goal) break;
                yield return Verts[curr.next.vid];
                curr = curr.twin.next;
                once = true;
            };
        }

        public IEnumerable<Face> GetAdjacentFaces(Vert v) {
            var goal = halfedges[v.hid];
            while (goal.onBoundary) { goal = goal.twin.next; }
            var curr = goal;
            var once = false;
            while (true) {
                while (curr.onBoundary) { curr = curr.twin.next; }
                if (once && curr == goal) break;
                once = true;
                yield return curr.next.face;
                curr = curr.twin.next;
            };
        }

        public IEnumerable<Corner> GetAdjacentConers(Vert v) {
            var goal = halfedges[v.hid];
            while (goal.onBoundary) goal = goal.twin.next;
            var curr = goal;
            var once = false;
            while (true) {
                while (curr.onBoundary) curr = curr.twin.next;
                if (once && curr == goal) break;
                once = true;
                yield return curr.next.corner;
                curr = curr.twin.next;
            };
        }

        public IEnumerable<HalfEdge> GetAdjacentHalfedges(Vert v) {
            var curr = halfedges[v.hid];
            var goal = curr;
            var once = false;
            while (true) {
                if (once && curr == goal) break;
                yield return curr;
                curr = curr.twin.next;
                once = true;
            };
        }

        public IEnumerable<HalfEdge> GetAdjacentHalfedges(Face f) {
            var curr = halfedges[f.hid];
            var goal = curr;
            var once = false;
            while (true) {
                if (once && curr == goal) break;
                yield return curr;
                curr = curr.next;
                once = true;
            };
        }
    }
}