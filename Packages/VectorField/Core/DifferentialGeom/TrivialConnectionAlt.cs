using System;
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


        public TrivialConnectionAlt(HeGeom geom) {
            this.geom = geom;
            var homologygen = new HomologyGenerator(geom);
            generators = homologygen.BuildGenerators();
            var d0  = E.BuildExteriorDerivative0Form(geom);
            //d0 = BuildCycleMatrix();
            var storage = new List<(int, int, double)>();
            foreach (var v in d0.Storage.EnumerateNonZeroIndexed()) {
                storage.Add((v.Item2, v.Item1, v.Item3));
            }
            for (var i =0; i < generators.Count; i++) {
                foreach (var v in SignsAroundGenerator(generators[i])) {
                    storage.Add((geom.nVerts + i, v.i, v.v));
                }
            }
            HeterA = S.OfIndexed(
                this.geom.nVerts + generators.Count,
                this.geom.nEdges,
                storage);

            TransA = S.OfMatrix(HeterA.Transpose());
            SqareA = HeterA * TransA;
            //var inverse = (d1 * d1t).Inverse();
            //nullSpaceCoef = S.OfMatrix(d1t * inverse * d1);
        }
        
        public V ComputeCoExactComponent(float[] singularity) {
            var rhs = new double[geom.nVerts + generators.Count];
            foreach (var v in geom.Verts)
                rhs[v.vid] = -geom.AngleDefect(v) + 2 * PI * singularity[v.vid];
            for(var i =0; i < generators.Count; i++) {
                rhs[geom.nVerts + i] = AngleDefectAroundGenerator(generators[i]);
            }

            //throw new Exception();
            return TransA * Solver.Cholesky(SqareA, rhs);
        }
        
        IEnumerable<(int i, double v)> SignsAroundGenerator(IEnumerable<HalfEdge> generator) {
            return generator.Select(h => (h.edge.eid, h.edge.hid == h.id ? 1d : -1d)); 
        }
        
        double AngleDefectAroundGenerator(List<HalfEdge> generator) {
            var angle = 0d;
            foreach (var h in generator) angle = TransportNoRotation(h, angle);
            // TODO: check whether + or -
            return angle - 2 * PI;
        }


        S BuildCycleMatrix() {
            var triplets = new List<(int i, int j, double v)>();
            foreach (var v in geom.Verts) {
                foreach (var h in geom.GetAdjacentHalfedges(v)) {
                    var k = h.edge.eid;
                    var i = h.vid;
                    var j = h.twin.vid;
                    triplets.Add((k, v.vid, h.IsEdgeDir()? 1d : -1d));
                }
            }
            return S.OfIndexed(geom.nEdges, geom.nVerts, triplets);
        }
        
        double TransportNoRotation(HalfEdge h, double alphaI = 0) {
            var u = geom.Vector(h);
            var (e1, e2) = geom.OrthonormalBasis(h.face);
            var (f1, f2) = geom.OrthonormalBasis(h.twin.face);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            return alphaI - thetaIJ + thetaJI;
        }
    }
}
