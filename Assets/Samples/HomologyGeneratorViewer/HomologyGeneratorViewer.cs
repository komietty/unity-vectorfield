using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System.Linq;

namespace ddg {
    public class HomologyGeneratorViewer : MonoBehaviour {
        [SerializeField] protected Material treeMat;
        [SerializeField] protected Material cotrMat;
        [SerializeField] protected Material gensMat;
        protected HeGeom geom;
        protected HalfEdge[] gens;
        protected GraphicsBuffer treeBuf, cotrBuf, gensBuf;
        protected List<Vector3> tarr;
        protected List<Vector3> carr;
        protected List<Vector3> garr;
            
        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var mesh = HeComp.Weld(filt.sharedMesh);
            geom = new HeGeom(mesh);
            var h = new HomologyGenerator(geom);
            var gens = h.BuildGenerators();
            var tree = h.vertParent;
            var cotr = h.faceParent;
            var gnum = gens.Select(g => g.Count).ToArray();
            tarr = new List<Vector3>(tree.Length * 2);
            carr = new List<Vector3>(cotr.Length * 2);
            garr = new List<Vector3>(gnum.Sum() * 4);

            for (var i = 0; i < tree.Length; i++) {
                var v1 = i;
                var v2 = tree[i];
                tarr.Add(geom.Pos[v1]);
                tarr.Add(geom.Pos[v2]);
            }

            //foreach(var item in cotr) {
            for (var i = 0; i < cotr.Length; i++) {
                var fid1 = i;
                var fid2 = cotr[i];
                carr.Add(geom.Centroid(geom.Faces[fid1]) + geom.FaceNormal(geom.Faces[fid1]).n * 0.02f);
                carr.Add(geom.Centroid(geom.Faces[fid2]) + geom.FaceNormal(geom.Faces[fid2]).n * 0.02f);
            }

            foreach(var g in gens) {
            foreach(var he in g) {
                var c1 = geom.Centroid(he.face);
                var c2 = geom.Centroid(he.twin.face);
                var p1 = geom.Pos[he.vid];
                var p2 = geom.Pos[he.twin.vid];
                var m = (p1 + p2) * 0.5f;
                garr.Add(c1);
                garr.Add(m);
                garr.Add(m);
                garr.Add(c2);
            }
            }

            treeBuf = new GraphicsBuffer(Target.Structured, tree.Length * 2, sizeof(float) * 3);
            cotrBuf = new GraphicsBuffer(Target.Structured, cotr.Length * 2, sizeof(float) * 3);
            gensBuf = new GraphicsBuffer(Target.Structured, gnum.Sum() * 4,  sizeof(float) * 3);
            treeBuf.SetData(tarr);
            cotrBuf.SetData(carr);
            gensBuf.SetData(garr);
            treeMat.SetBuffer("_Line", treeBuf);
            cotrMat.SetBuffer("_Line", cotrBuf);
            gensMat.SetBuffer("_Line", gensBuf);
        }

        void OnRenderObject() {
            treeMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, tarr.Count);
            cotrMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, carr.Count);
            gensMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, garr.Count);
        }

        void OnDestroy() {
            treeBuf.Dispose();
            cotrBuf.Dispose();
            gensBuf.Dispose();
        }
    }
}
