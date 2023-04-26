using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;

namespace VectorField {
    using Vecd = MathNet.Numerics.LinearAlgebra.Vector<double>;

    public static class HamonicBasis {
        
        static Vecd BuildClosedPrimalOneForm(
            HeGeom geom,
            List<HalfEdge> gen
            ) {
            var n = geom.nEdges;
            var d = new double[n];
            foreach (var h in gen) d[h.edge.eid] = h.edge.hid == h.id ? 1 : -1;
            return DenseVector.OfArray(d);
        } 

        public static List<Vecd> Compute(
            HeGeom geom,
            HodgeDecomposition hd,
            List<List<HalfEdge>> gens) {
            var l = new List<Vecd>();
            if (gens.Count > 0) {
                foreach (var g in gens) {
                    var omega = BuildClosedPrimalOneForm(geom, g);
                    var exact = hd.Exact(omega);
                    l.Add(omega - exact);
                }
            }
            return l;
        }
    }
}