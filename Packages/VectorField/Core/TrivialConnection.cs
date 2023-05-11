using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Sparse = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
using static Unity.Mathematics.math;

namespace VectorField {
    /*
     * Trivial connection with Hodge decomposition.
     * This technic is proposed after the original thesis, 
     * in the lecture note of Carnegie Mellon Univ. DDG course. 
     */
    public class TrivialConnectionHD: TrivialConnection {
        private readonly Sparse A;
        private readonly Sparse P;
        private readonly Sparse h1;
        private readonly Sparse d0;
        
        public TrivialConnectionHD(HeGeom g): base(g) {
            P  = BuildPeriodMatrix();
            A  = hodge.MatA;
            h1 = hodge.MatH1;
            d0 = hodge.MatD0;
        }
        
        Sparse BuildPeriodMatrix() {
            var T = new List<(int, int, double)>();
            for (var i = 0; i < nBasis; i++) {
                for (var j = 0; j < nBasis; j++) {
                    var b = basis[j];
                    var s = 0.0;
                    foreach (var h in genes[i]) {
                        var k = h.edge.eid;
                        var v = h.IsCanonical() ? -1 : 1;
                        s += v * b[k];
                    }
                    T.Add((i, j, s));
                }
            }
            return Sparse.OfIndexed(nBasis, nBasis, T);
        }
        
        Vector ComputeCoExactComponent(float[] singularity) {
            var rhs = new double[G.nVerts];
            foreach (var v in G.Verts)
                rhs[v.vid] = -G.AngleDefect(v) + 2 * PI * singularity[v.vid];
            return h1 * d0 * Solver.Cholesky(A, rhs);
        }
        
        Vector ComputeHarmonicComponent(Vector deltaBeta) {
            var H = Vector.Build.Dense(G.nEdges, 0);
            if (nBasis == 0) return H;
            var rhs = Vector.Build.Dense(nBasis, 0);
            for (var i = 0; i < nBasis; i++) {
                var gen = genes[i];
                var sum = 0.0;
                foreach (var h in gen) {
                    var k = h.edge.eid;
                    var s = h.IsCanonical() ? -1 : 1;
                    sum += TransportNoRotation(h);
                    sum -= s * deltaBeta[k];
                }
                while (sum < -PI) sum += 2 * PI;
                while (sum >= PI) sum -= 2 * PI;
                rhs[i] = sum;
            }
            var x = Solver.LU(P, rhs);
            for (var i = 0; i < nBasis; i++) H += basis[i] * x[i];
            return H;
        }
        
        public Vector ComputeConnections(float[] singularity) {
            if(!SatisfyGaussBonnet(singularity))
                throw new System.Exception("Singularities must satisfy Gauss-Bonnet theorem");
            var c = ComputeCoExactComponent(singularity);
            var h = ComputeHarmonicComponent(c);
            return c + h;
        }
    }
    
    /*
     * Trivial connection with QR factorization (WIP).
     * This is equivalent to trivial connection with Hodge decomposition above,
     * the only difference is that this technic is proposed in original thesis.
     */
    public class TrivialConnectionQR: TrivialConnection {
        private readonly Sparse A;

        public TrivialConnectionQR(HeGeom g): base(g) {
            A = BuildCycleMatrix();
        }
        
        double AngleDefectAroundGenerator(List<HalfEdge> generator) {
            var theta = 0d;
            foreach (var h in generator) theta = TransportNoRotation(h, theta);
            while(theta >=  PI) theta -= 2 * PI;
            while(theta <  -PI) theta += 2 * PI;
            return -theta;
        }
        
        Vector ComputeSmallestEigenValue(float[] singularity) {
            var rhs = new double[G.nVerts + genes.Count];
            foreach (var v in G.Verts)
                rhs[v.vid] = -G.AngleDefect(v) + 2 * PI * singularity[v.vid];
            for(var i = 0; i < genes.Count; i++) 
                rhs[G.nVerts + i] = -AngleDefectAroundGenerator(genes[i]);
            return Solver.QR(A, rhs);
        }
        
        public Vector ComputeConnections(float[] singularity) {
            if(!SatisfyGaussBonnet(singularity)) throw new System.Exception();
            var c = ComputeSmallestEigenValue(singularity);
            var d1 = DEC.BuildExteriorDerivative1Form(G);
            var d1t = Sparse.OfMatrix(d1.Transpose());
            var ddt = d1 * d1t;
            var x_sol = c - d1t * ddt.LU().Solve(d1 * c);
            //var x = c - d1t * (d1 * d1t).Inverse() * d1 * c;
            return x_sol;
        }

        Sparse BuildCycleMatrix() {
            var nv = G.nVerts;
            var ne = G.nEdges;
            var ng = genes.Count;
            var d0 = DEC.BuildExteriorDerivative0Form(G);
            var d0t = d0.Transpose();
            var h_strage = new List<(int i, int j, double v)>();
            for (var i = 0; i < ng; i++)
                foreach (var h in genes[i]) {
                    // +- ambiguous
                    h_strage.Add((h.edge.eid, i, h.IsCanonical() ? 1 : -1));
                }

            var H = Sparse.OfIndexed(ne, ng, h_strage);
            var Ht = H.Transpose();

            var A = Sparse.Create(nv + ng, ne, 0);
            for (var i = 0; i < nv; i++) { A.SetRow(i, d0t.Row(i)); }
            for (var i = 0; i < ng; i++) { A.SetRow(i + nv, Ht.Row(i)); }
            return A;
        }
    }
    
    /*
     * Common method holder for TrivialConnectionHD and TrivialConnectionQR.
     */
    public abstract class TrivialConnection {
        protected readonly HeGeom G;
        protected readonly HodgeDecomposition hodge;
        protected readonly List<Vector> basis;
        protected readonly List<List<HalfEdge>> genes;
        protected int nBasis => basis.Count;

        protected TrivialConnection(HeGeom g) {
            G = g;
            hodge = new HodgeDecomposition(G);
            genes = new HomologyGenerator(G).BuildGenerators();
            basis = genes.Select(g => hodge.ComputeHarmonicBasis(g)).ToList();
        }

        protected double TransportNoRotation(HalfEdge h, double alphaI = 0) {
            var u = G.Vector(h);
            var (e1, e2) = G.OrthonormalBasis(h.face);
            var (f1, f2) = G.OrthonormalBasis(h.twin.face);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            return alphaI - thetaIJ + thetaJI;
        }
        
        protected bool SatisfyGaussBonnet(float[] singularity){
            var sum = 0f;
            foreach (var v in G.Verts) sum += singularity[v.vid];
            return abs(G.eulerCharactaristics - sum) < 1e-8;
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