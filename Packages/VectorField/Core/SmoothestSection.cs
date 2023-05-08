using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;

namespace VectorField {
    using static math;
    using RVector = MathNet.Numerics.LinearAlgebra.Vector<double>;
    using CVector = MathNet.Numerics.LinearAlgebra.Vector<Complex>;
    using RSparse = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using CSparse = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
    using RDense = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
    
    public class SmoothestSection
    {
        private readonly int fieldDegree = 1;
        private readonly HeGeom G;
        public CVector fileds;
        
        public SmoothestSection(HeGeom g) {
            G = g;
            //var angl = VertexPolarAngleForEachHe();
            //var engy = BuildFeildEnergy(angl);
            var engy = Operator.ConnectionLaplace(G);
            var mass = Operator.MassComplex(G);
            fileds = Solver.SmallestEigenPositiveDefinite(engy, mass);
        }
        
        RVector VertexPolarAngleForEachHe() {
            var angles = RVector.Build.Dense(G.halfedges.Length);
            foreach (var v in G.Verts) {
                var a = 0f;
                var proj2plane = 2 * PI / G.AngleSum(v);
                foreach (var h in G.GetAdjacentHalfedges(v)) {
                    var v1 = normalize(G.Vector(h));
                    var v2 = normalize(G.Vector(h.twin.next));
                    a += acos(dot(v1, v2)) * proj2plane;
                    angles[h.id] = a;
                }
            }
            return angles;
        }
        
        CSparse BuildFeildEnergy(IList<double> angles) {
            var A = CSparse.Create(G.nVerts, G.nVerts, 0);
            foreach (var f in G.Faces) {
                foreach (var h in G.GetAdjacentHalfedges(f)) {
                    int i = h.vid;
                    int j = h.twin.vid;
                    var w = G.Cotan(h);
                    var thetaI = angles[h.id];
                    var thetaJ = angles[h.twin.id];
                    var phi = fieldDegree * (thetaI - thetaJ + PI);
                    var r = new Complex(cos(phi), sin(phi));
                    A[i, i] += w;
                    A[i, j] -= w * r;
                    A[j, j] += w;
                    A[j, i] -= Utility.Div(w, r);
                }
            }
            return A;
        }
        
        public static float3[] ComputeVertVectorField(HeGeom g, CVector connection) {
            var field = new float3[g.nVerts];
            foreach (var v in g.Verts) {
                var (e1, e2) = g.OrthonormalBasis(v);
                var c = connection[v.vid];
                var l = sqrt(pow(c.Real, 2) + pow(c.Imaginary, 2));
                var s = c / l;
                field[v.vid] = e1 * (float)s.Real + e2 * (float)s.Imaginary;
            }
            return field;
        }
    }
}