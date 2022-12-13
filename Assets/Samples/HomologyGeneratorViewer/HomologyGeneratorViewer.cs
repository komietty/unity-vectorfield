using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ddg {
    public class HomologyGeneratorViewer : MonoBehaviour {
        protected HeGeom geom;
        protected HalfEdge[] gens;
        protected GraphicsBuffer treeBuf;
            

        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var mesh = MeshUtils.Weld(filt.sharedMesh);
            geom = new HeGeom(mesh);
            var h = new Homology(geom);
            h.BuildPrimalSpanningTree();
            h.BuildDualSpanningCotree();
            var tree = h.vertParentList;
            var cotr = h.faceParentList;
            var tarr = new List<Vector3>(tree.Count * 2);
            var carr = new List<Vector3>(cotr.Count * 2);
            //treeBuf = new GraphicsBuffer();

            foreach(var item in tree) {
                var v1 = item.Key;
                var v2 = item.Value;
                tarr.Add(geom.Pos[v1]);
                tarr.Add(geom.Pos[v2]);
            }

            foreach(var item in cotr) {
                var fid1 = item.Key;
                var fid2 = item.Value;
                Debug.Log(fid1);
                Debug.Log(fid2);
                Debug.Log("-----");
            }
        }
    }
}
