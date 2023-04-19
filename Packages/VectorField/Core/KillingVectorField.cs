using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorField {
    using Sprs = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    
    public class KillingVectorField {
        protected HeGeom geom;
        
        public KillingVectorField(HeGeom geom) {
            this.geom = geom;
            var h1  = ExteriorDerivatives.BuildHodgeStar1Form(geom);
            var h2  = ExteriorDerivatives.BuildHodgeStar2Form(geom);
            var d0  = ExteriorDerivatives.BuildExteriorDerivative0Form(geom);
            var d1  = ExteriorDerivatives.BuildExteriorDerivative1Form(geom);
            var h2i = Sprs.OfDiagonalVector(h2.Diagonal().Map(v => 1 / v));
            //var L = Operator.Laplace(geom);
            //var B = h1;
            //var G = Sprs.OfDiagonalArray();
            //var delta = L + d1 * (h2i * d1 * h1) - 2 * B;
        }

        double CurvatureOnEdge()
        {
            throw new System.Exception();
        }
    }
}

