using UnityEngine;
using System.Linq;
using System;
using System.Buffers;

namespace ddg {
    public class MeshUtils {

        /**
         * Weld vertices which are apparently same but structualy disconnected.
        */
        public static Mesh Weld(Mesh original, bool normalize = true) {
            var ogl_vrts = original.vertices;
            var ogl_idcs = original.triangles;
            var alt_mesh = new Mesh();
            //Normalize(ogl_vrts, true);
            var alt_vrts = ogl_vrts.Distinct().ToArray();
            var alt_idcs = new int[ogl_idcs.Length];
            var vrt_rplc = new int[ogl_vrts.Length];
            for (var i = 0; i < ogl_vrts.Length; i++) {
                var o = -1;
                for (var j = 0; j < alt_vrts.Length; j++) {
                    if (alt_vrts[j] == ogl_vrts[i]) { o = j; break; }
                }
                vrt_rplc[i] = o;
            }

            for (var i = 0; i < alt_idcs.Length; i++) {
                alt_idcs[i] = vrt_rplc[ogl_idcs[i]];
            }
            alt_mesh.SetVertices(alt_vrts);
            alt_mesh.SetTriangles(alt_idcs, 0);
            alt_mesh.RecalculateNormals();
            alt_mesh.RecalculateBounds();
            //return original;
            return alt_mesh;
        }

        /**
         * Normalize position of the vertices.
        */
        public static void Normalize(Vector3[] ps, bool rescale = true) {
            var n = ps.Length;
            var c = new Vector3();
            foreach (var p in ps) { c += p; }
            c /= n;

            var rad = -1f;
            for (int i = 0; i < n; i++) {
                ps[i] -= c;
                rad = Mathf.Max(rad, ps[i].magnitude);
            }

            if (rescale) {
                for (int i = 0; i < n; i++) { ps[i] /= rad; }
            }
        }
    }

    ref struct TempList<T> {
        int index;
        T[] array;
        public ReadOnlySpan<T> Span => new ReadOnlySpan<T>(array, 0, index);
        public TempList(int initCapacity) {
            this.array = ArrayPool<T>.Shared.Rent(initCapacity);
            this.index = 0;
        }

        public void Add(T value) {
            if (array.Length <= index) {
                var newArr = ArrayPool<T>.Shared.Rent(index * 2);
                Array.Copy(array, newArr, index);
                ArrayPool<T>.Shared.Return(array, true);
                array = newArr;
            }
            array[index++] = value;
        }

        public void Dispose() { ArrayPool<T>.Shared.Return(array, true); }
    }
}