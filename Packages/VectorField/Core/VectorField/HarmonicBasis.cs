using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;

namespace VectorField {
    using V = Vector<double>;

    public class HamonicBasis {
        protected HeGeom geom;

        public HamonicBasis(HeGeom geom) { this.geom = geom; }

        public V BuildClosedPrimalOneForm(List<HalfEdge> generator) {
            var n = geom.nEdges;
            var d = new double[n];
            foreach (var h in generator) d[h.edge.eid] = h.edge.hid == h.id ? 1 : -1;
            return DenseVector.OfArray(d);
        } 

        public List<V> Compute(HodgeDecomposition hd, List<List<HalfEdge>> generators) {
            var gammas = new List<V>();
            if (generators.Count > 0) {
                foreach (var g in generators) {
                    var omega  = BuildClosedPrimalOneForm(g);
                    var dAlpha = hd.Exact(omega);
                    gammas.Add(omega - dAlpha);
                }
            }
            return gammas;
        }
    }
}