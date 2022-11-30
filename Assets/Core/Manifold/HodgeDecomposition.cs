using MathNet.Numerics.LinearAlgebra.Double;
using System.Linq;

namespace ddg {
    public class HodgeDecomposition {
        SparseMatrix h1;
        SparseMatrix h2;
        SparseMatrix h1i;
        SparseMatrix h2i;
        SparseMatrix d0;
        SparseMatrix d1;
        SparseMatrix d0t;
        SparseMatrix d1t;
        SparseMatrix A;
        SparseMatrix B;

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

        public double[] ComputeExactComponent(DenseMatrix omega) {
            var m = d0t * h1 * omega;
            var outs = new double[m.RowCount];
            var trps = A.Storage.EnumerateNonZeroIndexed().Select(t => new Triplet(t.Item3, t.Item1, t.Item2)).ToArray();
            Solver.DecompAndSolveChol(trps.Length, m.RowCount, trps, m.Column(0).ToArray(), outs);
            var mm = DenseMatrix.OfColumnMajor(outs.Length, 1, outs);
            return (this.d0 * mm).Column(0).ToArray();
        }

        public double[] ComputeCoExactComponent(DenseMatrix omega) {
            var m = d1 * omega;
            var outs = new double[m.RowCount];
            var trps = B.Storage.EnumerateNonZeroIndexed().Select(t => new Triplet(t.Item3, t.Item1, t.Item2)).ToArray();
            Solver.DecompAndSolveLU(trps.Length, m.RowCount, trps, m.Column(0).ToArray(), outs);
            var mm = DenseMatrix.OfColumnMajor(outs.Length, 1, outs);
            return (h1i * d1t * mm).Column(0).ToArray();
        }

        public float[] ComputeHarmonicComponent(DenseMatrix omega) {
            throw new System.Exception();
        }
    }
}
