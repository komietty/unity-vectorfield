using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace VectorField {
    using S = SparseMatrix;

    public class VectorHeatMethod {
        protected HeGeom geom;
        protected ScalarHeatMethod shm;

        public VectorHeatMethod(HeGeom geom) {
            this.geom = geom;
        }
        
        // must have right direction, but not magnitude
        public DenseMatrix ComputeVectorHeatFlow() {
            throw new System.Exception();
        }

        public static S ConnectionLaplace(HeGeom geom){
            var t = new List<(int, int, double)>();
            var n = geom.nVerts;
            for (var i = 0; i < n; i++) {
                var v = geom.Verts[i];
                var s = 0f;
                foreach (var h in geom.GetAdjacentHalfedges(v)) {
                    var a = geom.Cotan(h);
                    var b = geom.Cotan(h.twin);
                    var c = (a + b) * 0.5f;
                    t.Add((i, h.next.vid, -c));
                    s += c;
                }
                t.Add((i, i, s));
            }
            var M = S.OfIndexed(n, n, t);
            var C = S.CreateDiagonal(n, n, 1e-8d);
            return M + C;
        }
    }
}
