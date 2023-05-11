using UnityEngine;
using Unity.Mathematics;
using RVector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using CVector = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;
using static Unity.Mathematics.math;

namespace VectorField.Demo {
    public class SmoothestSectionViewer : MonoBehaviour {
        void Start() {
            var C = GetComponent<GeomContainer>();
            var G = C.geom;
            var connection = SmoothestSection.Compute(G);
            var X = ComputeVertVectorField(G, connection);
            C.BuildVertArrowBuffer(X);
            C.showVertArrow = true;
            C.showFaceArrow = false;
            C.surfMode = GeomContainer.SurfMode.blackBase;
        }
        
        float3[] ComputeVertVectorField(HeGeom g, CVector connection) {
            var X = new float3[g.nVerts];
            foreach (var v in g.Verts) {
                var (e1, e2) = g.OrthonormalBasis(v);
                var c = connection[v.vid];
                var l = sqrt(pow(c.Real, 2) + pow(c.Imaginary, 2));
                var s = c / l ;
                X[v.vid] = e1 * (float)s.Real + e2 * (float)s.Imaginary;
            }
            return X;
        }
    }
}
