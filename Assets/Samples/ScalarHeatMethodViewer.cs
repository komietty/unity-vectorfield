using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

namespace VectorField.Demo {
    using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;

    public class ScalarHeatMethodViewer : MonoBehaviour {
        [SerializeField] protected Gradient colScheme;
        GraphicsBuffer tracerBuff;
        GraphicsBuffer colourBuff;
        GraphicsBuffer normalBuff;
        List<Vector3> tracers = new ();
        List<Vector3> colours = new ();
        List<Vector3> normals = new ();
        Material lineMat;

        void Start() {
            var c = GetComponent<GeomContainer>();
            var g = c.geom;
            lineMat = new Material(c.LineMat);

            var s = Vector.Build.Dense(g.nVerts);
            for (var i = 0; i < g.nVerts; i++) s[i] = 0;
            s[UnityEngine.Random.Range(0, g.nVerts)] = 1;
            s[UnityEngine.Random.Range(0, g.nVerts)] = 1;
            s[UnityEngine.Random.Range(0, g.nVerts)] = 1;

            var dist = ScalarHeatMethod.ComputeScalarHeatFlow(g, s);
            var max = dist.Max(v => v);
            var cols = new Color[g.nVerts];
            for(var i = 0; i < g.nVerts; i++) cols[i] = colScheme.Evaluate((float)(dist[i] / max));
            c.vertexColors = cols;
            
            tracers = Isoline.Build(g, dist, (float)max);

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
            lineMat.SetBuffer("_Line", tracerBuff);
            lineMat.SetBuffer("_Norm", normalBuff);
            lineMat.SetBuffer("_Color", colourBuff);
            lineMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, tracers.Count);
        }
    
        protected void OnDestroy() {
            tracerBuff?.Dispose();
            colourBuff?.Dispose();
            normalBuff?.Dispose();
        }
    }

    public static class Isoline {
        public static List<Vector3> Build(HeGeom g, Vector phi, float maxPhi) {
            var lines = new List<Vector3>();
            var sgmts = new List<Vector3>();
            var interval = maxPhi / 30;
            var mlen = g.MeanEdgeLength();

            foreach (var f in g.Faces) {
                foreach (var h in g.GetAdjacentHalfedges(f)) {
                    var i = h.vid;
                    var j = h.twin.vid;
                    var region1 = math.floor(phi[i] / interval);
                    var region2 = math.floor(phi[j] / interval);
                    if (region1 != region2) {
                        var t = region1 < region2
                            ? (float)((region2 * interval - phi[i]) / (phi[j] - phi[i]))
                            : (float)((region1 * interval - phi[i]) / (phi[j] - phi[i]));
                        sgmts.Add(g.Pos[i] * (1 - t) + g.Pos[j] * t + g.FaceNormal(f).n  * mlen * 0.1f);
                    }
                }
                if (sgmts.Count == 2) lines.AddRange(sgmts);
                sgmts.Clear();
            }
            return lines;
        }
    }
}
