using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace VectorField {
    using S = SparseMatrix;
    using V = Vector<double>;

    public class ScalarHeatMethod {
        protected HeGeom geom; 
        protected S A; // The laplace matrix of the input mesh
        protected S F; // The mean curvature flow oparator built on the input mesh

        public ScalarHeatMethod(HeGeom geom) {
            this.geom = geom;
            var t = math.pow(geom.MeanEdgeLength(), 2);
            this.A = Operator.Laplace(geom);
            this.F = Operator.Mass(geom) + A * t;
        }
    }
}
