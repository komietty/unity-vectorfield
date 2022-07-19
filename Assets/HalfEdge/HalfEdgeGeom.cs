using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ddg {
    public class HalfEdgeGeom {
        public HalfEdgeMesh mesh;
        public HalfEdge[] halfedges => mesh.halfedges;

        public HalfEdgeGeom(Mesh mesh) {
            this.mesh = new HalfEdgeMesh(mesh);
        }

        public float Length(HalfEdge h) {
            //return h.Vector().magnitude;
            return Mathf.Sqrt(Vector3.Dot(h.Vector(), h.Vector()));
        }

        public bool FaceNormal(Face f, out Vector3 o){
            o = new Vector3();
            var h = halfedges[f.hid];
            if(h.onBoundary) return false;
            var u = h.Vector();
            var v = h.prev.Vector() * -1;
            o = Vector3.Cross(u, v).normalized;
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
            var vec = h.Vector() / Length(h);
            var crs = Vector3.Cross(n_ijk, n_jil);
            var dot = Vector3.Dot(n_ijk, n_jil);
            var ang = Mathf.Atan2(Vector3.Dot(vec, crs), dot);

/*
                Debug.Log(n_ijk + ", " + n_jil);
                Debug.Log(vec);
                Debug.Log(ang * 180 / Mathf.PI);
                var _crs = Vector3.Cross(n_jil, n_ijk);
                var _dot = Vector3.Dot(n_jil, n_ijk);
                var _ang = Mathf.Atan2(Vector3.Dot(vec, _crs), _dot);
                Assert.IsTrue(Mathf.Abs(ang) == Mathf.Abs(_ang));
                Debug.Log("-----------");

*/
            return ang;
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
                sum += DihedralAngle(h) * Length(h);
                //Debug.Log("d: " + DihedralAngle(h) + ", l: " + Length(h));
                //Debug.Log("v1: " + h.vert.pos);
                //Debug.Log("v2: " + h.next.vert.pos);
            }
            //Debug.Log("============" + sum * 0.5f);
            return sum * 0.5f;
        }
    }
}