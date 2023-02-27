using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace VectorField {
    public class SamplingViewer : MonoBehaviour {

        /*
        void Start() {
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
            var hamonic = h.Harmonic(omega, exact, coexact);
            var tf = TangentField.InterpolateWhitney(hamonic, geom);

            UpdateTng(tf);
            for (var i = 0; i < tf.Length; i++) {
                var go = GameObject.Instantiate(scale);
                var f = geom.Faces[i];
                var t = math.normalize(tf[i]);
                var n = math.normalize(geom.FaceNormal(f).n);
                go.transform.localScale *= UnityEngine.Random.Range(0.034f, 0.036f);
                go.transform.position = geom.Centroid(f) + n * 0.01f + t * UnityEngine.Random.Range(0.0f, 0.01f);
                var q = Quaternion.LookRotation(t + n * UnityEngine.Random.Range(0.1f, 0.4f), n);
                go.transform.rotation = q * Quaternion.identity;
            }
        }
        */
    }
}
