using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using System.Collections.Generic;

namespace ddg {
    public class TrivialConnectionViewer : TangentBundleBehaviour {
        TrivialConnection t;

        protected override void Start() {
            base.Start();
            var g = bundle.Geom;
            t = new TrivialConnection(g);
            var singularites = new float[g.nVerts];
            for(var i = 0; i < g.nVerts; i++){
                var v = 0f;
                if (i == 1) v = 1f;
                if (i == 5) v = 1f;
                singularites[i] = v;
            }
            var m = t.ComputeConnections(singularites);
            var b = BuildDirectionalField(g, m);
            UpdateTng(b);

            tngMat.SetFloat("_C", 1);
        }

        Dictionary<int, int> faceParentList = new Dictionary<int, int>();
        Dictionary<int, double> alpha = new Dictionary<int, double>();

        float3[] BuildDirectionalField(HeGeom geom, DenseMatrix phi) {
            foreach (var f in geom.Faces) faceParentList.Add(f.fid, f.fid);
            var queue = new Queue<int>();
            var f0 = geom.Faces[0];
            queue.Enqueue(f0.fid);
            alpha[f0.fid] = 0;
            while (queue.Count > 0) {
                var fid = queue.Dequeue();
                foreach (var h in geom.GetAdjacentHalfedges(geom.Faces[fid])) {
                    var gid = h.twin.face.fid;
                    if (faceParentList[gid] == gid && gid != f0.fid) {
                        var sign = h.edge.hid == h.id ? 1 : -1;
                        var conn = sign * phi[h.edge.eid, 0]; 
                        alpha[gid] = t.TransportNoRotation(h, alpha[fid]) - conn;
                        faceParentList[gid] = fid;
                        queue.Enqueue(gid);
                    }
                }
            } 
            var field = new float3[geom.nFaces];
            foreach (var f in geom.Faces) {
                var u = new Vector2(Mathf.Cos((float)alpha[f.fid]), Mathf.Sin((float)alpha[f.fid]));
                var (e1, e2) = geom.OrthonormalBasis(f);
                field[f.fid] = e1 * u.x + e2 * u.y;
            }
            return field;
        }
        protected void UpdateTng(float3[] omega) {
            var g = bundle.Geom;
            var n = g.nFaces;
            var tngs = new Vector3[n * 6];
            var mlen = 0.3f * g.MeanEdgeLength();
            for(var i = 0; i < n; i++){
                var face = g.Faces[i];
                var field = omega[i] * mlen;
                var C = (float3)g.Centroid(face);
                var (_, N) = g.FaceNormal(face);
                field = ClampFieldLength(field, mlen);
                var fc1 = C - field + N * 0.005f;
                var fc2 = C + field + N * 0.005f;
                var v = fc2 - fc1;
                var vT = math.cross(N, v);
                tngs[i * 6 + 0] = fc1;
                tngs[i * 6 + 1] = fc2;
                tngs[i * 6 + 2] = fc2;
                tngs[i * 6 + 3] = fc2 - v * 0.2f + vT * 0.2f;
                tngs[i * 6 + 4] = fc2;
                tngs[i * 6 + 5] = fc2 - v * 0.2f - vT * 0.2f;
            }
            tngBuf.SetData(tngs);
            tngMat.SetBuffer("_Line", tngBuf);
        }
    }
}