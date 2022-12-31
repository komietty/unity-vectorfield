using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ddg;
using Unity.Mathematics;

public class RibbonViewer : TangentBundle {
    [SerializeField] protected GameObject arrow;
    float3[] tangentFields;
    Face face;
    HalfEdge curr;
    float rate; 

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

        //tangentFields = new float3[geom.nFaces];
        //var tmp = new float3[geom.nFaces];
        //for (var i = 0; i < geom.nFaces; i++) {
        //    tangentFields[i] = new float3(0, 0, -1);
        //    tmp[i] = new float3(0, 0, 0);
        //}
        //UpdateTng(tmp);

        //Draw();
        face = geom.Faces[1];
        curr = geom.halfedges[face.hid];
        rate = 0.5f;
    }


    void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            var tng = tangentFields[face.fid];
            var g1 = GameObject.Instantiate(arrow);
            g1.transform.position = (geom.Pos[curr.next.vid] - geom.Pos[curr.vid]) * rate + geom.Pos[curr.vid];
            g1.transform.LookAt(g1.transform.position + (Vector3)tng);
            g1.transform.localScale *= 0.02f;

            var g2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g2.transform.position = geom.Centroid(face);
            g2.transform.localScale *= 0.02f;

            var (hid, r) = TangentTracer.CrossHe(tng, face, curr, rate, geom);
            r = Mathf.Min(r, 0.95f);
            r = Mathf.Max(r, 0.05f);
            curr = geom.halfedges[hid].twin;
            face = curr.face;
            rate = 1f - r;

        }
    }

    void Draw() {
        for(var i = 0; i < 60; i++) {
            var tng = tangentFields[face.fid];
            var g1 = GameObject.Instantiate(arrow);
            g1.transform.position = (geom.Pos[curr.next.vid] - geom.Pos[curr.vid]) * rate + geom.Pos[curr.vid];
            g1.transform.LookAt(g1.transform.position + (Vector3)tng);
            g1.transform.localScale *= 0.02f;
            var (hid, r) = TangentTracer.CrossHe(tng, face, curr, rate, geom);
            curr = geom.halfedges[hid].twin;
            face = curr.face;
            rate = 1f - r;
            if(curr.onBoundary) break;
        }
    }
}
