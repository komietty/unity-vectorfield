using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ddg {
    public class HamonicBasisViewer : TangentBundleBehaviour {
        [SerializeField] protected int baseNumber;

        protected override void Start() {
            base.Start();
            var hb = new HamonicBasis(bundle.Geom);
            var hd = new HodgeDecomposition(bundle.Geom);
            var hm = new Homology(bundle.Geom);
            var bases = hb.Compute(hd, hm.BuildGenerators());
            var w = bases[Mathf.Clamp(baseNumber, 0, bases.Count)];
            UpdateTng(w.ToArray());
        }
    }
}
