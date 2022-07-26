using UnityEngine;

namespace ddg {
    public class CurvatureFlow : MonoBehaviour {
        [SerializeField] protected bool native = true;
        [SerializeField, Range(0.001f, 0.1f)] protected float delta = 0.001f;
        [SerializeField] protected MeanCurvatureFlow.Type type;
        HalfEdgeGeom geom;
        MeanCurvatureFlow flow;
        MeshFilter filt;
        Mesh mesh;

        void Start() {
            filt = GetComponentInChildren<MeshFilter>();
            var rend = GetComponentInChildren<MeshRenderer>();
            mesh = MeshUtils.Weld(filt.sharedMesh);
            filt.sharedMesh = mesh;
            geom = new HalfEdgeGeom(mesh);
            flow = new MeanCurvatureFlow(geom, type, native);
            mesh.RecalculateNormals();
        }

        void Update() {
            if(Input.GetKeyDown(KeyCode.Space)){
                var l = geom.nVerts;
                var a = new Vector3[l];
                flow.Integrate(delta);
                for (var i = 0; i < l; i++) { a[i] = geom.Pos[i]; }
                mesh.SetVertices(a);
                mesh.RecalculateNormals();
            }
        }
    }
}


