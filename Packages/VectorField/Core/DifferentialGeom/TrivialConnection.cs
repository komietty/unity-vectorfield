using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace VectorField {
    using S = SparseMatrix;
    using V = Vector<double>;

    public class TrivialConnection {
        protected HeGeom geom;
        protected S A;
        protected S h1;
        protected S d0;

        public TrivialConnection(HeGeom g, HodgeDecomposition h) {
            geom = g;
            A  = h.A;
            h1 = h.h1;
            d0 = h.d0;
        }
        
        bool SatisfyGaussBonnet(float[] singularity){
            var sum = 0f;
            foreach (var v in geom.Verts) sum += singularity[v.vid];
            return abs(geom.eulerCharactaristics - sum) < 1e-8;
        }

        double TransportNoRotation(HalfEdge h, double alphaI = 0) {
            var u = geom.Vector(h);
            var (e1, e2) = geom.OrthonormalBasis(h.face);
            var (f1, f2) = geom.OrthonormalBasis(h.twin.face);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            return alphaI - thetaIJ + thetaJI;
        }

        V ComputeCoExactComponent(float[] singularity) {
            var rhs = new double[geom.nVerts];
            foreach (var v in geom.Verts)
                rhs[v.vid] = -geom.AngleDefect(v) + 2 * PI * singularity[v.vid];
            return h1 * d0 * Solver.Cholesky(A, rhs);
        }

        public V ComputeConnections(float[] singularity) {
            if(!SatisfyGaussBonnet(singularity)) throw new System.Exception();
            return  ComputeCoExactComponent(singularity);
        }

        public float3[] GenField(V phi) {
            var visit = new bool[geom.nFaces];
            var alpha = new double[geom.nFaces];
            var field = new float3[geom.nFaces];
            var queue = new Queue<int>();
            var f0 = geom.Faces[0];
            queue.Enqueue(f0.fid);
            alpha[f0.fid] = 0;
            while (queue.Count > 0) {
                var fid = queue.Dequeue();
                foreach (var h in geom.GetAdjacentHalfedges(geom.Faces[fid])) {
                    var gid = h.twin.face.fid;
                    if (!visit[gid] && gid != f0.fid) {
                        var sign = h.IsEdgeDir() ? 1 : -1;
                        var conn = sign * phi[h.edge.eid];
                        alpha[gid] = TransportNoRotation(h, alpha[fid]) + conn;
                        visit[gid] = true;
                        queue.Enqueue(gid);
                    }
                }
            } 
            foreach (var f in geom.Faces) {
                var a = alpha[f.fid];
                var (e1, e2) = geom.OrthonormalBasis(f);
                field[f.fid] = e1 * (float)cos(a) + e2 * (float)sin(a);
            }
            return field;
        }
    }
}

/*
        SparseMatrix BuildPeriodMatrix() {
            var n = bases.Count;
            var t = new List<(int, int, double)>();
            for (var i = 0; i < n; i++) {
                var g = generators[i];
                for (var j = 0; j < n; j++) {
                    var bases = this.bases[j];
                    var sum = 0.0;
                    foreach (var h in g) {
                        var k = h.edge.eid;
                        var s = h.edge.hid == h.id ? 1 : -1;
                        sum += s * bases[k, 0];
                    }
                    t.Add((i, j, sum));
                }
            }
            return SparseMatrix.OfIndexed(n, n, t);
        }

        DenseMatrix ComputeHamonicComponent(DenseMatrix deltaBeta) {
            var N = bases.Count;
            var E = geom.nEdges;
            var gamma = DenseMatrix.Create(E, 1, 0);

            if (N > 0) {
                // construct right hand side
                var rhs = DenseMatrix.Create(N, 1, 0);
                for (var i = 0; i < N; i++) {
                    var generator = generators[i];
                    var sum = 0.0;

                    foreach (var h in generator) {
                        var k = h.edge.eid;
                        var s = h.edge.hid == h.id ? 1 : -1;
                        sum += TransportNoRotation(h);
                        sum -= s * deltaBeta[k, 0];
                    }

                    // normalize sum between -π and π
                    while (sum < -Mathf.PI) sum += 2 * Mathf.PI;
                    while (sum >= Mathf.PI) sum -= 2 * Mathf.PI;

                    rhs[i, 0] = sum;
                }

                var outs = new double[rhs.RowCount];
                var trps = P.Storage.EnumerateNonZeroIndexed().Select(t => new Triplet(t.Item3, t.Item1, t.Item2)).ToArray();
                Solver.DecompAndSolveLU(trps.Length, rhs.RowCount, trps, rhs.Column(0).ToArray(), outs);
                // compute γ
                for (var i = 0; i < N; i++) { gamma += this.bases[i] * outs[i]; }
            }
            return gamma;
        }
*/