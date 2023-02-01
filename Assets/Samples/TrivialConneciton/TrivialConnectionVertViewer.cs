using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorField.Demo {
    public class TrivialConnectionVertViewer : MonoBehaviour {
    
        public enum VectorSpaceType { VertSpace, FaceSpace, VertArrow, FaceArrow }
        [SerializeField] protected Material vectorSpaceMat;
        [SerializeField] protected VectorSpaceType vectorSpaceType;
        protected GraphicsBuffer vertTangentSpaces;
        protected GraphicsBuffer vertTangentArrows;
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

            var hd = new HodgeDecomposition(geom);
            var o = TangentField.GenRandomOneForm(geom).oneForm;
            var e = hd.Exact(o);
            var c = hd.CoExact(o);
            var h = hd.Harmonic(o, e, c);
            var trivialConn = new TrivialConnection(geom, hd);
            var s = new float[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
            //s[Random.Range(0, geom.nVerts)] = 1;
            //s[Random.Range(0, geom.nVerts)] = 1;
            s[0] = 1;
            s[geom.nVerts - 1] = 1;
            var conn = trivialConn.ComputeConnections(s);
            Debug.Log(hd.A);

            var trivialConnVert = new TrivialConnectionVert(geom, hd); 
            faceTangentArrows = bundle.GenFaceTangentArrows(trivialConn.GenField(conn));
            vertTangentArrows = bundle.GenVertTangentArrows(trivialConnVert.GenField(conn));
        }
    

        void OnRenderObject() {
            if (vectorSpaceType == VectorSpaceType.VertSpace) {
                vectorSpaceMat.SetPass(0);
                vectorSpaceMat.SetBuffer("_Lines", vertTangentSpaces);
                Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nVerts * 6);
            }
            if (vectorSpaceType == VectorSpaceType.FaceSpace) {
                vectorSpaceMat.SetPass(0);
                vectorSpaceMat.SetBuffer("_Lines", faceTangentSpaces);
                Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nFaces * 6);
            }
            if (vectorSpaceType == VectorSpaceType.FaceArrow) {
                vectorSpaceMat.SetPass(1);
                vectorSpaceMat.SetBuffer("_Lines", faceTangentArrows);
                Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nFaces * 6);
            }
            if (vectorSpaceType == VectorSpaceType.VertArrow) {
                vectorSpaceMat.SetPass(1);
                vectorSpaceMat.SetBuffer("_Lines",vertTangentArrows);
                Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nVerts * 6);
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
