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
        private S A;
        private S d0t;
        private S nullSpaceCoef;

        public TrivialConnectionAlt(HeGeom g) {
            geom = g;
            A = BuildCycleMatrix();
            var d1 = E.BuildExteriorDerivative1Form(g);
            var d1t = S.OfMatrix(d1.Transpose());
            var d0  = E.BuildExteriorDerivative0Form(g);
            d0t = S.OfMatrix(d0.Transpose());
            Debug.Log(A);
            Debug.Log(d0t.Transpose());
            
            var inverse = (d1 * d1t).Inverse();
            nullSpaceCoef = S.OfMatrix(d1t * inverse * d1);
        }
        
        public V Compute(float[] singularity) {
            var rhs = new double[geom.nVerts];
            foreach (var v in geom.Verts)
                rhs[v.vid] = -geom.AngleSum(v) + 2 * PI * singularity[v.vid];
            //var svd = A.Transpose().Svd();
            var svd = d0t.Svd();
            var rhobar = svd.Solve(V.Build.DenseOfArray(rhs));
            var rho = rhobar - nullSpaceCoef * rhobar;
            return rho;
        }
        

        S BuildCycleMatrix() {
            var triplets = new List<(int i, int j, double v)>();
            foreach (var v in geom.Verts) {
                foreach (var h in geom.GetAdjacentHalfedges(v)) {
                    var k = h.edge.eid;
                    var i = h.vid;
                    var j = h.twin.vid;
                    triplets.Add((k, v.vid, i > j? 1d : -1d));
                }
            }
            return S.OfIndexed(geom.nEdges, geom.nVerts, triplets);
        }
    }
}
