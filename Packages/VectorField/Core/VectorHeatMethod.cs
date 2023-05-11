using System.Collections.Generic;
using Unity.Mathematics;
using RVector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using CVector = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;
using static Unity.Mathematics.math;

namespace VectorField {
    public class VectorHeatMethod {
        private readonly HeGeom G;
        public VectorHeatMethod(HeGeom g) { G = g; }

        /*
         * Compute magnitude
         * t: time of source point spreads 
         */
        public RVector ComputeExtendedScalar(List<(int vid, double val)> src) {
            var dataRhs = RVector.Build.Dense(G.nVerts);
            var idctRhs = RVector.Build.Dense(G.nVerts);
            foreach (var s in src) {
                dataRhs[s.vid] = s.val;
                idctRhs[s.vid] = 1;
            }
            var t = pow(G.MeanEdgeLength(), 2);
            var laplace = DEC.Laplace(G);
            var flowMatrix = DEC.Mass(G) + laplace * t;
            var dataSol = Solver.LU(flowMatrix, dataRhs);
            var idctSol = Solver.LU(flowMatrix, idctRhs);
            var magnitude = RVector.Build.Dense(G.nVerts);
            for (var i = 0; i < G.nVerts; i++) {
                var data = dataSol[i];
                var idct = idctSol[i];
                magnitude[i] = data / idct;
            }
            return magnitude;
        }
        
        /*
         * Compute connection
         * t: time of source point spreads 
         */
        public CVector ComputeVectorHeatFlow(CVector phi) {
            var t = pow(G.MeanEdgeLength(), 2);
            var conLaplace = DEC.ConnectionLaplace(G);
            var flowMatrix = DEC.MassComplex(G) + conLaplace * t;
            var connection = Solver.LUComp(flowMatrix, phi);
            return connection;
        }
        
        /*
         * Compute vector field from conneciton and mangnitude
         */
        public float3[] ComputeVectorField(CVector connection, RVector magnitude) {
            var X = new float3[G.nVerts];
            foreach (var v in G.Verts) {
                var (e1, e2) = G.OrthonormalBasis(v);
                var c = connection[v.vid];
                var m = magnitude[v.vid];
                var l = sqrt(pow(c.Real, 2) + pow(c.Imaginary, 2));
                var s = c / l * m;
                X[v.vid] = e1 * (float)s.Real + e2 * (float)s.Imaginary;
            }
            return X;
        }
    }
}
