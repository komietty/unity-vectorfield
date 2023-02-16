using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VectorField.Demo {
    public class RibbonViewer : MonoBehaviour {
        [SerializeField] protected Gradient colScheme;
        [SerializeField] protected int tracerNum;
        [SerializeField] protected int tracerLen;
        Material lineMat;
        TangentTracer tracer;
        GraphicsBuffer tracerBuff;
        GraphicsBuffer colourBuff;
        GraphicsBuffer normalBuff;
        List<Vector3> tracers = new ();
        List<Vector3> colours = new ();
        List<Vector3> normals = new ();
    
        void Start() {
            var container = GetComponent<GeomContainer>();
            var geom = container.geom;
            var hodge = new HodgeDecomposition(geom);
            var omega = TangentField.GenRandomOneForm(geom).oneForm;
            var exact = hodge.Exact(omega);
            var coexact = hodge.CoExact(omega);
            var hamonic = hodge.Harmonic(omega, exact, coexact);
            var tngs = TangentField.InterpolateWhitney(hamonic, geom);
            tracer = new TangentTracer(geom, tngs, tracerLen);
            lineMat = new Material(container.LineMat);
            
    
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
            lineMat.SetBuffer("_Line", tracerBuff);
            lineMat.SetBuffer("_Norm", normalBuff);
            lineMat.SetBuffer("_Col",  colourBuff);
            lineMat.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Lines, tracers.Count);
        }
    
        void OnDestroy() {
            Destroy(lineMat);
            tracerBuff?.Dispose();
            colourBuff?.Dispose();
        }
    }
}