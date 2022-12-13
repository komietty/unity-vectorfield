using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace ddg {
    public class HomologyGeneratorViewer : MonoBehaviour {
        [SerializeField] protected Material treeMat;
        [SerializeField] protected Material cotrMat;
        protected HeGeom geom;
        protected HalfEdge[] gens;
        protected GraphicsBuffer treeBuf, cotrBuf;
        protected List<Vector3> tarr;
        protected List<Vector3> carr;
            

        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var mesh = MeshUtils.Weld(filt.sharedMesh);
            geom = new HeGeom(mesh);
            var h = new Homology(geom);
            h.BuildPrimalSpanningTree();
            h.BuildDualSpanningCotree();
            var tree = h.vertParentList;
            var cotr = h.faceParentList;
            tarr = new List<Vector3>(tree.Count * 2);
            carr = new List<Vector3>(cotr.Count * 2);

            foreach(var item in tree) {
                var v1 = item.Key;
                var v2 = item.Value;
                tarr.Add(geom.Pos[v1]);
                tarr.Add(geom.Pos[v2]);
            }

            foreach(var item in cotr) {
                var fid1 = item.Key;
                var fid2 = item.Value;
                carr.Add(geom.Centroid(geom.Faces[fid1]));
                carr.Add(geom.Centroid(geom.Faces[fid2]));
            }

            treeBuf = new GraphicsBuffer(Target.Structured, tree.Count * 2, sizeof(float) * 3);
            cotrBuf = new GraphicsBuffer(Target.Structured, cotr.Count * 2, sizeof(float) * 3);
            treeBuf.SetData(tarr);
            cotrBuf.SetData(carr);
            treeMat.SetBuffer("_Line", treeBuf);
            cotrMat.SetBuffer("_Line", cotrBuf);

        }

        void OnRenderObject() {
                treeMat.SetPass(0);
                Graphics.DrawProceduralNow(MeshTopology.Lines, tarr.Count);
                cotrMat.SetPass(0);
                Graphics.DrawProceduralNow(MeshTopology.Lines, carr.Count);
        }

        void OnDestroy() {
            treeBuf.Dispose();
            cotrBuf.Dispose();
        }
    }
}
