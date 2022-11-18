using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using UnityEngine;
using ddg;

public class ScalarPoissonProblem : MonoMfdViewer {
    protected Matrix<double> phi;

    protected override void Start() {
        base.Start();
        SolveScalarPoissonProblem(geom, new List<int> { 0, 5 });
        UpdateColor();
    }

    /*
     * Computes the solution of the poisson problem Ax = -M(rho - rhoBar),
     * where A is the positive definite laplace matrix and M is the mass matrix.
	 * rho: A scalar density of vertices of the input mesh.
    */
    Matrix<double> Solve(HalfEdgeGeom geom, DenseMatrix rho){
        var M = Operator.Mass(geom);
        var A = Operator.Laplace(geom);
        var T = geom.TotalArea();
        var rhoSum = (M * rho).RowSums().Sum();
        var rhoBar = DenseMatrix.Create(M.RowCount, 1, rhoSum / T);
        var rhoDif = rho - rhoBar;
        var B = - M * rhoDif;
        var evd = A.Evd();
        var ev = evd.EigenValues;
        var LLT = A.LU();
        return LLT.Solve(B);
        //var LLT = A.Cholesky(); // must be very naive chol decomp
        //var data = SparseCompressedRowMatrixStorage<double>.OfMatrix(A.Storage);
        //var iter = data.EnumerateNonZeroIndexed();
        //var outs = new Vector3[geom.nVerts];
        //var trps = iter.Select(i => new Triplet(i.Item3, i.Item1, i.Item2)).ToArray();
        //Solver.SolveLU(iter.Count(), geom.nVerts, trps, geom.pos, outs);
    }

    void SolveScalarPoissonProblem(HalfEdgeGeom geom, List<int> vertexIds) {
        var rho = DenseMatrix.Create(geom.nVerts, 1, 0);
        foreach(var i in vertexIds) rho[i, 0] = 1;
        phi = Solve(geom, rho);
    }

    protected override float GetValueOnSurface(Vert v) => (float)phi[v.vid, 0]; 

    void GenRandomOneForm() {

    }
}
