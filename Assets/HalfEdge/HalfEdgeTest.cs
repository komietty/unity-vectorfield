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
            if(Input.GetKeyDown(KeyCode.Space)){
                count = (count + 1) % mesh.halfedges.Length;
                curr = mesh.halfedges[count];
                var p = (curr.vert.pos + curr.next.vert.pos) / 2;
                cube1.transform.position = p;
                cube2.transform.position = curr.vert.pos;
            }
            if(Input.GetKeyDown(KeyCode.N)){
                curr = curr.next;
                var p = (curr.vert.pos + curr.next.vert.pos) / 2;
                cube1.transform.position = p;
                cube2.transform.position = curr.vert.pos;
            }
            if(Input.GetKeyDown(KeyCode.T)){
                curr = curr.twin;
                var p = (curr.vert.pos + curr.next.vert.pos) / 2;
                cube1.transform.position = p;
                cube2.transform.position = curr.vert.pos;
            }
        }
    }
}
