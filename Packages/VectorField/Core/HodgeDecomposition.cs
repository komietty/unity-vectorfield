namespace VectorField {
    using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
    using Sparse = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    public class HodgeDecomposition {
        Sparse h1;
        Sparse d0;
        Sparse d1;
        Sparse h1i;
        Sparse d0t;
        Sparse d1t;
        Sparse A;
        Sparse B;

        public HodgeDecomposition(HeGeom g) {
            h1  = ExteriorDerivatives.BuildHodgeStar1Form(g);
            d0  = ExteriorDerivatives.BuildExteriorDerivative0Form(g);
            d1  = ExteriorDerivatives.BuildExteriorDerivative1Form(g);
            h1i = Sparse.OfDiagonalVector(h1.Diagonal().Map(v => 1 / v));
            d0t = Sparse.OfMatrix(d0.Transpose());
            d1t = Sparse.OfMatrix(d1.Transpose());
            var n = d0t.RowCount;
            A = d0t * h1 * d0 + Sparse.CreateDiagonal(n, n, 1e-8);
            B = d1 * h1i * d1t;
        }

        public Vector Exact   (Vector omega) => d0 * Solver.Cholesky(A, d0t * h1 * omega);
        public Vector CoExact (Vector omega) => h1i * d1t * Solver.LU(B, d1 * omega);
        public Vector Harmonic(Vector omega, Vector exact, Vector coexact) => omega - exact - coexact;
    }
}
