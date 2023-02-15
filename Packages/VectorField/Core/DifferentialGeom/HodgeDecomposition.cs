using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;

namespace VectorField {
    using V = Vector<double>;
    using S = SparseMatrix;
    using E = ExteriorDerivatives;

    public class HodgeDecomposition {
        public S h1  { get; }
        public S h2  { get; }
        public S d0  { get; }
        public S d1  { get; }
        public S h1i { get; }
        public S h2i { get; }
        public S d0t { get; }
        public S d1t { get; }
        public S A   { get; }
        public S B   { get; }

        public HodgeDecomposition(HeGeom g) {
            h1  = E.BuildHodgeStar1Form(g);
            h2  = E.BuildHodgeStar2Form(g);
            d0  = E.BuildExteriorDerivative0Form(g);
            d1  = E.BuildExteriorDerivative1Form(g);
            h1i = S.OfDiagonalVector(h1.Diagonal().Map(v => 1 / v));
            h2i = S.OfDiagonalVector(h2.Diagonal().Map(v => 1 / v));
            d0t = S.OfMatrix(d0.Transpose());
            d1t = S.OfMatrix(d1.Transpose());
            var n = d0t.RowCount;
            A = d0t * h1 * d0 + S.CreateDiagonal(n, n, 1e-8);
            B = d1 * h1i * d1t;
        }

        public V Exact   (V omega) => d0 * Solver.Cholesky(A, d0t * h1 * omega);
        public V CoExact (V omega) => h1i * d1t * Solver.LU(B, d1 * omega);
        public V Harmonic(V omega, V exact, V coexact) => omega - exact - coexact;
    }
}
