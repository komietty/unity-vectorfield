using System;
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
        List<Vector3> tracers = new ();
        List<Vector3> colours = new ();
        List<Vector3> normals = new ();

        void Start() {
            var container = GetComponent<GeomContainer>();
            var geom = container.geom;
            var mesh = container.mesh;

            var s = new double[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
            s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;
            s[UnityEngine.Random.Range(0, geom.nVerts)] = 1;

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
                vals[i] = colScheme.Evaluate((float)(hd[i] / max));
            }
            mesh.colors = vals;
            
            tracers = Isoline.Build(geom, hd, (float)max);

            for(var i = 0; i < tracers.Count; i++) {
                colours.Add(new Vector3(1, 1, 1));
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
            tracerMat.SetBuffer("_Line", tracerBuff);
            tracerMat.SetBuffer("_Norm", normalBuff);
            tracerMat.SetBuffer("_Color", colourBuff);
            tracerMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, tracers.Count);
        }
    
        protected void OnDestroy() {
            tracerBuff?.Dispose();
            colourBuff?.Dispose();
            normalBuff?.Dispose();
        }
    }

    public static class Isoline {
        public static List<Vector3> Build(HeGeom geom, V phi, float maxPhi) {
            var lines = new List<Vector3>();
            var sgmts = new List<Vector3>();
            var interval = maxPhi / 30;

            foreach (var f in geom.Faces) {
                foreach (var h in geom.GetAdjacentHalfedges(f)) {
                    var i = h.vid;
                    var j = h.twin.vid;
                    var region1 = math.floor(phi[i] / interval);
                    var region2 = math.floor(phi[j] / interval);
                    if (region1 != region2) {
                        var t = region1 < region2
                            ? (float)((region2 * interval - phi[i]) / (phi[j] - phi[i]))
                            : (float)((region1 * interval - phi[i]) / (phi[j] - phi[i]));
                        sgmts.Add(geom.Pos[i] * (1 - t) + geom.Pos[j] * t + geom.FaceNormal(f).n * 0.005f);
                    }
                }
                if (sgmts.Count == 2) lines.AddRange(sgmts);
                sgmts.Clear();
            }
            return lines;
        }
    }
}
