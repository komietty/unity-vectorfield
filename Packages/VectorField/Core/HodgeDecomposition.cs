namespace VectorField {
    using Vecd = MathNet.Numerics.LinearAlgebra.Vector<double>;
    using Sprs = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    public class HodgeDecomposition {
        Sprs h1;
        Sprs h2;
        Sprs d0;
        Sprs d1;
        Sprs h1i;
        Sprs h2i;
        Sprs d0t;
        Sprs d1t;
        Sprs A;
        Sprs B;

        public HodgeDecomposition(HeGeom g) {
            h1  = ExteriorDerivatives.BuildHodgeStar1Form(g);
            h2  = ExteriorDerivatives.BuildHodgeStar2Form(g);
            d0  = ExteriorDerivatives.BuildExteriorDerivative0Form(g);
            d1  = ExteriorDerivatives.BuildExteriorDerivative1Form(g);
            h1i = Sprs.OfDiagonalVector(h1.Diagonal().Map(v => 1 / v));
            h2i = Sprs.OfDiagonalVector(h2.Diagonal().Map(v => 1 / v));
            d0t = Sprs.OfMatrix(d0.Transpose());
            d1t = Sprs.OfMatrix(d1.Transpose());
            var n = d0t.RowCount;
            A = d0t * h1 * d0 + Sprs.CreateDiagonal(n, n, 1e-8);
            B = d1 * h1i * d1t;
        }

        public Vecd Exact   (Vecd omega) => d0 * Solver.Cholesky(A, d0t * h1 * omega);
        public Vecd CoExact (Vecd omega) => h1i * d1t * Solver.LU(B, d1 * omega);
        public Vecd Harmonic(Vecd omega, Vecd exact, Vecd coexact) => omega - exact - coexact;
    }
}
