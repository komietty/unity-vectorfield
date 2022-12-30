using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ddg;
using Unity.Mathematics;

public class RibbonViewer : MonoBehaviour {
    [SerializeField] protected GameObject arrow;
    HeGeom g;

    void Start() {
        g = new HeGeom(HeComp.Weld(GetComponent<MeshFilter>().sharedMesh));
        var f = g.Faces[0];
        var hInit = g.halfedges[f.hid];
        var t = new float3(0.1f, 1.0f, 0);
        //for(var i = 0; i < 100; i++) {
            var rate = 0.3f;
            var (h, r) = TangentTracer.CrossHe(t, f, hInit, rate, g);
            var g1 = GameObject.Instantiate(arrow);
            //g1.transform.position = g.Centroid(f);
            g1.transform.position = (g.Pos[hInit.next.vid] - g.Pos[hInit.vid]) * rate + g.Pos[hInit.vid];
            g1.transform.LookAt(g1.transform.position + (Vector3)t);
            g1.transform.localScale *= 0.02f;
            var g2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g2.transform.position = (g.Pos[h.next.vid] - g.Pos[h.vid]) * r + g.Pos[h.vid];
            g2.transform.localScale *= 0.02f;
            f = h.twin.face;
            Debug.Log(f.fid);
        //}
        //var ob = g.OrthonormalBasis(f);
        //var o1 = GameObject.Instantiate(arrow);
        //var o2 = GameObject.Instantiate(arrow);
        //o1.transform.position = g.Centroid(f);
        //o2.transform.position = g.Centroid(f);
        //o1.transform.localScale *= 0.05f;
        //o2.transform.localScale *= 0.05f;
        //o1.name = "o1";
        //o2.name = "o2";
        //o1.transform.LookAt(o1.transform.position + (Vector3)ob.Item1);
        //o2.transform.LookAt(o2.transform.position + (Vector3)ob.Item2);
    }
}
