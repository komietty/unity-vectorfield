using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace VectorField {
    using S = SparseMatrix;
    using V = Vector<double>;

    public class TrivialConnectionVert {
        protected HeGeom geom;

        public TrivialConnectionVert(HeGeom geom, HodgeDecomposition hodge) {
            this.geom = geom;
        }

        double TransportNoRotation(HalfEdge h, double alphaI = 0) {
            var u = geom.Vector(h);
            var (e1, e2) = geom.OrthonormalBasis(geom.Verts[h.vid]);
            var (f1, f2) = geom.OrthonormalBasis(geom.Verts[h.twin.vid]);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            return alphaI - thetaIJ + thetaJI;
        }

        Complex32 TransportNoRotationComplex(HalfEdge h, Complex32 alpha) {
            var u = geom.Vector(h);
            var (e1, e2) = geom.OrthonormalBasis(geom.Verts[h.vid]);
            var (f1, f2) = geom.OrthonormalBasis(geom.Verts[h.twin.vid]);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            return new Complex32(1, alpha.Imaginary - thetaIJ + thetaJI);
        }
        
        Complex32 TransportNoRotationComplex(HalfEdge h) {
            var u = geom.Vector(h);
            var (e1, e2) = geom.OrthonormalBasis(geom.Verts[h.vid]);
            var (f1, f2) = geom.OrthonormalBasis(geom.Verts[h.twin.vid]);
            var thetaIJ = atan2(dot(u, e2), dot(u, e1));
            var thetaJI = atan2(dot(u, f2), dot(u, f1));
            return new Complex32(1, -thetaIJ + thetaJI);
        }

        public float3[] GenField(V phi) {
            var visit = new bool[geom.nVerts];
            var alpha = new Complex32[geom.nVerts];
            var field = new float3[geom.nVerts];
            var queue = new Queue<int>();
            var v0 = geom.Verts[0];
            queue.Enqueue(v0.vid);
            alpha[v0.vid] = 0;
            while (queue.Count > 0) {
                var vid = queue.Dequeue();
                foreach (var h in geom.GetAdjacentHalfedges(geom.Verts[vid])) {
                    var gid = h.twin.vid;
                    if (!visit[gid] && gid != v0.vid) {
                        var sign = h.IsEdgeDir() ? 1 : -1;
                        var conn = sign * phi[h.edge.eid];
                        alpha[gid] = TransportNoRotationComplex(h, alpha[vid]);// + conn;
                        visit[gid] = true;
                        queue.Enqueue(gid);
                    }
                }
            } 
            var c = 0;
            foreach (var v in geom.Verts) {
                var a = alpha[v.vid];
                var (e1, e2) = geom.OrthonormalBasis(v);
                field[v.vid] = e1 * (float)cos(a.Imaginary) + e2 * (float)sin(a.Imaginary);
                if (c > 200) break;
                c++;
            }
            return field;
        }
    }
}
