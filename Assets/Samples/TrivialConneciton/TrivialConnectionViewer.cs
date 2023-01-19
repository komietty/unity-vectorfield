using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace VectorField {
    public class TrivialConnectionViewer : TangentBundle {
        protected GraphicsBuffer normalBuf;

        protected override void Start() {
            base.Start();
            var t = new TrivialConnection(geom, new HodgeDecomposition(geom));
            var s = new float[geom.nVerts];
            for (var i = 0; i < geom.nVerts; i++) s[i] = 0;
            s[Random.Range(0, geom.nVerts)] = 1;
            s[Random.Range(0, geom.nVerts)] = 1;
            UpdateTng(t.GenField(t.ComputeConnections(s)));

            var n = geom.nFaces;
            var nrms = new Vector3[n * 6];
            for(var i = 0; i < n; i++){
                var face = geom.Faces[i];
                var (_, N) = geom.FaceNormal(face);
                nrms[i * 6 + 0] = N;
                nrms[i * 6 + 1] = N;
                nrms[i * 6 + 2] = N;
                nrms[i * 6 + 3] = N;
                nrms[i * 6 + 4] = N;
                nrms[i * 6 + 5] = N;
            }
            normalBuf = new GraphicsBuffer(Target.Structured, geom.nFaces * 6, 12);
            normalBuf.SetData(nrms);
            tngtMat.SetBuffer("_Norm", normalBuf);
        }
    }
}