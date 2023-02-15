using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  VectorField {
    public class GeomContainer : MonoBehaviour {
        [SerializeField] protected Material tracer;
        public HeGeom geom { get; private set; }
        public Mesh mesh { get; private set; }

        void Awake() {
            var f = GetComponentInChildren<MeshFilter>();
            mesh = HeComp.Weld(f.sharedMesh);
            geom = new HeGeom(mesh, transform);
            f.sharedMesh = mesh;
        }
    }
}
