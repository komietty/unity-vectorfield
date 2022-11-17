using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using UnityEngine;
using ddg;

public class ScalarPoissonProblem : MonoBehaviour
{
    void Start()
    {
        
    }

    /*
     * Computes the solution of the poisson problem Ax = -M(rho - rhoBar),
     * where A is the positive definite laplace matrix and M is the mass matrix.
	 * rho: A scalar density of vertices of the input mesh.
    */
    DenseMatrix Solve(HalfEdgeGeom geom, DenseMatrix rho){
        var M = Operator.Mass(geom);
        var A = Operator.Laplace(geom);
        var T = geom.TotalArea();

        var rhoSum = (M * rho);//sum();
        var rhoBar = DenseMatrix.CreateIdentity(1);
        var rhoDif = M - rhoBar;
        var B = -M * rhoDif;
        var LLT = DenseMatrix.CreateIdentity(1);
        return LLT;
    }
}
