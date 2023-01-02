using UnityEngine;

namespace VFD {
    public class HamonicBasisViewer : TangentBundle {
        [SerializeField] protected int baseNumber;

        protected override void Start() {
            base.Start();
            var hb = new HamonicBasis(geom);
            var hd = new HodgeDecomposition(geom);
            var hm = new HomologyGenerator(geom);
            var bs = hb.Compute(hd, hm.BuildGenerators());
            UpdateTng(bs[Mathf.Clamp(baseNumber, 0, bs.Count)]);
        }
    }
}
