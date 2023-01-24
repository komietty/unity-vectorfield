using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace VectorField {
    using DS = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;
    using DD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
    using CS = MathNet.Numerics.LinearAlgebra.Complex32.SparseMatrix;

    public class VectorHeatMethod {
        protected HeGeom geom;
        protected ScalarHeatMethod shm;

        public VectorHeatMethod(HeGeom geom) {
            this.geom = geom;
        }
        
        // must have right direction, but not magnitude
        public DD ComputeVectorHeatFlow() {
            throw new System.Exception();
        }

        // https://numerics.mathdotnet.com/api/MathNet.Numerics/Complex32.htm
        public static CS ConnectionLaplace(HeGeom geom){
            var t = new List<(int, int, Complex32)>();
            var n = geom.nVerts;
            for (var i = 0; i < n; i++) {
                var v = geom.Verts[i];
                var s = 0f;
                foreach (var h in geom.GetAdjacentHalfedges(v)) {
                    var a = geom.Cotan(h);
                    var b = geom.Cotan(h.twin);
                    var c = (a + b) * 0.5f;
                    t.Add((i, h.next.vid, new Complex32(-c, 0)));
                    s += c;
                }
                t.Add((i, i, s));
            }
            var M = CS.OfIndexed(n, n, t);
            var C = CS.CreateDiagonal(n, n, new Complex32(1e-8f, 0));
            return M + C;
        }
    }
}
