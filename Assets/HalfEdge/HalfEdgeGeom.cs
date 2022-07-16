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

        public float Angle(Corner c) {
            var v1 = halfedges[c.hid].next.Vector().normalized;
            var v2 = halfedges[c.hid].prev.Vector().normalized * -1;
            return Mathf.Acos(Vector3.Dot(v1, v2));
        }

        public float AngleDefect(Vert v) {
            var sum = Mathf.PI * 2;
            foreach (var c in v.GetAdjacentConers(halfedges)) { sum -= Angle(c); }
            return sum;
        }

        public float ScalarGaussCurvature(Vert v) => AngleDefect(v); 
    }
}