using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine;

namespace ddg {
    public class ScalarPoissonProblemViewer : TangentBundleBehaviour {
        protected override void Start() {
            base.Start();
            UpdateCol(Solve(
                new int[] {
                    Random.Range(0, bundle.Geom.nVerts),
                    Random.Range(0, bundle.Geom.nVerts),
                }));
        }

        float[] Solve(int[] vids) {
            var g = bundle.Geom;
            var r = DenseMatrix.Create(g.nVerts, 1, 0);
            foreach (var i in vids) r[i, 0] = 1;
            return ScalerPoissonProblem.SolveOnSurfaceNative(g, r);
        }
    }
}