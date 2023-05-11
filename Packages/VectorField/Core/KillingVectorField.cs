using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Sparse = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;

namespace VectorField {
    public static class KillingVectorField {
        public static Vector Compute(HeGeom geom) {
            var h0  = DEC.BuildHodgeStar0Form(geom);
            var h1  = DEC.BuildHodgeStar1Form(geom); // B
            var h2  = DEC.BuildHodgeStar2Form(geom);
            var d0  = DEC.BuildExteriorDerivative0Form(geom);
            var d1  = DEC.BuildExteriorDerivative1Form(geom);
            var h0i = Sparse.OfDiagonalVector(h0.Diagonal().Map(v => 1 / v));
            var h1i = Sparse.OfDiagonalVector(h1.Diagonal().Map(v => 1 / v));
            var d0t = Sparse.OfMatrix(d0.Transpose());
            var d1t = Sparse.OfMatrix(d1.Transpose());
            var adjoint1  = h0i * d0t * h1;
            var adjoint2  = h1i * d1t * h2;
            var laplacian = adjoint2 * d1 + d0 * adjoint1;
            var strage = new double[geom.Edges.Length];
            for (var i = 0; i < geom.Edges.Length; i++) 
                strage[i] = PointwiseCurvatureOnEdge(geom, geom.Edges[i]);
            var G = Sparse.OfDiagonalArray(strage);
            var R = laplacian + d0 * adjoint1 - 2 * h1 * G;
            return Solver.SmallestEigenPositiveDefinite(R, h1);
        }

        private static double PointwiseCurvatureOnEdge(HeGeom G, Edge e) {
            var h  = G.halfedges[e.hid];
            var vi = G.Verts[h.vid];
            var vj = G.Verts[h.twin.vid];
            var ki = G.ScalarGaussCurvature(vi);
            var kj = G.ScalarGaussCurvature(vj);
            return (
                ki / G.CircumcentricDualArea(vi) +
                kj / G.CircumcentricDualArea(vj)
                ) * 0.5;
        }
    }
}

