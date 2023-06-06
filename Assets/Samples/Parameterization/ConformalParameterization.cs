using Unity.Mathematics;
using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using f3 = Unity.Mathematics.float3;
using f2 = Unity.Mathematics.double2;
using d2 = Unity.Mathematics.double2;
using RS = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
using CS = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
using RV = MathNet.Numerics.LinearAlgebra.Vector<double>;
using CV = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;
using static Unity.Mathematics.math;

namespace VectorField {
    public class Parameterization {
        
        /*
         * check:
         * 1: laplaceComplex matrix
         * 2: boundary
         * 3: inversepowermethod
         */
        public static CV SpectralConformal(HeGeom G) {
            var Ed = DEC.LaplaceComplex(G) * 0.5;
            var T = new List<(int, int, Complex)>();
            foreach(var f in G.Bunds) {
                var h = G.halfedges[f.hid];
                var v = new Complex(0, 1);
                var i = h.vid;
                var j = h.twin.vid;
                T.Add((i, j, v *  0.25));
                T.Add((j, i, v * -0.25));
            }
            var nr = Ed.RowCount;
            var nc = Ed.ColumnCount;
            var Ec = Ed - CS.OfIndexed(nr, nc, T);
            //var z = Solver.SmallestEigenPositiveDefinite(Ec, CS.CreateIdentity(nr));
            var z = Solver.InversePowerMethod(Ec);
            return z;
        }
    }
}