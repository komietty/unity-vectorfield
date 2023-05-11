using Unity.Mathematics;
using UnityEngine;
using System.Linq;

namespace VectorField {
    public class KillingVectorFieldViewer : MonoBehaviour {
        void Start() {
            var C = GetComponent<GeomContainer>();
            var G = C.geom;
            var O = KillingVectorField.Compute(G);
            var F = DEC.InterpolateWhitney(O, G).Select(f => f * 8.5f).ToArray();

            var max = 0f;
            var len = new float[G.nFaces];
            var col = new Color[G.nVerts];
            
            for (var i = 0; i < G.nFaces; i++) {
                var l = math.lengthsq(F[i]);
                max = math.max(l, max);
                len[i] = l;
            }
            
            foreach (var v in G.Verts) {
                var sum = 0f;
                var itr = 0;
                foreach (var f in G.GetAdjacentFaces(v)) {
                    sum += len[f.fid];
                    itr++;
                }
                var i = sum / (itr * max) * 0.9f + 0.05f;
                col[v.vid] = Color.HSVToRGB(i, 1, 1);
            }
            
            C.BuildFaceArrowBuffer(F, false);
            C.surfMode = GeomContainer.SurfMode.vertexColorBase;
            C.showFaceArrow = true;
            C.showVertArrow = false;
            C.vertexColors = col;
        }
    }
}
