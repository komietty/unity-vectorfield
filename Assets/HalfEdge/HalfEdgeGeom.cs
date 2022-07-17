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

        public float Length(Edge e) {
            return halfedges[e.hid].Vector().magnitude;
        }
        public bool FaceNormal(Face f, out Vector3 o){
            o = new Vector3();
            if(halfedges[f.hid].onBoundary) return false;
            var u = halfedges[f.hid].Vector();
            var v = halfedges[f.hid].prev.Vector() * -1;
            o = Vector3.Cross(v, u).normalized;
            return true;
        }

        public float Angle(Corner c) {
            var v1 = halfedges[c.hid].next.Vector().normalized;
            var v2 = halfedges[c.hid].prev.Vector().normalized * -1;
            return Mathf.Acos(Vector3.Dot(v1, v2));
        }

        public float DihedralAngle(HalfEdge h) {
            FaceNormal(h.face, out Vector3 n_ijk);
            FaceNormal(h.twin.face, out Vector3 n_jil);
            var vec = h.Vector() / Length(h.edge);
            var crs = Vector3.Cross(n_ijk, n_jil);
            var dot = Vector3.Dot(n_ijk, n_jil);
            return Mathf.Atan2(Vector3.Dot(vec, crs), dot);
        }

        public float AngleDefect(Vert v) {
            var sum = Mathf.PI * 2;
            foreach (var c in v.GetAdjacentConers(halfedges)) { sum -= Angle(c); }
            return sum;
        }

        public float ScalarGaussCurvature(Vert v) => AngleDefect(v); 

        public float ScalarMeanCurvature(Vert v) {
            var sum = 0f;
            foreach (var h in v.GetAdjacentHalfedges(halfedges)) {
                sum += DihedralAngle(h) * Length(h.edge);
            }
            return sum * 0.5f;
        }
    }
}