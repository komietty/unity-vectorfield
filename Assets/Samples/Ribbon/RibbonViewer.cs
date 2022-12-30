using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ddg;
using Unity.Mathematics;

public class RibbonViewer : MonoBehaviour {
    [SerializeField] protected GameObject arrow;
    HeGeom g;

    void Start() {
        g = new HeGeom(HeComp.Weld(GetComponent<MeshFilter>().sharedMesh), transform);
        var face = g.Faces[1];
        var curr = g.halfedges[face.hid];
        var rate = 0.8f;
        var tng = transform.TransformDirection(new float3(1f, -0f, -0.5f));
        for(var i = 0; i < 20; i++) {
            var (h, r) = TangentTracer.CrossHe(tng, face, curr, rate, g);
            var g1 = GameObject.Instantiate(arrow);
            g1.transform.position = (g.Pos[curr.next.vid] - g.Pos[curr.vid]) * rate + g.Pos[curr.vid];
            g1.transform.LookAt(g1.transform.position + (Vector3)tng);
            g1.transform.localScale *= 0.02f;
            var g2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g2.transform.position = (g.Pos[h.next.vid] - g.Pos[h.vid]) * r + g.Pos[h.vid];
            g2.transform.localScale *= 0.02f;
            face = h.twin.face;
            curr = h.twin;
            rate = 1f - r;
        }
    }
}
