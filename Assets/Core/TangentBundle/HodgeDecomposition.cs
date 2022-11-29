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

        public HodgeDecomposition(HalfEdgeGeom geom) {
            h1 = ExteriorDerivatives.BuildHodgeStar1Form(geom);
            h2 = ExteriorDerivatives.BuildHodgeStar2Form(geom);
            d0 = ExteriorDerivatives.BuildExteriorDerivative0Form(geom);
            d1 = ExteriorDerivatives.BuildExteriorDerivative1Form(geom);
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
            var n = m.RowCount;
            var l = omega.RowCount;
            var outs = new float[n];
            var trps = A.Storage.EnumerateNonZeroIndexed().Select(t => new Triplet(t.Item3, t.Item1, t.Item2)).ToArray();
            var marr = m.Column(0).Map(v => (float)v).ToArray();
            Solver.DecompAndSolveChol(trps.Length, n, trps, marr, outs);
            var mm = DenseMatrix.OfColumnMajor(outs.Length, 1, outs.Select(v => (double)v));
            return (this.d0 * mm).Column(0).ToArray();
        }

        public double[] ComputeCoExactComponent(DenseMatrix omega) {
            var m = d1 * omega;
            var n = m.RowCount;
            var outs = new float[n];
            var trps = B.Storage.EnumerateNonZeroIndexed().Select(t => new Triplet(t.Item3, t.Item1, t.Item2)).ToArray();
            var marr = m.Column(0).Map(v => (float)v).ToArray();
            Solver.DecompAndSolveLU(trps.Length, n, trps, marr, outs);
            var mm = DenseMatrix.OfColumnMajor(outs.Length, 1, outs.Select(v => (double)v));
            //return (h1i * d1t * mm).Column(0).ToArray();
            var lu = B.LU();
            var ans1 = lu.Solve(m);
            return (h1i * d1t * ans1).Column(0).ToArray();
        }

        public float[] ComputeHarmonicComponent(DenseMatrix omega) {
            throw new System.Exception();
        }
    }
}
