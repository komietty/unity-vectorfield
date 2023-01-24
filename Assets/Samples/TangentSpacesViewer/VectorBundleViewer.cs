using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorField {
    public class VectorBundleViewer : MonoBehaviour {
        public enum VectorSpaceType { VertSpace, FaceSpace, VertArrow, FaceArrow }
        [SerializeField] protected Material vectorSpaceMat;
        [SerializeField] protected VectorSpaceType vectorSpaceType;
        protected GraphicsBuffer vertTangentSpaces;
        protected GraphicsBuffer faceTangentSpaces;
        protected GraphicsBuffer faceTangentArrows;
        protected HeGeom geom;

        void Start() {
            var fltr = GetComponentInChildren<MeshFilter>();
            var rndr = GetComponentInChildren<MeshRenderer>();
            geom = new HeGeom(HeComp.Weld(fltr.sharedMesh), transform);
            var bundle = new VectorBundle(geom);
            vertTangentSpaces = bundle.GenVertTangeSpaces();
            faceTangentSpaces = bundle.GenFaceTangeSpaces();

            var h = new HodgeDecomposition(geom);
            var (omega, sids, vids) = TangentField.GenRandomOneForm(geom);
            var exact = h.Exact(omega);
            var coexact = h.CoExact(omega);
            var hamonic = h.Harmonic(omega, exact, coexact);
            var tngs = TangentField.InterpolateWhitney(exact, geom);

            faceTangentArrows = bundle.GenFaceTangentArrows(tngs);
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
            if (vectorSpaceType == VectorSpaceType.FaceArrow) {
                // tmp _Lines -> _Line!
                vectorSpaceMat.SetBuffer("_Line", faceTangentArrows);
                Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nFaces * 6);
            }
        }

        void OnDestroy() {
            vertTangentSpaces?.Dispose();
            faceTangentSpaces?.Dispose();
            faceTangentArrows?.Dispose();
        }
    }
}
