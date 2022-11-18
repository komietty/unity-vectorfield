using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using ddg;

public class ScalarPoissonProblemViewer : MonoMfdViewer {
    protected Matrix<double> phi;

    protected override void Start() {
        base.Start();
        Solve(geom, new List<int> { 0, 5 });
        UpdateColor();
    }

    protected override float GetValueOnSurface(Vert v) => (float)phi[v.vid, 0]; 

    void Solve(HalfEdgeGeom geom, List<int> vids) {
        var rho = DenseMatrix.Create(geom.nVerts, 1, 0);
        foreach (var i in vids) rho[i, 0] = 1;
        phi = ScalerPoissonProblem.SolveOnSurface(geom, rho);
    }

    void GenRandomOneForm() { }
}
