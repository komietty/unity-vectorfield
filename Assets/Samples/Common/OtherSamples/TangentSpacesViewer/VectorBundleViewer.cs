using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorField {
    public class VectorBundleViewer : MonoBehaviour {
        public enum VectorSpaceType { None, VertSpace, FaceSpace, VertArrow, FaceArrow }
        [SerializeField] protected Material vectorSpaceMat;
        [SerializeField] protected VectorSpaceType vectorSpaceType;
        protected GraphicsBuffer vertTangentSpaces;
        protected GraphicsBuffer vertTangentArrows;
        protected GraphicsBuffer faceTangentSpaces;
        protected GraphicsBuffer faceTangentArrows;
        protected HeGeom geom;

        void Start() {
            var fltr = GetComponentInChildren<MeshFilter>();
            geom = new HeGeom(HeComp.Weld(fltr.sharedMesh), transform);
            var bundle = new VectorBundle(geom);
            vertTangentSpaces = bundle.GenVertTangeSpaces();
            faceTangentSpaces = bundle.GenFaceTangeSpaces();

            var hd = new HodgeDecomposition(geom);
            var o = TangentField.GenRandomOneForm(geom).oneForm;
            var e = hd.Exact(o);
            var c = hd.CoExact(o);
            var h = hd.Harmonic(o, e, c);

            faceTangentArrows = bundle.GenFaceTangentArrows(TangentField.InterpolateWhitney(h, geom));
        }
    

        void OnRenderObject() {
            switch (vectorSpaceType) {
                case VectorSpaceType.VertSpace: 
                    vectorSpaceMat.SetBuffer("_Lines", vertTangentSpaces);
                    vectorSpaceMat.SetPass(0);
                    Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nVerts * 6);
                    break;
                case VectorSpaceType.FaceSpace:
                    vectorSpaceMat.SetBuffer("_Lines", faceTangentSpaces);
                    vectorSpaceMat.SetPass(0);
                    Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nFaces * 6);
                    break;
                case VectorSpaceType.FaceArrow:
                    vectorSpaceMat.SetBuffer("_Lines", faceTangentArrows);
                    vectorSpaceMat.SetPass(1);
                    Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nFaces * 6);
                    break;
                case VectorSpaceType.VertArrow: 
                    vectorSpaceMat.SetBuffer("_Lines",vertTangentArrows);
                    vectorSpaceMat.SetPass(1);
                    Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nVerts * 6);
                    break;
                case VectorSpaceType.None: break;
            }
        }

        void OnDestroy() {
            vertTangentSpaces?.Dispose();
            vertTangentArrows?.Dispose();
            faceTangentSpaces?.Dispose();
            faceTangentArrows?.Dispose();
        }
    }
}