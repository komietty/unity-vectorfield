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
    using E = ExteriorDerivatives;
    
    public class TrivialConnectionAlt {
        private HeGeom geom;
        private S SqareA;
        private S TransA;
        private S HeterA;
        private S nullSpaceCoef;
        private List<List<HalfEdge>> generators;

        protected List<V> bases;
        protected S Period;

        public TrivialConnectionAlt(HeGeom geom) {
            this.geom = geom;
            generators = new HomologyGenerator(geom).BuildGenerators();
            HeterA = BuildCycleMatrix();
            TransA = S.OfMatrix(HeterA.Transpose());
            SqareA = TransA * HeterA;
            
            var hd = new HodgeDecomposition(geom);
            generators = new HomologyGenerator(geom).BuildGenerators();
            bases = new HamonicBasis(geom).Compute(hd, generators);
            Period = BuildPeriodMatrix();
        }
        
        public V ComputeCoExactComponent(float[] singularity) {
            var rhs = new double[geom.nVerts + generators.Count];
            foreach (var v in geom.Verts)
                rhs[v.vid] = -geom.AngleDefect(v) + 2 * PI * singularity[v.vid];
            for(var i = 0; i < generators.Count; i++) 
                rhs[geom.nVerts + i] = -AngleDefectAroundGenerator(generators[i]);
            return HeterA * Solver.Cholesky(SqareA, rhs);
        }
        
        public V ComputeConnections(float[] singularity) {
            if(!SatisfyGaussBonnet(singularity)) throw new System.Exception();
            var coexact = ComputeCoExactComponent(singularity);
            var gamma = ComputeHamonicComponent(coexact);
            return coexact + gamma;
        }
        
        double AngleDefectAroundGenerator(List<HalfEdge> generator) {
            var theta = 0d;
            foreach (var h in generator) theta = TransportNoRotation(h, theta);
            while( theta >=  PI ) theta -= 2 * PI;
            while( theta <  -PI ) theta += 2 * PI;
            return -theta;
        }

        S BuildCycleMatrix() {
            var T = new List<(int i, int j, double v)>();
            foreach (var v in geom.Verts) 
            foreach (var h in geom.GetAdjacentHalfedges(v)) {
                T.Add((h.edge.eid, v.vid, h.IsEdgeDir()? -1 : 1));
            }
            for (var i = 0; i < generators.Count; i++) 
            foreach (var h in generators[i]) {
                T.Add((h.edge.eid, geom.nVerts + i, h.IsEdgeDir() ? -1 : 1));
            }
            
            return S.OfIndexed(geom.nEdges, geom.nVerts + generators.Count, T);
        }
        
        public float3[] GetFaceVectorFromConnection(V phi) {
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
        
        double TransportNoRotation(HalfEdge h, double alphaI = 0) {
            var u = geom.Vector(h);
            var (e1, e2) = geom.OrthonormalBasis(h.face);
            var (f1, f2) = geom.OrthonormalBasis(h.twin.face);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            return alphaI - thetaIJ + thetaJI;
        }
        
        bool SatisfyGaussBonnet(float[] singularity){
            var sum = 0f;
            foreach (var v in geom.Verts) sum += singularity[v.vid];
            return abs(geom.eulerCharactaristics - sum) < 1e-8;
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
                        var s = h.edge.hid == h.id ? 1 : -1;
                        sum += s * bases[k];
                    }
                    t.Add((i, j, sum));
                }
            }
            return SparseMatrix.OfIndexed(n, n, t);
        }

        V ComputeHamonicComponent(V deltaBeta) {
            var N = bases.Count;
            var E = geom.nEdges;
            var gamma = V.Build.Dense(E, 0);

            if (N > 0) {
                var rhs = V.Build.Dense(N, 0);
                for (var i = 0; i < N; i++) {
                    var generator = generators[i];
                    var sum = 0.0;

                    foreach (var h in generator) {
                        var k = h.edge.eid;
                        var s = h.edge.hid == h.id ? 1 : -1;
                        sum += TransportNoRotation(h);
                        sum -= s * deltaBeta[k];
                    }

                    while (sum < -PI) sum += 2 * PI;
                    while (sum >= PI) sum -= 2 * PI;
                    rhs[i] = sum;
                }

                var outs = Solver.LU(Period,rhs);
                for (var i = 0; i < N; i++) { gamma += bases[i] * outs[i]; }
            }
            return gamma;
        }
    }
}
