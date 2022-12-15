using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ddg {
    public class HamonicBasisViewer : TangentBundleBehaviour {
        protected override void Start() {
            base.Start();
            var hb = new HamonicBasis(bundle.Geom);
            var hd = new HodgeDecomposition(bundle.Geom);
            var hm = new Homology(bundle.Geom);
            var w  = hb.Compute(hd, hm.BuildGenerators());
            UpdateTng(w[0]);
        }
    }
}
