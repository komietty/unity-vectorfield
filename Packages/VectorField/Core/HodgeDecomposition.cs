using System.Collections.Generic;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Sparse = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;

namespace VectorField {
    public class HodgeDecomposition {
        private readonly HeGeom G;
        private readonly Sparse A;
        private readonly Sparse B;
        private readonly Sparse h1;
        private readonly Sparse d0;
        private readonly Sparse d1;
        private readonly Sparse h1i;
        private readonly Sparse d0t;
        private readonly Sparse d1t;
        public Sparse MatA => A; 
        public Sparse MatH1 => h1; 
        public Sparse MatD0 => d0; 

        public HodgeDecomposition(HeGeom g) {
            G = g;
            h1  = DEC.BuildHodgeStar1Form(G);
            d0  = DEC.BuildExteriorDerivative0Form(G);
            d1  = DEC.BuildExteriorDerivative1Form(G);
            h1i = Sparse.OfDiagonalVector(h1.Diagonal().Map(v => 1 / v));
            d0t = Sparse.OfMatrix(d0.Transpose());
            d1t = Sparse.OfMatrix(d1.Transpose());
            var n = d0t.RowCount;
            A = d0t * h1 * d0 + Sparse.CreateDiagonal(n, n, 1e-8);
            B = d1 * h1i * d1t;
        }

        /*
         * Decompose input one-forms to exact, co-exact, and harmonic elements.
         */
        public Vector ComputeExact   (Vector v) => d0 * Solver.Cholesky(A, d0t * h1 * v);
        public Vector ComputeCoExact (Vector v) => h1i * d1t * Solver.LU(B, d1 * v);
        public Vector ComputeHarmonic(Vector v, Vector e, Vector c) => v - e - c;
        
        /*
         * Compute harmonic element basis using a generator of the target manifold.
         * Build closed primal 1-form first, then subtract exact elements from it.
         */
        public Vector ComputeHarmonicBasis(List<HalfEdge> generator) {
            var oneForm = Vector.Build.Dense(G.nEdges);
            foreach (var h in generator)
                oneForm[h.edge.eid] = h.edge.hid == h.id ? 1 : -1;
            return oneForm - ComputeExact(oneForm);
        }
    }
}
