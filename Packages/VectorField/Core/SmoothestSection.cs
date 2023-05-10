using CVector = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;

namespace VectorField {
    public static class SmoothestSection {
        public static CVector Compute(HeGeom G, int fieldDegree = 1) {
            var engy = DEC.ConnectionLaplace(G);
            var mass = DEC.MassComplex(G);
            return Solver.SmallestEigenPositiveDefinite(engy, mass);
        }
        
        /*
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
         */
    }
}