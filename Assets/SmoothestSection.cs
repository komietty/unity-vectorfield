using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;

namespace VectorField
{
    using static math;
    using RVector = MathNet.Numerics.LinearAlgebra.Vector<double>;
    using CVector = MathNet.Numerics.LinearAlgebra.Vector<Complex>;
    using RSparse = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using CSparse = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
    using RDense = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
    
    public class SmoothestSection
    {
        private int fieldDegree = 1;
        private HeGeom G;
        private RVector angles;
        private CVector fileds;
        
        Complex Div(Complex u, Complex v) 
        {
            var d = v.Real * v.Real + v.Imaginary * v.Imaginary;
            return new Complex(
                u.Real * v.Real + u.Imaginary * v.Imaginary,
                u.Imaginary * v.Real - u.Real * v.Imaginary
                ) / d;
        }
        
        public SmoothestSection()
        {
            var engy = BuildFeildEnergy();
            var mass = Operator.MassComplex(G);
            var vals = Solver.SmallestEigenPositiveDefinite(engy, mass);
        }
        
        public RVector VertexPolarAngleForEachHe() {
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
        
        public CSparse BuildFeildEnergy() {
            var k = fieldDegree;
            var A = CSparse.Create(G.nVerts, G.nVerts, 0);
            foreach (var f in G.Faces) {
                foreach (var h in G.GetAdjacentHalfedges(f)) {
                    int i = h.vid;
                    int j = h.twin.vid;
                    var w = G.Cotan(h);
                    var thetaI = angles[h.id];
                    var thetaJ = angles[h.twin.id];
                    var phi = k * (thetaI - thetaJ + PI);
                    var r = new Complex(cos(phi), sin(phi));
                    A[i, i] += w;
                    A[i, j] -= w * r;
                    A[j, j] += w;
                    A[j, i] -= Div(w, r);
                }
            }
            return A;
        }
    }
}