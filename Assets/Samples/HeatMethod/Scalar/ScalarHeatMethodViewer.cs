using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;

namespace VectorField.Demo {
    using V = Vector<double>;

    public class ScalarHeatMethodViewer : MonoBehaviour {
        [SerializeField] protected Material tracerMat;
        [SerializeField] protected Gradient colScheme;
        GraphicsBuffer tracerBuff;
        GraphicsBuffer colourBuff;
        GraphicsBuffer normalBuff;
        List<Vector3> tracers = new List<Vector3>();
        List<Vector3> colours = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();

        void Start() {
            var filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            var mesh = HeComp.Weld(filt.sharedMesh);
            var geom = new HeGeom(mesh, transform);
            filt.sharedMesh = mesh;

            var s = new double[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
            s[100] = 1;
            s[0] = 1;
            //s[Random.Range(0, geom.nVerts)] = 1;

            var hm = new ScalarHeatMethod(geom); 
            var hd = hm.Compute(V.Build.DenseOfArray(s));
            var vals = new Color[geom.nVerts];
            var max = 0.0;
            foreach(var v in geom.Verts) {
                var i = v.vid;
                max = math.max(max, hd[i]);
            }
            foreach(var v in geom.Verts) {
                var i = v.vid;
                vals[i] = colScheme.Evaluate((float)((hd[i]) / max));
            }
            mesh.colors = vals;
            
            tracers = Isoline.Build(geom, hd, (float)max);

            for(var i = 0; i < tracers.Count; i++) {
                colours.Add(new Vector3(0, 0, 0));
                normals.Add(new Vector3(0, 0, 0));
            }

            tracerBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tracers.Count, 12);
            colourBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tracers.Count, 12);
            normalBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tracers.Count, 12);
            tracerBuff.SetData(tracers);
            normalBuff.SetData(normals);
            colourBuff.SetData(colours);
        }

        protected void OnRenderObject() {
            tracerMat.SetPass(0);
            tracerMat.SetBuffer("_Line", tracerBuff);
            tracerMat.SetBuffer("_Norm", normalBuff);
            tracerMat.SetBuffer("_Color", colourBuff);
            Graphics.DrawProceduralNow(MeshTopology.Lines, tracers.Count);
        }
    
        protected void OnDestroy() {
            tracerBuff.Dispose();
            colourBuff.Dispose();
            normalBuff.Dispose();
        }
    }
}
