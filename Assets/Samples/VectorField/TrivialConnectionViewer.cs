using MathNet.Numerics.LinearAlgebra;
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
                if (i == 3) v = 1f;
                singularites[i] = v;
            }
            var m = t.ComputeConnections(singularites);
            UpdateTng(BuildDirectionalField(g, m));

            tngMat.SetFloat("_C", 1);
        }

        Dictionary<int, int> faceParentList = new Dictionary<int, int>();
        Dictionary<int, double> alpha = new Dictionary<int, double>();

        float3[] BuildDirectionalField(HeGeom geom, Vector<double> phi) {
            foreach (var f in geom.Faces) faceParentList.Add(f.fid, f.fid);
            var queue = new Queue<int>();
            var f0 = geom.Faces[UnityEngine.Random.Range(0, geom.nFaces)];
            queue.Enqueue(f0.fid);
            alpha[f0.fid] = 0;
            while (queue.Count > 0) {
                var fid = queue.Dequeue();
                foreach (var h in geom.GetAdjacentHalfedges(geom.Faces[fid])) {
                    var gid = h.twin.face.fid;
                    if (faceParentList[gid] == gid && gid != f0.fid) {
                        var sign = h.edge.hid == h.id ? 1 : -1;
                        var conn = sign * phi[h.edge.eid]; 
                            alpha[gid] = t.TransportNoRotation(h, alpha[fid])  + conn;
                        faceParentList[gid] = fid;
                        queue.Enqueue(gid);
                    }
                }
            } 
            var field = new float3[geom.nFaces];
            foreach (var f in geom.Faces) {
                if (alpha.ContainsKey(f.fid)) {
                    var u = new double2(math.cos(alpha[f.fid]), math.sin(alpha[f.fid]));
                    var (e1, e2) = geom.OrthonormalBasis(f);
                    field[f.fid] = e1 * (float)u.x + e2 * (float)u.y;
                }else {
                    field[f.fid] = float3.zero;
                }
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