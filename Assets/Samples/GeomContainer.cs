using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace  VectorField {
    public class GeomContainer : MonoBehaviour {
        public enum SurfMode { blackBase, whiteBase, vertexColorBase }

        [SerializeField] protected Material lineMat;
        [SerializeField] protected Material arrowMat;
        public SurfMode surfMode;
        public bool showVertSpace;
        public bool showFaceSpace;
        public bool showVertArrow;
        public bool showFaceArrow;
        public bool showFaceRibbon;
        public HeGeom geom { get; private set; }
        public Mesh mesh { get; private set; }
        [HideInInspector] public Color[] vertexColors;
        public Material LineMat => lineMat;
        TangentRibbon faceRibbon;
        TangentVertArrow  vertArrow;
        TangentFaceArrow  faceArrow;
        Color arrowColor;
        bool flag;

        void OnValidate() {
            if (flag) SwitchSurface();
        }

        void Awake() {
            var f = GetComponentInChildren<MeshFilter>();
            mesh = HeComp.Weld(f.sharedMesh);
            geom = new HeGeom(mesh, transform);
            f.sharedMesh = mesh;
        }

        void Update() {
            if(!flag) SwitchSurface();
            flag = true;
        }

        void OnRenderObject() {
            if (showVertArrow && vertArrow != null) DrawArrows(vertArrow.buff, geom.nVerts);
            if (showFaceArrow && faceArrow != null) DrawArrows(faceArrow.buff, geom.nFaces);
            if (showFaceRibbon && faceRibbon != null) DrawRibbons();
        }

        void SwitchSurface() {
            switch (surfMode) {
                case SurfMode.blackBase:
                    arrowColor = Color.white;
                    mesh.SetColors(Enumerable.Repeat(Color.black, geom.nVerts).ToArray());
                    break;
                case SurfMode.whiteBase:
                    arrowColor = Color.black;
                    mesh.SetColors(Enumerable.Repeat(Color.white, geom.nVerts).ToArray());
                    break;
                case SurfMode.vertexColorBase:
                    arrowColor = Color.black;
                    mesh.SetColors(vertexColors);
                    break;
            }
        }

        public void PutSingularityPoint(int vid) {
            var o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            o.transform.position = geom.Pos[vid];
            o.transform.localScale *= geom.MeanEdgeLength() * 0.3f;
        }

        public void BuildVertArrowBuffer(float3[] vecs, bool clamp = true) {
            vertArrow?.Dispose();
            vertArrow = new TangentVertArrow(vecs, geom, clamp);
        }

        public void BuildFaceArrowBuffer(float3[] vecs, bool clamp = true) {
            faceArrow?.Dispose();
            faceArrow = new TangentFaceArrow(vecs, geom, clamp);
        }
        
        
        public void BuildRibbonBuffer(float3[] faceVector, Gradient colScheme) {
            faceRibbon?.Dispose();
            var n = math.min((int)(geom.nFaces * 0.5f), 2000);
            var l = math.min((int)(geom.nFaces * 0.1f), 400);
            faceRibbon = new TangentRibbon(faceVector, geom, n, l);
        }
        
        public void BuildFaceTangentSpaceBasisBuffer() { }
        public void BuildVertTangentSpaceBasisBuffer() { }
        
        void DrawTangentSpaceBasis(GraphicsBuffer buff, int count) {
            arrowMat.SetBuffer("_Lines", buff);
            arrowMat.SetVector("_Color", arrowColor);
            arrowMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, count * 6);
        }
        
        void DrawArrows(GraphicsBuffer buff, int count) {
            arrowMat.SetBuffer("_Lines", buff);
            arrowMat.SetVector("_Color", arrowColor);
            arrowMat.SetFloat("_T", Time.time);
            arrowMat.SetPass(1);
            Graphics.DrawProceduralNow(MeshTopology.Lines, count * 6);
        }
        
        void DrawRibbons() {
            lineMat.SetBuffer("_Line", faceRibbon.tracerBuff);
            lineMat.SetBuffer("_Norm", faceRibbon.normalBuff);
            lineMat.SetBuffer("_Col",  faceRibbon.colourBuff);
            lineMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, faceRibbon.nTracers);
        }

        void OnDestroy() {
            vertArrow?.Dispose();
            faceArrow?.Dispose();
            faceRibbon?.Dispose();
        }
    }
}
