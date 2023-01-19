using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace VectorField.Demo {
    using V = Vector<double>;

    public class ScalarHeatMethodViewer : MonoBehaviour {

        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            var mesh = HeComp.Weld(filt.sharedMesh);
            var geom = new HeGeom(mesh, transform);

            var s = new double[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
            s[Random.Range(0, geom.nVerts)] = 1;
            s[Random.Range(0, geom.nVerts)] = 1;

            var hm = new ScalarHeatMethod(geom); 
            var hd = hm.Compute(V.Build.DenseOfArray(s)).ToArray();
            foreach(var val in hd) {
                Debug.Log(val);
            }
        }
    }
}
