using MathNet.Numerics.LinearAlgebra.Double;
using System.Linq;

namespace ddg {
    public class HodgeDecomposition {
        public SparseMatrix h1 {get; private set;}
        public SparseMatrix h2 {get; private set;}
        public SparseMatrix d0 {get; private set;}
        public SparseMatrix d1 {get; private set;}
        SparseMatrix h1i;
        SparseMatrix h2i;
        SparseMatrix d0t;
        SparseMatrix d1t;
        SparseMatrix A;
        SparseMatrix B;
        public SparseMatrix ZeroFromLaplaceMtx => A;

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

        public DenseMatrix ComputeExactComponent(DenseMatrix omega) {
            var m = d0t * h1 * omega;
            var outs = new double[m.RowCount];
            var trps = A.Storage.EnumerateNonZeroIndexed().Select(t => new Triplet(t.Item3, t.Item1, t.Item2)).ToArray();
            Solver.DecompAndSolveChol(trps.Length, m.RowCount, trps, m.Column(0).ToArray(), outs);
            return (DenseMatrix)(this.d0 * DenseMatrix.OfColumnMajor(outs.Length, 1, outs));
        }

        public DenseMatrix ComputeCoExactComponent(DenseMatrix omega) {
            var m = d1 * omega;
            var outs = new double[m.RowCount];
            var trps = B.Storage.EnumerateNonZeroIndexed().Select(t => new Triplet(t.Item3, t.Item1, t.Item2)).ToArray();
            Solver.DecompAndSolveLU(trps.Length, m.RowCount, trps, m.Column(0).ToArray(), outs);
            return (DenseMatrix)(h1i * d1t * DenseMatrix.OfColumnMajor(outs.Length, 1, outs));
        }

        public DenseMatrix ComputeHarmonicComponent(DenseMatrix omega, DenseMatrix exact, DenseMatrix coexact) {
            return omega - exact - coexact;
        }
    }
}
