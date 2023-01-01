using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ddg;
using System.Linq;

public class RibbonViewer : TangentBundle {
    [SerializeField] protected Material tmp;
    List<Vector3> tracerlist = new List<Vector3>();
    TangentTracer tracer;
    GraphicsBuffer posBuff;
    GraphicsBuffer tblBuff;
    int[] tblArr;

    protected override void Start() {
        base.Start();
        var t = new TrivialConnection(geom);
        var s = new float[geom.nVerts];
        for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
        s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
        s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
        var tngs = t.GenField(t.ComputeConnections(s));
        tracer = new TangentTracer(geom, tngs);
        UpdateTng(tngs);

        var count = 1000;
        tblArr = new int[count];

        for (var i = 0; i < count; i++) {
            var f = geom.Faces[Random.Range(0, geom.nFaces)];
            var tr = tracer.GenTracer(f);
            tblArr[i] = (tr.Count - 1) * 2;
            for (var j = 0; j < tr.Count - 1; j++) {
                tracerlist.Add(tr[j]);
                tracerlist.Add(tr[j + 1]);
            }
        }
        //var posArr = new Vector3[size];
        posBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tracerlist.Count, sizeof(float) * 3);
        posBuff.SetData(tracerlist);
        tmp.SetBuffer("_Line", posBuff);
        //tblBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, sizeof(int));
    }

    protected override void OnRenderObject() {
        base.OnRenderObject();
        tmp.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Lines, tracerlist.Count);
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        posBuff.Dispose();
    }
}
