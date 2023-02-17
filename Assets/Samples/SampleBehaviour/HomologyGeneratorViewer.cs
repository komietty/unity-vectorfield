using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System.Linq;

namespace VectorField {
    public class HomologyGeneratorViewer : MonoBehaviour {
        public enum Mode { tree, cotree, generator }

        [SerializeField] protected Material lineMat;
        [SerializeField] protected Mode mode;
        protected GraphicsBuffer treeBuf, cotrBuf, gensBuf;
        protected (int, int, int) buffLength;
            
        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var mesh = HeComp.Weld(filt.sharedMesh);
            var geom = new HeGeom(mesh, transform);
            var h = new HomologyGenerator(geom);
            var gens = h.BuildGenerators();
            var tree = h.vertParent;
            var cotr = h.faceParent;
            var gnum = gens.Select(g => g.Count).ToArray();
            var tarr = new List<Vector3>(tree.Length * 2);
            var carr = new List<Vector3>(cotr.Length * 2);
            var garr = new List<Vector3>(gnum.Sum() * 4);

            for (var i = 0; i < tree.Length; i++) {
                var v1 = i;
                var v2 = tree[i];
                tarr.Add(geom.Pos[v1]);
                tarr.Add(geom.Pos[v2]);
            }

            for (var i = 0; i < cotr.Length; i++) {
                var fid1 = i;
                var fid2 = cotr[i];
                carr.Add(geom.Centroid(geom.Faces[fid1]) + geom.FaceNormal(geom.Faces[fid1]).n * 0.01f);
                carr.Add(geom.Centroid(geom.Faces[fid2]) + geom.FaceNormal(geom.Faces[fid2]).n * 0.01f);
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

            buffLength = (tarr.Count, carr.Count, garr.Count);
            treeBuf = new GraphicsBuffer(Target.Structured, tree.Length * 2, sizeof(float) * 3);
            cotrBuf = new GraphicsBuffer(Target.Structured, cotr.Length * 2, sizeof(float) * 3);
            gensBuf = new GraphicsBuffer(Target.Structured, gnum.Sum() * 4,  sizeof(float) * 3);
            treeBuf.SetData(tarr);
            cotrBuf.SetData(carr);
            gensBuf.SetData(garr);
        }

        void OnRenderObject() {
            if (mode == Mode.tree) {
                lineMat.SetBuffer("_Line", treeBuf);
                lineMat.SetPass(0);
                Graphics.DrawProceduralNow(MeshTopology.Lines, buffLength.Item1);
            } else if (mode == Mode.cotree) {
                lineMat.SetBuffer("_Line", cotrBuf);
                lineMat.SetPass(0);
                Graphics.DrawProceduralNow(MeshTopology.Lines, buffLength.Item2);
            } else if (mode == Mode.generator) {
                lineMat.SetBuffer("_Line", gensBuf);
                lineMat.SetPass(0);
                Graphics.DrawProceduralNow(MeshTopology.Lines, buffLength.Item3);
            }

        }

        void OnDestroy() {
            treeBuf.Dispose();
            cotrBuf.Dispose();
            gensBuf.Dispose();
        }
    }
}
