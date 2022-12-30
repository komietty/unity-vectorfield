using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ddg;
using Unity.Mathematics;

public class RibbonViewer : TangentBundle {
    [SerializeField] protected GameObject arrow;
    float3[] tangentFields;

    protected override void Start() {
        base.Start();
        var t = new TrivialConnection(geom);
        var s = new float[geom.nVerts];
        for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
        s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
        s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
        tangentFields = t.GenField(t.ComputeConnections(s));
        UpdateTng(tangentFields);
        tngtMat.SetFloat("_C", 1);
        Draw();
    }

    void Draw() {
        var face = geom.Faces[0];
        var curr = geom.halfedges[face.hid];
        var rate = 0.2f;
        for(var i = 0; i < 30; i++) {
            var tng = tangentFields[face.fid];
            var g1 = GameObject.Instantiate(arrow);
            g1.transform.position = (geom.Pos[curr.next.vid] - geom.Pos[curr.vid]) * rate + geom.Pos[curr.vid];
            g1.transform.LookAt(g1.transform.position + (Vector3)tng);
            g1.transform.localScale *= 0.02f;
            var (h, r) = TangentTracer.CrossHe(tng, face, curr, rate, geom);
            var g2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g2.transform.position = (geom.Pos[h.next.vid] - geom.Pos[h.vid]) * r + geom.Pos[h.vid];
            g2.transform.localScale *= 0.02f;
            face = h.twin.face;
            curr = h.twin;
            rate = 1f - r;
        }
    }
}
