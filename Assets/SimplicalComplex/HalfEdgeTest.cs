using UnityEngine;

namespace ddg {
    public class HalfEdgeTest : MonoBehaviour {
        HalfEdgeMesh mesh;
        GameObject cube1;
        GameObject cube2;
        HalfEdge curr;
        int count = 0;

        void Start() {
            var m = GetComponentInChildren<MeshFilter>().sharedMesh;
            mesh = new HalfEdgeMesh(m);
            cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube1.transform.localScale *= 0.1f;
            cube2.transform.localScale *= 0.1f;
            curr = mesh.halfedges[count];
        }

        void Update() {
            var vs = mesh.verts;
            var p = Vector3.zero;
            if(Input.GetKeyDown(KeyCode.Space)){
                count = (count + 1) % mesh.halfedges.Length;
                curr = mesh.halfedges[count];
                p = (vs[curr.vid].pos + vs[curr.next.vid].pos) / 2;
            }
            if(Input.GetKeyDown(KeyCode.N)){
                curr = curr.next;
                p = (vs[curr.vid].pos + vs[curr.next.vid].pos) / 2;
            }
            if(Input.GetKeyDown(KeyCode.T)){
                curr = curr.twin;
                p = (vs[curr.vid].pos + vs[curr.next.vid].pos) / 2;
            }
            cube1.transform.position = p;
            cube2.transform.position = vs[curr.vid].pos;
        }
    }
}
