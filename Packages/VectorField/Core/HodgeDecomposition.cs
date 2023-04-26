using System.Collections.Generic;

namespace VectorField {
    using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
    using Sparse = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    
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

        public HodgeDecomposition(HeGeom g) {
            G = g;
            h1  = ExteriorDerivatives.BuildHodgeStar1Form(G);
            d0  = ExteriorDerivatives.BuildExteriorDerivative0Form(G);
            d1  = ExteriorDerivatives.BuildExteriorDerivative1Form(G);
            h1i = Sparse.OfDiagonalVector(h1.Diagonal().Map(v => 1 / v));
            d0t = Sparse.OfMatrix(d0.Transpose());
            d1t = Sparse.OfMatrix(d1.Transpose());
            var n = d0t.RowCount;
            A = d0t * h1 * d0 + Sparse.CreateDiagonal(n, n, 1e-8);
            B = d1 * h1i * d1t;
        }

        /*
         * Decompose input one-forms to exact, coexact, and hamonic elements.
         */
        public Vector ComputeExact   (Vector v) => d0 * Solver.Cholesky(A, d0t * h1 * v);
        public Vector ComputeCoExact (Vector v) => h1i * d1t * Solver.LU(B, d1 * v);
        public Vector ComputeHarmonic(Vector v, Vector e, Vector c) => v - e - c;
        
        /*
         * Compute hamonic element basis using generators of the target manifold.
         */
        public List<Vector> ComputeHamonicBasis(List<List<HalfEdge>> generators) {
            var l = new List<Vector>();
            if (generators.Count == 0) return l;
            foreach (var h in generators) {
                var o = BuildClosedPrimalOneForm(h);
                var e = ComputeExact(o);
                l.Add(o - e);
            }
            return l;
        }
        
        Vector BuildClosedPrimalOneForm(List<HalfEdge> generator) {
            var v = Vector.Build.Dense(G.nEdges);
            foreach (var h in generator)
                v[h.edge.eid] = h.edge.hid == h.id ? 1 : -1;
            return v;
        } 
    }
}
