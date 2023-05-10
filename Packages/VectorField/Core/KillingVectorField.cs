using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace VectorField {
    using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
    using Sparse = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    
    public class KillingVectorField {
        private readonly HeGeom geom;
        public Vector omega;

        public KillingVectorField(HeGeom geom) {
            this.geom = geom;
            var h0  = ExteriorDerivatives.BuildHodgeStar0Form(geom);
            var h1  = ExteriorDerivatives.BuildHodgeStar1Form(geom); // B
            var h2  = ExteriorDerivatives.BuildHodgeStar2Form(geom);
            var d0  = ExteriorDerivatives.BuildExteriorDerivative0Form(geom);
            var d1  = ExteriorDerivatives.BuildExteriorDerivative1Form(geom);
            var h0i = Sparse.OfDiagonalVector(h0.Diagonal().Map(v => 1 / v));
            var h1i = Sparse.OfDiagonalVector(h1.Diagonal().Map(v => 1 / v));
            var adjoint1  = Sparse.OfMatrix(h0i * d0.Transpose() * h1);
            var adjoint2  = Sparse.OfMatrix(h1i * d1.Transpose() * h2);
            var laplacian = adjoint2 * d1 + d0 * adjoint1;
            var g = new double[geom.Edges.Length];
            for (var i = 0; i < this.geom.Edges.Length; i++) 
                g[i] = PointwiseCurvatureOnEdge(this.geom.Edges[i]);
            var G = Sparse.OfDiagonalArray(g);
            var R = laplacian + d0 * adjoint1 - 2 * h1 * G;
            omega = Solver.SmallestEigenPositiveDefinite(R, h1);
        }

        double PointwiseCurvatureOnEdge(Edge e) {
            var h  = geom.halfedges[e.hid];
            var vi = geom.Verts[h.vid];
            var vj = geom.Verts[h.twin.vid];
            var ki = geom.ScalarGaussCurvature(vi);
            var kj = geom.ScalarGaussCurvature(vj);
            return (ki / geom.CircumcentricDualArea(vi) + kj / geom.CircumcentricDualArea(vj)) * 0.5;
        }
        
        public float3[] GenVectorField(Vector oneForm) {
            return ExteriorDerivatives.InterpolateWhitney(oneForm, geom);
        }

    }
}

