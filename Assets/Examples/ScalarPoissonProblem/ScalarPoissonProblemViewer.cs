using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine;
using ddg;

public class ScalarPoissonProblemViewer : MonoMfdViewer {
    protected float[] phi;
    protected override float GetValueOnSurface(Vert v) => phi[v.vid]; 

    protected override void Start() {
        base.Start();
        Solve(geom, new List<int> { 0 });
        UpdateColor();
    }

    void Solve(HeGeom geom, List<int> vids) {
        var rho = DenseMatrix.Create(geom.nVerts, 1, 0);
        foreach (var i in vids) rho[i, 0] = 1;
        phi = ScalerPoissonProblem.SolveOnSurfaceNative(geom, rho);
    }
}
