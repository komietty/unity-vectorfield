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
            var mesh = MeshUtils.Weld(filt.sharedMesh);
            geom = new HeGeom(mesh);
            var h = new Homology(geom);
            var gens = h.BuildGenerators();
            var tree = h.vertParentList;
            var cotr = h.faceParentList;
            var gnum = gens.Select(g => g.Count).ToArray();
            tarr = new List<Vector3>(tree.Count * 2);
            carr = new List<Vector3>(cotr.Count * 2);
            garr = new List<Vector3>(gnum.Sum() * 4);

            foreach(var item in tree) {
                var v1 = item.Key;
                var v2 = item.Value;
                tarr.Add(geom.Pos[v1]);
                tarr.Add(geom.Pos[v2]);
            }

            foreach(var item in cotr) {
                var fid1 = item.Key;
                var fid2 = item.Value;
                carr.Add(geom.Centroid(geom.Faces[fid1]) + (Vector3)geom.FaceNormal(geom.Faces[fid1]).n * 0.02f);
                carr.Add(geom.Centroid(geom.Faces[fid2]) + (Vector3)geom.FaceNormal(geom.Faces[fid2]).n * 0.02f);
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

            treeBuf = new GraphicsBuffer(Target.Structured, tree.Count * 2, sizeof(float) * 3);
            cotrBuf = new GraphicsBuffer(Target.Structured, cotr.Count * 2, sizeof(float) * 3);
            gensBuf = new GraphicsBuffer(Target.Structured, gnum.Sum() * 4, sizeof(float) * 3);
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