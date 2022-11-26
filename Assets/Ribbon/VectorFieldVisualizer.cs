using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Unity.Mathematics;
using ddg;
using UnityEngine;
using static Unity.Mathematics.math;

public class VectorFieldVisualizer : MonoMfdViewer {
    protected Matrix<double> phi;
    protected DenseMatrix omega; 

    protected override void Start() {
        base.Start();

        omega = DenseMatrix.Create(geom.nEdges, 1, 0.0);
        GenRandomOneForm(geom);

        var n = geom.nFaces;
        var tngs = new Vector3[n * 6];
        var mlen = 0.3f * geom.MeanEdgeLength();
        var omegaField = InterpolateWhitney(omega);
        for(var i = 0; i < n; i++){
            var face = geom.Faces[i];
            var field = omegaField[face] * mlen;
            var C = (float3)geom.Centroid(face);
            geom.FaceNormal(face, out float3 N);
            field = ClampFieldLength(field, mlen);
            var fc1 = C - field + N * 0.005f;
            var fc2 = C + field + N * 0.005f;
            var v = fc2 - fc1;
            var vT = cross(N, v);
            tngs[i * 6 + 0] = fc1;
            tngs[i * 6 + 1] = fc2;
            tngs[i * 6 + 2] = fc2;
            tngs[i * 6 + 3] = fc2 - v * 0.2f + vT * 0.1f;
            tngs[i * 6 + 4] = fc2;
            tngs[i * 6 + 5] = fc2 - v * 0.2f - vT * 0.1f;
        }
        tngBuffer.SetData(tngs);
        nrmMat.SetBuffer("_Tng", tngBuffer);
    }

    protected override void OnRenderObject() {
        nrmMat.SetPass(1);
        Graphics.DrawProceduralNow(MeshTopology.Lines, geom.nFaces * 6);
    }

    protected override float GetValueOnSurface(Vert v) => (float)phi[v.vid, 0]; 

    void Solve(HalfEdgeGeom geom, List<int> vids) {
        var rho = DenseMatrix.Create(geom.nVerts, 1, 0);
        foreach (var i in vids) rho[i, 0] = 1;
        phi = ScalerPoissonProblem.SolveOnSurface(geom, rho);
    }

    void GenRandomOneForm(HalfEdgeGeom geom) { 
        var n = geom.nVerts;
        var r = max(2, (int)(n / 1000.0f));
        //var r = 2;
        var rho1 = DenseMatrix.Create(n, 1, 0);
        var rho2 = DenseMatrix.Create(n, 1, 0);
        for (var i = 0; i < r; i++) {
            var j1 = (int)(UnityEngine.Random.value * r);
            var j2 = (int)(UnityEngine.Random.value * r);
            rho1[j1, 0] = UnityEngine.Random.Range(-2500f, 2500f);
            rho2[j2, 0] = UnityEngine.Random.Range(-2500f, 2500f);
        }
//        var scalarPotential = ScalerPoissonProblem.SolveOnSurface(geom, rho1);
//        var vectorPotential = ScalerPoissonProblem.SolveOnSurface(geom, rho2);

        var scalarPotential = ScalerPoissonProblem.SolveOnSurfaceNative(geom, rho1);
        var vectorPotential = ScalerPoissonProblem.SolveOnSurfaceNative(geom, rho2);

        var field = new Dictionary<Face, float3>();
        for (var i = 0; i < geom.nFaces; i++) {
            var f = geom.Faces[i];
            var v = new float3();
            var N = new float3();
            var A = geom.Area(f);
            var b = geom.FaceNormal(f, out N);
            var C = geom.Centroid(f);
            foreach(var h in geom.GetAdjacentHalfedges(f)) {
                var j = h.prev.vid;
                var e = geom.Vector(h);
                var eT = cross(N, e);
                //v += (eT * (float)scalarPotential[j, 0] + e * (float)vectorPotential[j, 0]) / (2 * A);
                //v += eT * (float)(scalarPotential[j, 0] / (2 * A));
                //v += e * (float)(vectorPotential[j, 0] / (2 * A));
                //v += eT * (float)(scalarPotential[j] / (2 * A));
                //v += e * (float)(vectorPotential[j] / (2 * A));
            }
            var u = new float3(-C.z, 0, C.x);
            u -= N * dot(u, N);
            u = normalize(u);
            v += u * 0.3f;
            field[f] = v;
        }
        for (var i = 0; i < geom.nEdges; i++) {
            var e = geom.Edges[i];
            var h = geom.halfedges[e.hid];
            var f1 = h.onBoundary ?      new float3() : field[h.face]; 
            var f2 = h.twin.onBoundary ? new float3() : field[h.twin.face]; 
            omega[i, 0] = dot(f1 + f2, geom.Vector(h));
            //Debug.Log(omega[i, 0]);
        }
    }

    Dictionary<Face, float3> InterpolateWhitney(DenseMatrix oneForm) {
        var field = new Dictionary<Face, float3>();
        for (var i = 0; i < geom.nFaces; i++) {
            var f = geom.Faces[i];
            var h = geom.halfedges[f.hid];
            var pi = geom.Pos[h.vid];
            var pj = geom.Pos[h.next.vid];
            var pk = geom.Pos[h.prev.vid];
            var eij = pj - pi;
            var ejk = pk - pj;
            var eki = pi - pk;
            var cij = oneForm[h.edge.eid, 0];
            var cjk = oneForm[h.next.edge.eid, 0];
            var cki = oneForm[h.prev.edge.eid, 0];
            if (h.edge.hid != h.id) cij *= -1;
            if (h.next.edge.hid != h.next.id) cjk *= -1;
            if (h.prev.edge.hid != h.prev.id) cki *= -1;
            var A = geom.Area(f);
            var N = new float3();
            var _ = geom.FaceNormal(f, out N);
            var a = (eki - ejk) * (float)cij;
            var b = (eij - eki) * (float)cjk;
            var c = (ejk - eij) * (float)cki;
            field[f] = cross(N, (a + b + c)) / (float)(6 * A);
        }
        return field;
    }

    Vector3 ClampFieldLength(Vector3 field, float length) {
        var norm = field.magnitude;
        return norm > length ? field * length / norm : field;
    }
}
