using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  VectorField {
    public class GeomContainer : MonoBehaviour {
        public HeGeom geom { get; private set; }

        void Awake() {
            var f = GetComponentInChildren<MeshFilter>();
            geom = new HeGeom(HeComp.Weld(f.sharedMesh), transform);
        }
    }
}
