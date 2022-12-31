using UnityEngine;

namespace ddg {
    public class TrivialConnectionViewer : TangentBundle {

        protected override void Start() {
            base.Start();
            var t = new TrivialConnection(geom);
            var s = new float[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
            s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            UpdateTng(t.GenField(t.ComputeConnections(s)));
            tngtMat.SetFloat("_C", 1);
        }
    }
}