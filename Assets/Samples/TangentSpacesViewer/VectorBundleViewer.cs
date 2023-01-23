using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorField {
    public class VectorBundleViewer : MonoBehaviour {
        public enum VectorSpaceType { VertSpace, FaceSpace }
        [SerializeField] protected Material vectorSpaceMat;
        [SerializeField] protected VectorSpaceType vectorSpaceType;
        protected GraphicsBuffer vertTangentSpaces;
        protected GraphicsBuffer faceTangentSpaces;
        protected HeGeom geom;

        void Start() {
            var fltr = GetComponentInChildren<MeshFilter>();
            var rndr = GetComponentInChildren<MeshRenderer>();
            geom = new HeGeom(HeComp.Weld(fltr.sharedMesh), transform);
            var bundle = new VectorBundle(geom);
            vertTangentSpaces = bundle.GenVertTangeSpaces(geom);
            faceTangentSpaces = bundle.GenFaceTangeSpaces(geom);
        }
    

        void OnRenderObject() {
            vectorSpaceMat.SetPass(0);

            if (vectorSpaceType == VectorSpaceType.VertSpace) {
                vectorSpaceMat.SetBuffer("_Lines", vertTangentSpaces);
                Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nVerts * 6);
            }
            if (vectorSpaceType == VectorSpaceType.FaceSpace) {
                vectorSpaceMat.SetBuffer("_Lines", faceTangentSpaces);
                Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nFaces * 6);
            }
        }

        void OnDestroy() {
            vertTangentSpaces.Dispose();
            faceTangentSpaces.Dispose();
        }
    }
}
