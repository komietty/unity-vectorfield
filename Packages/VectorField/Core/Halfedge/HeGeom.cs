using Unity.Mathematics;
using System.Collections.Generic;
using System;
using System.Linq;

namespace VectorField {
    using M = UnityEngine.Mesh;
    using T = UnityEngine.Transform;
    using f2 = float2;
    using f3 = float3;
    using static Unity.Mathematics.math;

    public class HeGeom: HeComp {
        public System.Span<f3> Pos => pos.AsSpan();
        public System.Span<f3> Nrm => nrm.AsSpan();
        f3[] pos;
        f3[] nrm;

        public HeGeom(M mesh, T trs) : base(mesh) {
            pos = mesh.vertices.Select(v => (f3)trs.TransformPoint(v)).ToArray();
            nrm = mesh.normals.Select(n => (f3)trs.TransformDirection(n)).ToArray();
        }

        public f3 Vector(HalfEdge h) {
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

        public f3 Centroid(Face f){
            var h = halfedges[f.hid];
            var a = Pos[h.vid];
            var b = Pos[h.next.vid];
            var c = Pos[h.prev.vid];
            if(h.onBoundary) return (a + b) / 2;
            return (a + b + c) / 3;
        }

        public (f3, f3) OrthonormalBasis(Face f) {
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

        public (bool b, f3 n) FaceNormal(Face f){
            var n = new f3();
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
        
        public f2 PrincipalCurvature(Vert v) {
            var A = CircumcentricDualArea(v);
            var H = ScalarMeanCurvature(v) / A;
            var K = ScalarGaussCurvature(v) / A;
            var D = sqrt(H * H - K);
            return new f2(H - D, H + D);
        } 

        public IEnumerable<Vert> GetAdjacentVerts(Vert v) {
            var curr = halfedges[v.hid];
            var endId = curr.id;
            var once = false;
            while (true) {
                if (once && curr.id == endId) break;
                yield return Verts[curr.next.vid];
                curr = curr.twin.next;
                once = true;
            };
        }

        public IEnumerable<Face> GetAdjacentFaces(Vert v) {
            var tgt = halfedges[v.hid];
            while (tgt.onBoundary) { tgt = tgt.twin.next; }
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
            while (tgt.onBoundary) tgt = tgt.twin.next;
            var curr = tgt;
            var endId = tgt.id;
            var once = false;
            while (true) {
                while (curr.onBoundary) curr = curr.twin.next;
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
                curr = curr.next;
                once = true;
            };
        }
    }
}