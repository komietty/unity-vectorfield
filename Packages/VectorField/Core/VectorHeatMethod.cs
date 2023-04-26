using System.Collections.Generic;
using Unity.Mathematics;

namespace VectorField {
    using static math;
    using RV = MathNet.Numerics.LinearAlgebra.Vector<double>;
    using CV = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;

    public static class VectorHeatMethod {
        
        public static RV ComputeExtendedScalar(HeGeom g, List<(int vid, double val)> src) {
            var dataRhs = RV.Build.Dense(g.nVerts);
            var idctRhs = RV.Build.Dense(g.nVerts);
            foreach (var s in src) {
                dataRhs[s.vid] = s.val;
                idctRhs[s.vid] = 1;
            }
            var L = Operator.Laplace(g);
            var F = Operator.Mass(g) + L * pow(g.MeanEdgeLength(), 2);
            var dataSol = Solver.LU(F, dataRhs);
            var idctSol = Solver.LU(F, idctRhs);
            var result = RV.Build.Dense(g.nVerts);
            for (var i = 0; i < g.nVerts; i++) {
                var data = dataSol[i];
                var idct = idctSol[i];
                result[i] = data / idct;
            }
            return result;
        }
        
        public static CV ComputeVectorHeatFlow(HeGeom g, CV phi) {
            var t = pow(g.MeanEdgeLength(), 2);
            var L = Operator.ConnectionLaplace(g);
            var F = Operator.MassComplex(g) + L * t;
            var C = Solver.LUComp(F,phi);
            return C;
        }

        public static float3[] ComputeVertVectorField(HeGeom g, CV connection, RV magnitude) {
            var field = new float3[g.nVerts];
            foreach (var v in g.Verts) {
                var (e1, e2) = g.OrthonormalBasis(v);
                var c = connection[v.vid];
                var m = magnitude[v.vid];
                var l = sqrt(pow(c.Real, 2) + pow(c.Imaginary, 2));
                var s = c / l * m;
                field[v.vid] = e1 * (float)s.Real + e2 * (float)s.Imaginary;
            }
            return field;
        }
    }
}
