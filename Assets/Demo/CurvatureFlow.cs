using System.Collections;
using UnityEngine;

namespace ddg {
    public class CurvatureFlow : DiscreteCurvature {
        [SerializeField] protected bool smooth = true;
        [SerializeField] protected bool native = true;
        [SerializeField, Range(0.001f, 0.1f)] protected float delta = 0.001f;
        [SerializeField] protected MeanCurvatureFlow.Type type;
        MeanCurvatureFlow flow;

        protected override void Start() {
            base.Start();
            if(smooth){
                flow = new MeanCurvatureFlow(geom, type, native);
                StartCoroutine(Smooth());
            }
        }

        IEnumerator Smooth() {
            yield return 0;
            while(true){
                yield return new WaitForSeconds(1);
                flow.Integrate(delta);
                mesh.SetVertices(geom.Pos.ToArray());
                SetCurvature(curvatureType);
            }
        }
    }
}
