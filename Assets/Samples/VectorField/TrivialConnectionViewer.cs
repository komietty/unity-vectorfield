using UnityEngine;

namespace ddg {
    public class TrivialConnectionViewer : TangentBundle {

        protected override void Start() {
            base.Start();
            var t = new TrivialConnection(geom);
            var singularites = new float[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) singularites[i] = 0;
            singularites[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            singularites[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            var m = t.ComputeConnections(singularites);
            UpdateTng(t.BuildDirectionalField(geom, m));
            tngtMat.SetFloat("_C", 1);
        }
    }
}