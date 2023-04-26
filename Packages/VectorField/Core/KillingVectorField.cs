using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using MathNet.Numerics.LinearAlgebra.Factorization;
using UnityEngine;

namespace VectorField {
    using Sprs = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    
    public class KillingVectorField {
        protected HeGeom geom;
        protected Evd<double> evd;
        protected Sprs Bi;

        public KillingVectorField(HeGeom geom) {
            this.geom = geom;
            var h0  = ExteriorDerivatives.BuildHodgeStar0Form(geom);
            var h1  = ExteriorDerivatives.BuildHodgeStar1Form(geom);
            var h2  = ExteriorDerivatives.BuildHodgeStar2Form(geom);
            var d0  = ExteriorDerivatives.BuildExteriorDerivative0Form(geom);
            var d1  = ExteriorDerivatives.BuildExteriorDerivative1Form(geom);
            var h0i = Sprs.OfDiagonalVector(h0.Diagonal().Map(v => 1 / v));
            var h1i = Sprs.OfDiagonalVector(h1.Diagonal().Map(v => 1 / v));
            var adjoint1  = h0i * d0.Transpose() * h1;
            var adjoint2  = h1i * d1.Transpose() * h2;
            var laplacian = (adjoint2 * d1 + d0 * adjoint1);
            var B  = h1;
            Bi = h1i;
            var arr = new double[geom.Edges.Length];
            for (var i = 0; i < this.geom.Edges.Length; i++) 
                arr[i] = PointwiseCurvatureOnEdge(this.geom.Edges[i]);
            // find B * omega
            // delta * invB * (B * omega) = lambda * (B * omega)
            var G = Sprs.OfDiagonalArray(arr);
            var delta = laplacian + d0 * adjoint1 - 2 * B * G;
            var lhs = delta * Bi;
            evd = lhs.Evd();
        }

        public float3[] GenVectorField() {
            var eigenValues  = evd.EigenValues;  // should return lambda
            var eigenVectors = evd.EigenVectors; // should return B * omega
            var oneForm = Bi * eigenVectors.Column(1);
            return ExteriorDerivatives.InterpolateWhitney(oneForm, geom);
            //var uniqueEigenVectors = new List<double>();
            //var uniqueEigenVectors = new List<>()
            //foreach (var v in eigenVectors) { }
        }

        double PointwiseCurvatureOnEdge(Edge e) {
            var h  = geom.halfedges[e.hid];
            var vi = geom.Verts[h.vid];
            var vj = geom.Verts[h.twin.vid];
            var ki = geom.ScalarGaussCurvature(vi);
            var kj = geom.ScalarGaussCurvature(vj);
            return (ki / geom.CircumcentricDualArea(vi) + kj / geom.CircumcentricDualArea(vj)) * 0.5;
        }
    }
}

