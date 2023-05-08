using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace VectorField {
    using S = SparseMatrix;
    using V = Vector<double>;
    
    public class TrivialConnection {
        private readonly HeGeom G;
        private readonly S A;
        private readonly List<List<HalfEdge>> generators;
        private readonly List<V> bases;
        private readonly S period;

        public TrivialConnection(HeGeom g) {
            G = g;
            generators = new HomologyGenerator(G).BuildGenerators();
            A = BuildCycleMatrix();
            var h = new HodgeDecomposition(G);
            bases = generators.Select(g => h.ComputeHamonicBasis(g)).ToList();
            period = BuildPeriodMatrix();
        }
        
        V ComputeCoExactComponent(float[] singularity) {
            var rhs = new double[G.nVerts + generators.Count];
            foreach (var v in G.Verts)
                rhs[v.vid] = -G.AngleDefect(v) + 2 * PI * singularity[v.vid];
            for(var i = 0; i < generators.Count; i++) 
                rhs[G.nVerts + i] = -AngleDefectAroundGenerator(generators[i]);
            return Solver.QR(A, rhs);
        }
        
        public V ComputeConnections(float[] singularity) {
            if(!SatisfyGaussBonnet(singularity)) throw new System.Exception();
            var c = ComputeCoExactComponent(singularity);
            var d1 = ExteriorDerivatives.BuildExteriorDerivative1Form(G);
            var d1t = S.OfMatrix(d1.Transpose());
            var ddt = d1 * d1t;
            var xmin = c - d1t * ddt.LU().Solve(d1 * c);
            
            Debug.Log("xmin");
            Debug.Log(xmin);
            return xmin;
            var x = c - d1t * (d1 * d1t).Inverse() * d1 * c;
            var h = ComputeHamonicComponent(x);
            return x;// + h;
        }
        
        double AngleDefectAroundGenerator(List<HalfEdge> generator) {
            var theta = 0d;
            foreach (var h in generator) theta = TransportNoRotation(h, theta);
            while(theta >=  PI) theta -= 2 * PI;
            while(theta <  -PI) theta += 2 * PI;
            return -theta;
        }

        S BuildCycleMatrix() {
            var nv = G.nVerts;
            var ne = G.nEdges;
            var ng = generators.Count;
            var d0 = ExteriorDerivatives.BuildExteriorDerivative0Form(G);
            var d0t = d0.Transpose();
            var h_strage = new List<(int i, int j, double v)>();
            for (var i = 0; i < ng; i++)
                foreach (var h in generators[i]) {
                    // +- ambiguous
                    h_strage.Add((h.edge.eid, i, h.IsCanonical() ? 1 : -1));
                }

            var H = S.OfIndexed(ne, ng, h_strage);
            var Ht = H.Transpose();

            var A = S.Create(nv + ng, ne, 0);
            for (var i = 0; i < nv; i++) { A.SetRow(i, d0t.Row(i)); }
            for (var i = 0; i < ng; i++) { A.SetRow(i + nv, Ht.Row(i)); }
            return A;
            /*
            var T = new List<(int i, int j, double v)>();
            foreach (var v in G.Verts) 
            foreach (var h in G.GetAdjacentHalfedges(v)) {
                T.Add((h.edge.eid, v.vid, h.IsEdgeDir()? -1 : 1));
            }
            for (var i = 0; i < ng; i++) 
                foreach (var h in generators[i]) {
                    T.Add((h.edge.eid, nv + i, h.IsEdgeDir() ? -1 : 1));
                }
            return S.OfIndexed(ne, nv + ng, T);
            */
        }
        
        public float3[] GetFaceVectorFromConnection(V phi) {
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
        
        double TransportNoRotation(HalfEdge h, double alphaI = 0) {
            var u = G.Vector(h);
            var (e1, e2) = G.OrthonormalBasis(h.face);
            var (f1, f2) = G.OrthonormalBasis(h.twin.face);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            return alphaI - thetaIJ + thetaJI;
        }
        
        bool SatisfyGaussBonnet(float[] singularity){
            var sum = 0f;
            foreach (var v in G.Verts) sum += singularity[v.vid];
            return abs(G.eulerCharactaristics - sum) < 1e-8;
        }
        
        S BuildPeriodMatrix() {
            var n = bases.Count;
            var t = new List<(int, int, double)>();
            for (var i = 0; i < n; i++) {
                var g = generators[i];
                for (var j = 0; j < n; j++) {
                    var bases = this.bases[j];
                    var sum = 0.0;
                    foreach (var h in g) {
                        var k = h.edge.eid;
                        var s = h.IsCanonical() ? 1 : -1;
                        sum += s * bases[k];
                    }
                    t.Add((i, j, sum));
                }
            }
            return SparseMatrix.OfIndexed(n, n, t);
        }

        V ComputeHamonicComponent(V deltaBeta) {
            var N = bases.Count;
            var gamma = V.Build.Dense(G.nEdges, 0);

            if (N > 0) {
                var rhs = V.Build.Dense(N, 0);
                for (var i = 0; i < N; i++) {
                    var generator = generators[i];
                    var sum = 0.0;
                    foreach (var h in generator) {
                        var k = h.edge.eid;
                        var s = h.IsCanonical() ? 1 : -1;
                        sum += TransportNoRotation(h);
                        sum -= s * deltaBeta[k];
                    }
                    while (sum < -PI) sum += 2 * PI;
                    while (sum >= PI) sum -= 2 * PI;
                    rhs[i] = sum;
                }
                var outs = Solver.LU(period,rhs);
                for (var i = 0; i < N; i++) { gamma += bases[i] * outs[i]; }
            }
            return gamma;
        }
    }
}
