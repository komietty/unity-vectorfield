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
            generators = new HomologyGenerator(geom).BuildGenerators();
            HeterA = BuildCycleMatrix();
            TransA = S.OfMatrix(HeterA.Transpose());
            SqareA = TransA * HeterA;
        }
        
        public V ComputeCoExactComponent(float[] singularity) {
            var rhs = new double[geom.nVerts + generators.Count];
            foreach (var v in geom.Verts)
                rhs[v.vid] = -geom.AngleDefect(v) + 2 * PI * singularity[v.vid];
            for(var i =0; i < generators.Count; i++) {
                rhs[geom.nVerts + i] = -AngleDefectAroundGenerator(generators[i]);
            }

            return HeterA * Solver.Cholesky(SqareA, rhs);
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