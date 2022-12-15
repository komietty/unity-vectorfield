using UnityEngine;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ddg {
    public class HamonicBasis {
        protected HeGeom geom;

        public HamonicBasis(HeGeom g) { geom = g; }

        public DenseMatrix BuildClosedPrimalOneForm(List<HalfEdge> generator) {
            var n = geom.nEdges;
            var d = new double[n];
            foreach (var h in generator) 
                d[h.edge.eid] = h.edge.hid == h.id ? 1 : -1;

            return DenseMatrix.OfColumnMajor(n, 1, d);
        } 

        public List<double[]> Compute(HodgeDecomposition hd, List<List<HalfEdge>> generators) {
            var gammas = new List<double[]>();
            if (generators.Count > 0) {
                foreach (var g in generators) {
                    var omega  = BuildClosedPrimalOneForm(g);
                    var dAlpha = hd.ComputeExactComponent(omega);
                    gammas.Add(dAlpha);
                }
            }
            return gammas;
        }
    }
}
