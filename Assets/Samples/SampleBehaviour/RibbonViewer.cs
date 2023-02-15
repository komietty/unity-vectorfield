using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VectorField.Demo {
    public class RibbonViewer : MonoBehaviour {
        [SerializeField] protected Gradient colScheme;
        [SerializeField] protected Material tracerMat;
        [SerializeField] protected int tracerNum;
        [SerializeField] protected int tracerLen;
        TangentTracer tracer;
        GraphicsBuffer tracerBuff;
        GraphicsBuffer colourBuff;
        GraphicsBuffer normalBuff;
        List<Vector3> tracers = new List<Vector3>();
        List<Vector3> colours = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
    
        void Start() {
            var geom = GetComponent<GeomContainer>().geom;
            var hodge = new HodgeDecomposition(geom);
            var omega = TangentField.GenRandomOneForm(geom).oneForm;
            var exact = hodge.Exact(omega);
            var coexact = hodge.CoExact(omega);
            var hamonic = hodge.Harmonic(omega, exact, coexact);
            var tngs = TangentField.InterpolateWhitney(hamonic, geom);
            tracer = new TangentTracer(geom, tngs, tracerLen);
    
            for (var i = 0; i < tracerNum; i++) {
                var f = geom.Faces[Random.Range(0, geom.nFaces)];
                var m = geom.MeanEdgeLength();
                var r = tracer.GenTracer(f);
                var c = colScheme.Evaluate(Random.Range(0, 1f));
                for (var j = 0; j < r.Count - 1; j++) {
                    var tr0 = r[j];
                    var tr1 = r[j + 1];
                    tracers.Add(tr0.p + tr0.n * m / 3);
                    tracers.Add(tr1.p + tr1.n * m / 3);
                    normals.Add(tr0.n);
                    normals.Add(tr1.n);
                    colours.Add((Vector4)c);
                    colours.Add((Vector4)c);
                }
            }
            tracerBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tracers.Count, 12);
            colourBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tracers.Count, 12);
            normalBuff = new GraphicsBuffer(GraphicsBuffer.Target.Structured, tracers.Count, 12);
            tracerBuff.SetData(tracers);
            normalBuff.SetData(normals);
            colourBuff.SetData(colours);
        }

        void OnRenderObject() {
            tracerMat.SetBuffer("_Line", tracerBuff);
            tracerMat.SetBuffer("_Norm", normalBuff);
            tracerMat.SetBuffer("_Col",  colourBuff);
            tracerMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, tracers.Count);
        }
    
        void OnDestroy() {
            tracerBuff?.Dispose();
            colourBuff?.Dispose();
        }
    }
}