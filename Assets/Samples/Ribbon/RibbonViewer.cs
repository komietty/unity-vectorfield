using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ddg;
using Unity.Mathematics;

public class RibbonViewer : TangentBundle {
    [SerializeField] protected GameObject arrow;
    [SerializeField] protected Material tmp;
    float3[] tangentFields;
    List<List<Vector3>> tracerlist = new List<List<Vector3>>();

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

        for(var i =0; i < 2000; i++) {
            var f = geom.Faces[UnityEngine.Random.Range(0, geom.nFaces)];
            var c = geom.halfedges[f.hid];
            DrawTrace(f, c, UnityEngine.Random.Range(0.1f, 0.9f));
        }
    }

    void DrawTrace(Face _face, HalfEdge _curr, float _rate) {
        var face = _face;
        var curr = _curr;
        var rate = _rate;
        var tr = new List<Vector3>();
        for(var i = 0; i < 100; i++) {
            var tng = tangentFields[face.fid];
            var pos = (geom.Pos[curr.next.vid] - geom.Pos[curr.vid]) * rate + geom.Pos[curr.vid];
            tr.Add(pos + (Vector3)geom.FaceNormal(face).n * 0.01f);
            //var g1 = GameObject.Instantiate(arrow);
            //g1.transform.position = (geom.Pos[curr.next.vid] - geom.Pos[curr.vid]) * rate + geom.Pos[curr.vid];
            //g1.transform.LookAt(g1.transform.position + (Vector3)tng);
            //g1.transform.localScale *= 0.02f;
            //var g2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //g2.transform.position = geom.Centroid(face);
            //g2.transform.localScale *= 0.01f;
            var (flg, hid, r) = TangentTracer.CrossHe(tng, face, curr, rate, geom);
            curr = geom.halfedges[hid].twin;
            face = curr.face;
            rate = 1f - r;
            if(!flg || curr.onBoundary) break;
        }
        tracerlist.Add(tr);
    }


    protected override void OnRenderObject() {
        base.OnRenderObject();

        for (int i = 0; i < tracerlist.Count; i++) {
            tmp.SetPass(0);
            tmp.SetColor("_Color", Color.HSVToRGB(0.17f + (i % 10) * 0.1f * 0.5f , 1, 1));
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.LINE_STRIP);
            var list = tracerlist[i];
            for (int j = 0; j < list.Count; j++) {
                GL.Vertex(list[j]);
            }
            GL.End();
            GL.PopMatrix();
        }
    }
}
