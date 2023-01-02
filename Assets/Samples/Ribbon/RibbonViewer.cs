using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VFD;

public class RibbonViewer : TangentBundle {
    [SerializeField] protected Material tmp;
    [SerializeField] protected int tracerNum;
    [SerializeField] protected int tracerLen;
    TangentTracer tracer;
    GraphicsBuffer posBuff;
    GraphicsBuffer colBuff;

    List<Vector3> tracerlist = new List<Vector3>();
    List<Vector3> colourlist = new List<Vector3>();

    protected override void Start() {
        base.Start();

        var t = new TrivialConnection(geom, new HodgeDecomposition(geom));
        var s = new float[geom.nVerts];
        for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
        s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
        s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
        var tngs = t.GenField(t.ComputeConnections(s));

        //var h = new HodgeDecomposition(geom);
        //var (omega, sids, vids) = TangentField.GenRandomOneForm(geom);
        //var exact    = h.Exact(omega);
        //var coexact  = h.CoExact(omega);
        //var tngs = TangentField.InterpolateWhitney(exact, geom);

        tracer = new TangentTracer(geom, tngs, tracerLen);
        UpdateTng(tngs);

        for (var i = 0; i < tracerNum; i++) {
            var f = geom.Faces[Random.Range(0, geom.nFaces)];
            var tr = tracer.GenTracer(f);
            var c = (Vector4)Color.HSVToRGB(0.6f + (i % 10) * 0.1f * 0.1f, Random.Range(0.5f, 1f), 1);
            for (var j = 0; j < tr.Count - 1; j++) {
                tracerlist.Add(tr[j]);
                tracerlist.Add(tr[j + 1]);
                colourlist.Add(c);
                colourlist.Add(c);
            }
        }
        posBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tracerlist.Count, sizeof(float) * 3);
        colBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tracerlist.Count, sizeof(float) * 3);
        posBuff.SetData(tracerlist);
        colBuff.SetData(colourlist);
        tmp.SetBuffer("_Line", posBuff);
        tmp.SetBuffer("_Col",  colBuff);
    }

    protected override void OnRenderObject() {
        base.OnRenderObject();
        tmp.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Lines, tracerlist.Count);
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        posBuff.Dispose();
        colBuff.Dispose();
    }
}
