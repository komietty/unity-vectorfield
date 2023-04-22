using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Factorization;
using UnityEngine;

namespace VectorField {
    using Sprs = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    
    public class KillingVectorField {
        protected HeGeom geom;
        
        public KillingVectorField(HeGeom geom) {
            this.geom = geom;
            var h1  = ExteriorDerivatives.BuildHodgeStar1Form(geom);
            var h2  = ExteriorDerivatives.BuildHodgeStar2Form(geom);
            var d0  = ExteriorDerivatives.BuildExteriorDerivative0Form(geom);
            var d1  = ExteriorDerivatives.BuildExteriorDerivative1Form(geom);
            var h1i = Sprs.OfDiagonalVector(h1.Diagonal().Map(v => 1 / v));
            var h2i = Sprs.OfDiagonalVector(h2.Diagonal().Map(v => 1 / v));
            var L = Operator.Laplace(geom);
            var B = h1;
            var invB = h1i;
            var arr = new double[geom.Edges.Length];
            for (var i = 0; i < this.geom.Edges.Length; i++) 
                arr[i] = PointwiseCurvatureOnEdge(this.geom.Edges[i]);

            // find B * omega
            // delta * invB * (B * omega) = lambda * (B * omega)
            var G = Sprs.OfDiagonalArray(arr);
            var delta  = L + d1 * (h2i * d1 * h1) - 2 * B * G;
            var evd = (delta * invB).Evd();
            var eigenValues  = evd.EigenValues;  // should return lambda
            var eigenVectors = evd.EigenVectors; // should return B * omega
        }

        double PointwiseCurvatureOnEdge(Edge e) {
            var h = geom.halfedges[e.hid];
            var vi = geom.Verts[h.vid];
            var vj = geom.Verts[h.twin.vid];
            var ki = geom.ScalarGaussCurvature(vi);
            var kj = geom.ScalarGaussCurvature(vj);
            return (ki / geom.CircumcentricDualArea(vi) + kj / geom.CircumcentricDualArea(vj)) * 0.5;
        }
    }
}

