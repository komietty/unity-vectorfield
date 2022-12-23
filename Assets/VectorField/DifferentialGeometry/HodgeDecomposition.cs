using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;

namespace ddg {
    using Vd = Vector<double>;

    public class HodgeDecomposition {
        public SparseMatrix h1  { get; private set; }
        public SparseMatrix h2  { get; private set; }
        public SparseMatrix d0  { get; private set; }
        public SparseMatrix d1  { get; private set; }
        public SparseMatrix h1i { get; private set; }
        public SparseMatrix h2i { get; private set; }
        public SparseMatrix d0t { get; private set; }
        public SparseMatrix d1t { get; private set; }
        public SparseMatrix A   { get; private set; }
        public SparseMatrix B   { get; private set; }

        public HodgeDecomposition(HeGeom g) {
            h1 = ExteriorDerivatives.BuildHodgeStar1Form(g);
            h2 = ExteriorDerivatives.BuildHodgeStar2Form(g);
            d0 = ExteriorDerivatives.BuildExteriorDerivative0Form(g);
            d1 = ExteriorDerivatives.BuildExteriorDerivative1Form(g);
            h1i = SparseMatrix.OfDiagonalVector(h1.Diagonal().Map(v => 1 / v));
            h2i = SparseMatrix.OfDiagonalVector(h2.Diagonal().Map(v => 1 / v));
            d0t = SparseMatrix.OfMatrix(d0.Transpose());
            d1t = SparseMatrix.OfMatrix(d1.Transpose());
            var n = d0t.RowCount;
            A = d0t * h1 * d0 + SparseMatrix.CreateDiagonal(n, n, 1e-8);
            B = d1 * h1i * d1t;
        }

        public Vd ComputeExactComponent(Vd omega) => d0 * Solver.Cholesky(A, d0t * h1 * omega);
        public Vd ComputeCoExactComponent(Vd omega) => h1i * d1t * Solver.LU(B, d1 * omega);
        public Vd ComputeHarmonicComponent(Vd omega, Vd exact, Vd coexact) => omega - exact - coexact;
    }
}
