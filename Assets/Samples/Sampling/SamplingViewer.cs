using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace VectorField {
    public class SamplingViewer : TangentBundle {
        [SerializeField] protected GameObject scale;

        protected override void Start() {
            base.Start();
            //var t = new TrivialConnection(geom, new HodgeDecomposition(geom));
            //var s = new float[geom.nVerts];
            //for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
            //s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            //s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            //var tf = t.GenField(t.ComputeConnections(s));

            var h = new HodgeDecomposition(geom);
            var (omega, sids, vids) = TangentField.GenRandomOneForm(geom);
            var exact = h.Exact(omega);
            var coexact = h.CoExact(omega);
            var tf = TangentField.InterpolateWhitney(exact, geom);

            UpdateTng(tf);
            for (var i = 0; i < tf.Length; i++) {
                var go = GameObject.Instantiate(scale);
                var f = geom.Faces[i];
                //go.transform.localScale *= 0.4f;
                go.transform.position = geom.Centroid(f);
                var q = Quaternion.LookRotation(tf[i] + geom.FaceNormal(f).n * 0.1f, geom.FaceNormal(f).n);
                go.transform.rotation = q * Quaternion.identity;
            }
        }
    }
}
