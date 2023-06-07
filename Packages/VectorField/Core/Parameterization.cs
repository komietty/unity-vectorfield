using System.Numerics;
using System.Collections.Generic;
using f3 = Unity.Mathematics.float3;
using f2 = Unity.Mathematics.double2;
using d2 = Unity.Mathematics.double2;
using RS = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
using CS = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
using RV = MathNet.Numerics.LinearAlgebra.Vector<double>;
using CV = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;

namespace VectorField {
    public static class Parameterization {
        public static CV SpectralConformal(HeGeom G) {
            var ED = DEC.LaplaceComplex(G) * 0.5;
            var T = new List<(int, int, Complex)>();
            foreach(var f in G.Bunds) {
                var h = G.halfedges[f.hid];
                T.Add((h.vid, h.twin.vid, new Complex(0,  0.25)));
                T.Add((h.twin.vid, h.vid, new Complex(0, -0.25)));
            }
            var EC = ED - CS.OfIndexed(ED.RowCount, ED.ColumnCount, T);
            return Solver.InversePowerMethod(EC);
        }
    }
}