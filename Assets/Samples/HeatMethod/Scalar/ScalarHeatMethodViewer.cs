using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;

namespace VectorField.Demo {
    using V = Vector<double>;

    public class ScalarHeatMethodViewer : MonoBehaviour {

        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            var mesh = HeComp.Weld(filt.sharedMesh);
            var geom = new HeGeom(mesh, transform);
            filt.sharedMesh = mesh;

            var s = new double[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
            s[0] = 1;
            //s[Random.Range(0, geom.nVerts)] = 1;

            var hm = new ScalarHeatMethod(geom); 
            var hd = hm.Compute(V.Build.DenseOfArray(s)).ToArray();
            var vals = new Color[geom.nVerts];
            var max = 0.0;
            foreach(var v in geom.Verts) {
                var i = v.vid;
                max = math.max(max, hd[i]);
            }
            foreach(var v in geom.Verts) {
                var i = v.vid;
                vals[i] = new Color((float)math.cos(max - hd[i] * 1000), 1, 1, 1);
            }
            mesh.colors = vals;
        }
    }
}
