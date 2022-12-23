using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Unity.Mathematics.math;

namespace ddg {
    public class TrivialConnection {
        protected HeGeom geom;
        protected SparseMatrix P;
        protected SparseMatrix A;
        protected SparseMatrix h1;
        protected SparseMatrix d0;
        protected List<DenseMatrix> bases;
        protected List<List<HalfEdge>> generators;

        public TrivialConnection(HeGeom g) {
            geom = g;
            var hd = new HodgeDecomposition(geom);
            var tc = new Homology(geom);
            var hb = new HamonicBasis(geom);
            generators = tc.BuildGenerators();
            bases = hb.Compute(hd, generators);
            //P = BuildPeriodMatrix();
            A = hd.ZeroFromLaplaceMtx;
            h1 = hd.h1;
            d0 = hd.d0;
        }

        
        bool SatisfyGaussBonnet(float[] singularity){
            var sum = 0f;
            foreach (var v in geom.Verts) sum += singularity[v.vid];
            return Mathf.Abs(geom.eulerCharactaristics - sum) < 1e-8;
        }

        public double TransportNoRotation(HalfEdge h, double alphaI = 0) {
            var u = geom.Vector(h);
            var (e1, e2) = geom.OrthonormalBasis(h.face);
            var (f1, f2) = geom.OrthonormalBasis(h.twin.face);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            return alphaI - thetaIJ + thetaJI;
        }

        DenseMatrix ComputeCoExactComponent(float[] singularity) {
            var rhs = new double[geom.nVerts];
            foreach (var v in geom.Verts) {
                var i = v.vid;
                rhs[i] = -geom.AngleDefect(v) + 2 * Mathf.PI * singularity[i];
            }
            var outs = new double[rhs.Length];
            var trps = A.Storage.EnumerateNonZeroIndexed().Select(t => new Triplet(t.Item3, t.Item1, t.Item2)).ToArray();
            Solver.DecompAndSolveChol(trps.Length, rhs.Length, trps, rhs, outs);
            return (DenseMatrix)(h1 * d0 * DenseMatrix.OfColumnMajor(outs.Length, 1, outs));
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
        public DenseMatrix ComputeConnections(float[] singularity) {
            //if(!SatisfyGaussBonnet(singularity)) throw new System.Exception();
            var deltaBeta = ComputeCoExactComponent(singularity);
            //var gamma = ComputeHamonicComponent(deltaBeta);
            //return deltaBeta + gamma;
            return deltaBeta;
        }
    }

    public class HamonicBasis {
        protected HeGeom geom;

        public HamonicBasis(HeGeom g) { geom = g; }

        public DenseMatrix BuildClosedPrimalOneForm(List<HalfEdge> generator) {
            var n = geom.nEdges;
            var d = new double[n];
            foreach (var h in generator) d[h.edge.eid] = h.edge.hid == h.id ? 1 : -1;
            return DenseMatrix.OfColumnMajor(n, 1, d);
        } 

        public List<DenseMatrix> Compute(HodgeDecomposition hd, List<List<HalfEdge>> generators) {
            var gammas = new List<DenseMatrix>();
            if (generators.Count > 0) {
                foreach (var g in generators) {
                    var omega  = BuildClosedPrimalOneForm(g);
                    var dAlpha = hd.ComputeExactComponent(omega);
                    gammas.Add(omega - dAlpha);
                }
            }
            return gammas;
        }
    }
}
