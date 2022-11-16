using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using ddg;

public class VectorFieldVisualizer : MonoBehaviour {
    [SerializeField] Material mat;
    Mesh mesh;
    GraphicsBuffer vrtsBuff;
    GraphicsBuffer nrmsBuff;

    void Start() {
        var m = GetComponentInChildren<MeshFilter>().sharedMesh;
        mesh = MeshUtils.Weld(m);
        mesh.RecalculateNormals();
        var vrts = mesh.vertices;
        var nrms = mesh.normals;
        vrtsBuff = new GraphicsBuffer(Target.Structured, vrts.Length, sizeof(float) * 3);
        nrmsBuff = new GraphicsBuffer(Target.Structured, nrms.Length, sizeof(float) * 3);
        vrtsBuff.SetData(vrts);
        nrmsBuff.SetData(nrms);
    }

    void Update() {
    }

    void OnRenderObject() {
        mat.SetBuffer("_VrtsBuff", vrtsBuff);
        mat.SetBuffer("_NrmsBuff", nrmsBuff);
        mat.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Quads, 4, mesh.vertexCount);
    }

    void OnDestroy() {
        vrtsBuff.Dispose();
        nrmsBuff.Dispose();
    }
}
