using UnityEngine;

namespace ddg {
    public class HamonicBasisViewer : TangentBundleBehaviour {
        [SerializeField] protected int baseNumber;

        protected override void Start() {
            base.Start();
            var g = bundle.geom;
            var hb = new HamonicBasis(g);
            var hd = new HodgeDecomposition(g);
            var hm = new Homology(g);
            var bases = hb.Compute(hd, hm.BuildGenerators());
            var w = bases[Mathf.Clamp(baseNumber, 0, bases.Count)];
            UpdateTng(w);
        }
    }
}
