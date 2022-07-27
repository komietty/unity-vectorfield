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
            var pos = mesh.Pos;
            var p = Vector3.zero;
            if(Input.GetKeyDown(KeyCode.Space)){
                count = (count + 1) % mesh.halfedges.Length;
                curr = mesh.halfedges[count];
                p = (pos[curr.vid] + pos[curr.next.vid]) / 2;
                cube1.transform.position = p;
            }
            if(Input.GetKeyDown(KeyCode.N)){
                curr = curr.next;
                p = (pos[curr.vid] + pos[curr.next.vid]) / 2;
                cube1.transform.position = p;
            }
            if(Input.GetKeyDown(KeyCode.T)){
                curr = curr.twin;
                p = (pos[curr.vid] + pos[curr.next.vid]) / 2;
                cube1.transform.position = p;
            }
            cube2.transform.position = pos[curr.vid];
        }
    }
}
