using System.Collections;
using UnityEngine;

namespace ddg {
    public class CurvatureFlow : CurvatureView {
        [SerializeField] protected bool native = true;
        [SerializeField, Range(0.001f, 0.1f)] protected float delta = 0.001f;
        [SerializeField] protected MeanCurvatureFlow.Type flowType;
        MeanCurvatureFlow flow;

        void Start() { Init(); StartCoroutine(Smooth()); }

        IEnumerator Smooth() {
            flow = new MeanCurvatureFlow(geom, flowType, native);
            SetCurvature(curvType);
            while(true){
                yield return new WaitForSeconds(1);
                flow.Integrate(delta);
                mesh.SetVertices(geom.Pos.ToArray());
                SetCurvature(curvType);
            }
        }
    }
}
