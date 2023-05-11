using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics;
using static Unity.Mathematics.math;
using RVector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using CVector = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;
using RSparse = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
using RDense  = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;

namespace VectorField {
    
    public class StripePattern {
        private readonly HeGeom G;
        private readonly RVector angle;
        private readonly RVector crossSheets;
        private readonly CVector parameterization;
        private readonly CVector directionalField;
        //private readonly double fieldDegree = 2;
        private readonly double lambda = 130; // initial global line frequency
        private readonly RSparse energyMatrix;
        private readonly RSparse massMatrix;
        
        private static float Dot(Complex a, Complex b) => (float)(a.Real * b.Real - a.Imaginary * b.Imaginary);
        private static float Arg(Complex a) => (float)atan2(a.Imaginary, a.Real);

        private Complex CanonicalVector(Vert v) {
            var r = directionalField[v.vid].Norm();
            var theta = Arg(directionalField[v.vid]) * 0.5;
            return r * new Complex(cos(theta), sin(theta));
        }

        public StripePattern(HeGeom g, CVector X) {
            G = g;
            angle = VertexPolarAngleForEachHe();
            crossSheets = RVector.Build.Dense(G.nEdges, 1);
            directionalField = X;
            parameterization = CVector.Build.Dense(G.nVerts);
            energyMatrix = ComputeEnegyMatrix(angle);
            massMatrix = ComputeMassMatrix();
        }

        /**
         * Algorithm 2.
         * Be sure h is inserted only once.
         */ 
        public RVector VertexPolarAngleForEachHe() {
            var angle = RVector.Build.Dense(G.halfedges.Length);
            foreach (var v in G.Verts) {
                var a = 0f;
                var proj2plane = 2 * PI / G.AngleSum(v);
                foreach (var h in G.GetAdjacentHalfedges(v)) {
                    var v1 = normalize(G.Vector(h));
                    var v2 = normalize(G.Vector(h.twin.next));
                    a += acos(dot(v1, v2)) * proj2plane;
                    angle[h.id] = a;
                }
            }
            return angle;
        }
        
        /* 
         * Algorithm 3.
         * X: tangent field in complex number on verts
         * v: target frequency on verts
        public (RVector sign, RVector omega) InitializeEdgeData(RVector angles, CVector X, RVector v) {
            var sign  = RVector.Build.Dense(G.nEdges);
            var omega = RVector.Build.Dense(G.nEdges);
            foreach (var e in G.Edges) {
                var hi = G.halfedges[e.hid];
                var hj = G.halfedges[e.hid].twin;
                var vi = hi.vid;
                var vj = hj.vid;
                var rho_ij = -angles[hi.id] + angles[hj.id] + PI;
                var sgn_ij = math.sign(Dot(new Complex(cos(rho_ij), sin(rho_ij)) * X[vi], X[vj]));
                var phi_i = Arg(X[vi]); //check
                var phi_j = Arg(sgn_ij * X[vj]); //check
                var l = G.Length(G.halfedges[e.hid]);
                var omega_ij = l * 0.5 * (
                    v[vi] * cos(phi_i - angles[hi.id]) +
                    v[vj] * cos(phi_j - angles[hj.id])
                );
                sign[e.eid] = sgn_ij;
                omega[e.eid] = omega_ij;
            }
            return (sign, omega);
        }
         */

        
        /**
         * Algorithm 3.
         * Algorithm 4.
         */
        public RSparse ComputeEnegyMatrix(RVector angle) {
            var nv = G.nVerts;
            var A = RDense.Create(nv * 2, nv * 2, 0);
            foreach (var e in G.Edges) {
                var h = G.halfedges[e.hid];
                var vi = G.Verts[h.vid];
                var vj = G.Verts[h.twin.vid];
                var thetaI = angle[h.vid];
                var thetaJ = angle[h.twin.vid] + PI;
                var dTheta = thetaJ - thetaI;
                var r_ij = new Complex(cos(dTheta), sin(dTheta));
                
                var cotAlpha = G.Cotan(h);
                var cotBeta  = G.Cotan(h.twin);
                //if(       e->he->face->fieldIndex(2.) != 0 ) cotAlpha = 0.;
                //if( e->he->flip->face->fieldIndex(2.) != 0 ) cotBeta  = 0.;
                var w = (cotAlpha + cotBeta) * 0.5;
                var Xi = CanonicalVector(vi);
                var Xj = CanonicalVector(vj);
                var s = Dot(r_ij * Xi, Xj) > 0 ? 1 : -1;
                // if (fieldDegree == 1) s = 1;
                if (s > 0) crossSheets[e.eid] = -1;

                var lij = G.Length(h);
                var phiI = Arg(Xi);
                var phiJ = Arg(s * Xj);
                var omegaIJ = lambda * lij * 0.5 * (cos(phiI-thetaI) + cos(phiJ-thetaJ));
                var a = w * cos(omegaIJ);
                var b = w * sin(omegaIJ);
                
                var i = vi.vid * 2;
                var j = vj.vid * 2;
                
                A[i + 0, i + 0] += w;
                A[i + 1, i + 1] += w;
                A[j + 0, j + 0] += w;
                A[j + 1, j + 1] += w;
                
                if (s > 0) {
                    A[i+0,j+0] = -a; A[i+0,j+1] = -b;
                    A[i+1,j+0] =  b; A[i+1,j+1] = -a;
                    A[j+0,i+0] = -a; A[j+0,i+1] =  b;
                    A[j+1,i+0] = -b; A[j+1,i+1] = -a;
                }
                else {
                    A[i+0,j+0] = -a; A[i+0,j+1] =  b;
                    A[i+1,j+0] =  b; A[i+1,j+1] =  a;
                    A[j+0,i+0] = -a; A[j+0,i+1] =  b;
                    A[j+1,i+0] =  b; A[j+1,i+1] =  a;

                }
            }
            return RSparse.OfMatrix(A);
        }
        
        /**
         * Algorithm 5.
         */
        RSparse ComputeMassMatrix() {
            var n = G.nVerts;
            var T = new List<(int, int, double)>();
            foreach (var v in G.Verts) {
                var i = v.vid;
                var dualArea = G.BarycentricDualArea(v);
                T.Add((i * 2 + 0, i * 2 + 1, dualArea));
                T.Add((i * 2 + 0, i * 2 + 1, dualArea));
            }
            return RSparse.OfIndexed(n * 2, n * 2, T);
        }

        /**
         * Algorithm 6.
         */
        public CVector ComputeParameterization(RSparse energyMatrix, RSparse massMatrix) {
            var nv = G.nVerts;
            var groundStateC = CVector.Build.Dense(nv, 0);
            var groundStateR = Solver.SmallestEigenPositiveDefinite(energyMatrix, massMatrix);
            for (var i = 0; i < nv; i++) {
                groundStateC[i] = new Complex(
                    groundStateR[i * 2 + 0],
                    groundStateR[i * 2 + 1]
                );
            }
            return groundStateC;
        }

        /**
         * Algorithm 7.
         */
        public void AssignTextureCoordinate(int p) {
            foreach (var f in G.Faces)
            {
                var h_ij = G.halfedges[f.hid];
                var h_jk = h_ij.next;
                var h_ki = h_jk.next;
                var psiI = parameterization[h_ij.vid];
                var psiJ = parameterization[h_jk.vid];
                var psiK = parameterization[h_ki.vid];
                
                var cIJ = h_ij.IsCanonical() ? 1 : -1;
                var cJK = h_jk.IsCanonical() ? 1 : -1;
                var cKI = h_ki.IsCanonical() ? 1 : -1;
                
                /*
                // grab the connection coeffients
                double omegaIJ = cIJ * h_ij.edge->omega;
                double omegaJK = cJK * h_jk.edge->omega;
                double omegaKI = cKI * h_ki.edge->omega;

                if( crossSheets[h_ij.edge.eid] > 0) {
                    psiJ = psiJ.Conjugate(); //bar?
                    omegaIJ =  cIJ * omegaIJ;
                    omegaJK = -cJK * omegaJK;
                }
                if( crossSheets[h_ki.edge.eid] > 0 ) {
                    psiK = psiK.Conjugate(); // bar?
                    omegaKI = -cKI * omegaKI;
                    omegaJK =  cJK * omegaJK;
                }
                
                // construct complex transport coefficients
                var rij = new Complex(cos(omegaIJ), sin(omegaIJ));
                var rjk = new Complex(cos(omegaJK), sin(omegaJK));
                var rki = new Complex(cos(omegaKI), sin(omegaKI));

                // compute the angles at the triangle corners closest to the target omegas
                double alphaI = Arg(psiI);
                double alphaJ = alphaI + omegaIJ - Arg(rij*psiI/psiJ); //fmodPI((varphiI + omegaIJ) - varphiJ); // could do this in terms of angles instead of complex numbers...
                double alphaK = alphaJ + omegaJK - Arg(rjk*psiJ/psiK); //fmodPI((varphiJ + omegaJK) - varphiK); // mostly a matter of taste---possibly a matter of performance?
                double alphaL = alphaK + omegaKI - Arg(rki*psiK/psiI); //fmodPI((varphiK + omegaKI) - varphiI);

                // adjust triangles containing zeros
                double n = lround((alphaL-alphaI)/(2 * PI));
                alphaJ -= 2 * PI * n / 3;
                alphaK -= 4 * PI * n / 3;

                // store the coordinates
                h_ij->texcoord[p] = alphaI;
                h_jk->texcoord[p] = alphaJ;
                h_ki->texcoord[p] = alphaK;
                f->paramIndex[p] = n;
                */
            }
        }
    }
}