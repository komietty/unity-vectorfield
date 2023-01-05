using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VFD {
    public class SamplingViewer : TangentBundle {
        [SerializeField] protected GameObject scale;

        protected override void Start() {
            base.Start();
            var t = new TrivialConnection(geom, new HodgeDecomposition(geom));
            var s = new float[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
            s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            var tf = t.GenField(t.ComputeConnections(s));
            UpdateTng(tf);
            for (var i = 0; i < tf.Length; i++) {
                var go = GameObject.Instantiate(scale);
                var f = geom.Faces[i];
                go.transform.position = geom.Centroid(f) + geom.FaceNormal(f).n * 0.05f;
                go.transform.forward = tf[i];
                go.transform.up = geom.FaceNormal(f).n;
            }
        }
    }
}
