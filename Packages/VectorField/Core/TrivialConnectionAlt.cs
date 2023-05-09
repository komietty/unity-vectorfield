using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace VectorField {
    using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
    using Sparse = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    
    public class TrivialConnectionAlt {
        private readonly HeGeom G;
        private readonly Sparse A;
        private readonly Sparse P;
        private readonly Sparse h1;
        private readonly Sparse d0;
        private readonly List<Vector> basis;
        public List<List<HalfEdge>> genes;
        
        public TrivialConnectionAlt(HeGeom g) {
            G = g;
            var hodge = new HodgeDecomposition(G);
            genes = new HomologyGenerator(G).BuildGenerators();
            basis = genes.Select(g => hodge.ComputeHamonicBasis(g)).ToList();
            P  = BuildPeriodMatrix();
            A  = hodge.MatA;
            h1 = hodge.MatH1;
            d0 = hodge.MatD0;
        }
        
        double TransportNoRotation(HalfEdge h, double alphaI = 0) {
            var u = G.Vector(h);
            var (e1, e2) = G.OrthonormalBasis(h.face);
            var (f1, f2) = G.OrthonormalBasis(h.twin.face);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            return alphaI - thetaIJ + thetaJI;
        }
        
        Sparse BuildPeriodMatrix() {
            var n = basis.Count;
            var T = new List<(int, int, double)>();
            for (var i = 0; i < n; i++) {
                var g = genes[i];
                for (var j = 0; j < n; j++) {
                    var b = basis[j];
                    var s = 0.0;
                    foreach (var h in g) {
                        var k = h.edge.eid;
                        var v = h.IsCanonical() ? -1 : 1;
                        s += v * b[k];
                    }
                    T.Add((i, j, s));
                }
            }
            return Sparse.OfIndexed(n, n, T);
        }
        
        Vector ComputeCoExactComponent(float[] singularity) {
            var rhs = new double[G.nVerts];
            foreach (var v in G.Verts)
                rhs[v.vid] = -G.AngleDefect(v) + 2 * PI * singularity[v.vid];
            return h1 * d0 * Solver.Cholesky(A, rhs);
        }
        
        Vector ComputeHamonicComponent(Vector deltaBeta) {
            var N = basis.Count;
            var gamma = Vector.Build.Dense(G.nEdges, 0);

            if (N > 0) {
                var rhs = Vector.Build.Dense(N, 0);
                for (var i = 0; i < N; i++) {
                    var generator = genes[i];
                    var sum = 0.0;
                    foreach (var h in generator) {
                        var k = h.edge.eid;
                        var s = h.IsCanonical() ? -1 : 1;
                        sum += TransportNoRotation(h);
                        sum -= s * deltaBeta[k];
                    }
                    while (sum < -PI) sum += 2 * PI;
                    while (sum >= PI) sum -= 2 * PI;
                    rhs[i] = sum;
                }
                var outs = Solver.LU(P, rhs);
                for (var i = 0; i < N; i++) { gamma += basis[i] * outs[i]; }
            }
            return gamma;
        }
        
        
        public Vector ComputeConnections(float[] singularity) {
            //if(!SatisfyGaussBonnet(singularity)) throw new System.Exception();
            var c = ComputeCoExactComponent(singularity);
            var h = ComputeHamonicComponent(c);
            return c + h;
        }
        
        public float3[] GetFaceVectorFromConnection(Vector phi) {
            var visit = new bool[G.nFaces];
            var alpha = new double[G.nFaces];
            var field = new float3[G.nFaces];
            var queue = new Queue<int>();
            var f0 = G.Faces[0];
            queue.Enqueue(f0.fid);
            alpha[f0.fid] = 0;
            while (queue.Count > 0) {
                var fid = queue.Dequeue();
                foreach (var h in G.GetAdjacentHalfedges(G.Faces[fid])) {
                    var gid = h.twin.face.fid;
                    if (!visit[gid] && gid != f0.fid) {
                        var sign = h.IsCanonical() ? 1 : -1;
                        var conn = sign * phi[h.edge.eid];
                        alpha[gid] = TransportNoRotation(h, alpha[fid]) + conn;
                        visit[gid] = true;
                        queue.Enqueue(gid);
                    }
                }
            } 
            foreach (var f in G.Faces) {
                var a = alpha[f.fid];
                var (e1, e2) = G.OrthonormalBasis(f);
                field[f.fid] = e1 * (float)cos(a) + e2 * (float)sin(a);
            }
            return field;
        }
    }
}