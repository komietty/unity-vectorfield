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
        private readonly S P;
        private readonly List<List<HalfEdge>> genes;
        private readonly List<V> bases;

        public TrivialConnection(HeGeom g) {
            G = g;
            var h = new HodgeDecomposition(G);
            genes = new HomologyGenerator(G).BuildGenerators();
            bases = genes.Select(g => h.ComputeHamonicBasis(g)).ToList();
            A = BuildCycleMatrix();
            P = BuildPeriodMatrix();
        }
        
        V ComputeCoExactComponent(float[] singularity) {
            var rhs = new double[G.nVerts + genes.Count];
            foreach (var v in G.Verts)
                rhs[v.vid] = -G.AngleDefect(v) + 2 * PI * singularity[v.vid];
            for(var i = 0; i < genes.Count; i++) 
                rhs[G.nVerts + i] = -AngleDefectAroundGenerator(genes[i]);
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
            return x;
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
            var ng = genes.Count;
            var d0 = ExteriorDerivatives.BuildExteriorDerivative0Form(G);
            var d0t = d0.Transpose();
            var h_strage = new List<(int i, int j, double v)>();
            for (var i = 0; i < ng; i++)
                foreach (var h in genes[i]) {
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
            throw new System.Exception();
            // same as alt
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
                var g = genes[i];
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
    }
}
