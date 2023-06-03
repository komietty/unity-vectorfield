using Unity.Mathematics;
using System.Numerics;
using System.Collections.Generic;
using f3 = Unity.Mathematics.float3;
using f2 = Unity.Mathematics.double2;
using d2 = Unity.Mathematics.double2;
using RS = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
using CS = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
using RV = MathNet.Numerics.LinearAlgebra.Vector<double>;
using static Unity.Mathematics.math;

namespace VectorField {
    public class ConformalParameterization {
        
        /*
        public static f2[] SpectralConformalParameterization(HeGeom G) {
            var ED = DEC.LaplaceComplex(G) * 0.5;
            var T = new List<(int, int, Complex)>();
            foreach(var f in G.Bunds) {
                var s = 0f;
                foreach (var h in G.GetAdjacentHalfedges(f)) {
                    var w = G.EdgeCotan(h.edge); 
                    T.Add((i, h.next.vid, -w));
                    s += w;
                }
                T.Add((i, i, s));
            }
            var A = CS.OfIndexed(ED.RowCount, ED.ColumnCount, T);
            var EC = ED - A;
            var z = Solver.SmallestEigenPositiveDefinite(EC);
        }
         */
    }
}