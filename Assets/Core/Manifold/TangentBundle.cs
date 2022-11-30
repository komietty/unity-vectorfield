using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using MathNet.Numerics.LinearAlgebra.Double;
using static Unity.Mathematics.math;

namespace ddg {
    public class TangentBundle {
        protected float3[] tangentField;
        protected HeGeom geom;
        public HeGeom Geom => geom;
        public float3[] TangentField => tangentField;

        public TangentBundle(Mesh mesh){
            geom = new HeGeom(mesh);
        }

        public static double[] GenRandomOneForm(HeGeom geom) {
            var nv = geom.nVerts;
            var r = max(2, (int)(nv / 1000f));
            var rho1 = DenseMatrix.Create(nv, 1, 0);
            var rho2 = DenseMatrix.Create(nv, 1, 0);
            for (var i = 0; i < r; i++) {
                var j1 = (int)(UnityEngine.Random.value * nv);
                var j2 = (int)(UnityEngine.Random.value * nv);
                rho1[j1, 0] = UnityEngine.Random.Range(-2500f, 2500f);
                rho2[j2, 0] = UnityEngine.Random.Range(-2500f, 2500f);
            }
            var scalarPotential = ScalerPoissonProblem.SolveOnSurfaceNative(geom, rho1);
            var vectorPotential = ScalerPoissonProblem.SolveOnSurfaceNative(geom, rho2);

            var field = new Dictionary<Face, float3>();
            for (var i = 0; i < geom.nFaces; i++) {
                var f = geom.Faces[i];
                var v = new float3();
                var a = geom.Area(f);
                var (_, n) = geom.FaceNormal(f);
                foreach (var h in geom.GetAdjacentHalfedges(f)) {
                    var j = h.prev.vid;
                    var e  = geom.Vector(h);
                    var eT = cross(n, e);
                    v += eT * (float)(scalarPotential[j] / (2 * a));
                    v += e  * (float)(vectorPotential[j] / (2 * a));
                }
                var c = geom.Centroid(f);
                var u = new float3(-c.z, 0, c.x);
                u -= n * dot(u, n);
                u = normalize(u);
                v += u * 0.3f;
                field[f] = v;
            }
            var omega = new double[geom.nEdges];
            for (var i = 0; i < geom.nEdges; i++) {
                var e = geom.Edges[i];
                var h = geom.halfedges[e.hid];
                var f1 = h.onBoundary ? new float3() : field[h.face];
                var f2 = h.twin.onBoundary ? new float3() : field[h.twin.face];
                omega[i] = dot(f1 + f2, geom.Vector(h));
            }
            return omega;
        }

        public static float3[] InterpolateWhitney(double[] oneForm, HeGeom geom) {
            var field = new float3[geom.nFaces];
            for (var i = 0; i < geom.nFaces; i++) {
                var f = geom.Faces[i];
                var h = geom.halfedges[f.hid];
                var pi = geom.Pos[h.vid];
                var pj = geom.Pos[h.next.vid];
                var pk = geom.Pos[h.prev.vid];
                var eij = pj - pi;
                var ejk = pk - pj;
                var eki = pi - pk;
                var cij = oneForm[h.edge.eid];
                var cjk = oneForm[h.next.edge.eid];
                var cki = oneForm[h.prev.edge.eid];
                if (h.edge.hid != h.id) cij *= -1;
                if (h.next.edge.hid != h.next.id) cjk *= -1;
                if (h.prev.edge.hid != h.prev.id) cki *= -1;
                var A = geom.Area(f);
                var (_, N) = geom.FaceNormal(f);
                var a = (eki - ejk) * (float)cij;
                var b = (eij - eki) * (float)cjk;
                var c = (ejk - eij) * (float)cki;
                field[i] = math.cross(N, (a + b + c)) / (float)(6 * A);
            }
            return field;
        }
    }
}