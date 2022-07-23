using UnityEngine;

namespace ddg {
    public class TestAssets {
        public static Mesh Primitive() {
            var m = new Mesh();
            m.SetVertices(new Vector3[]{
                new Vector3(0, -0.5f, -1),
                new Vector3(0.866025f, -0.5f, 0.5f),
                new Vector3(-0.866025f, -0.5f, 0.5f),
                new Vector3(0, 0.5f, 0),
                });
            m.SetIndices(new int[] { 0, 3, 1, 0, 1, 2, 1, 3, 2, 2, 3, 0 }, MeshTopology.Triangles, 0);
            return m;
        }
    }
}
