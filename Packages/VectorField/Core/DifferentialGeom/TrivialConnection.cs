using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace VectorField {
    using S = SparseMatrix;
    using V = Vector<double>;
    using E = ExteriorDerivatives;

    public class TrivialConnection {
        protected HeGeom geom;
        protected S A;
        protected S d0;
        protected S d0t;
        protected List<List<HalfEdge>> generators;
        protected List<V> bases;
        protected S P;

        public TrivialConnection(HeGeom g) {
            geom = g;
            d0  = E.BuildExteriorDerivative0Form(g);
            d0t = S.OfMatrix(d0.Transpose());
            var n = d0.ColumnCount;
            A = d0t * d0 + S.CreateDiagonal(n, n, 1e-8);
            var hd = new HodgeDecomposition(geom);
            generators = new HomologyGenerator(geom).BuildGenerators();
            bases = new HamonicBasis(geom).Compute(hd, generators);
            P = BuildPeriodMatrix();
        }

        double TransportNoRotation(HalfEdge h, double alphaI = 0) {
            var u = geom.Vector(h);
            var (e1, e2) = geom.OrthonormalBasis(h.face);
            var (f1, f2) = geom.OrthonormalBasis(h.twin.face);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            return alphaI - thetaIJ + thetaJI;
        }

        public V ComputeCoExactComponent(float[] singularity) {
            var rhs = new double[geom.nVerts];
            foreach (var v in geom.Verts)
                rhs[v.vid] = -geom.AngleDefect(v) + 2 * PI * singularity[v.vid];
            return d0 * Solver.Cholesky(A, rhs);
        }
        
        bool SatisfyGaussBonnet(float[] singularity){
            var sum = 0f;
            foreach (var v in geom.Verts) sum += singularity[v.vid];
            return abs(geom.eulerCharactaristics - sum) < 1e-8;
        }

        public V ComputeConnections(float[] singularity) {
            if(!SatisfyGaussBonnet(singularity)) throw new System.Exception();
            var deltaBeta =  ComputeCoExactComponent(singularity);
            var gamma = ComputeHamonicComponent(deltaBeta);
            return deltaBeta + gamma;
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

                var outs = Solver.LU(P,rhs);
                for (var i = 0; i < N; i++) { gamma += bases[i] * outs[i]; }
            }
            return gamma;
        }
    }
}
