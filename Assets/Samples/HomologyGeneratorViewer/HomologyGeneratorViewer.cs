using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ddg {
    public class HomologyGeneratorViewer : MonoBehaviour {
        protected HeGeom geom;
        protected HalfEdge[] gens;
        protected GraphicsBuffer treeBuf;
            

        void Start() {
            var m = GetComponentInChildren<MeshFilter>().sharedMesh;
            geom = new HeGeom(m);
            var h = new Homology(geom);
            h.BuildPrimalSpanningTree();
            var tree = h.vertParentList;
            //var cotr = hmlg.faceParentList;
            //gens = hmlg.BuildGenerators();
            foreach(var item in tree) {
                var v1 = item.Key;
                var v2 = item.Value;
                var p1 = geom.Pos[v1.vid];
                var p2 = geom.Pos[v2.vid];
                Debug.Log(p1);
                Debug.Log(p1);
                Debug.Log("----");
            }
        }
    }
}
