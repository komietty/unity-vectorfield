using System.Collections.Generic;
using Unity.Mathematics;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace VectorField {
    using V = Vector<double>;

    public class Isoline {

        public static List<Vector3> Build(HeGeom geom, V phi, float maxPhi) {
            var lines = new List<Vector3>();
            var sgmts = new List<Vector3>();
            var interval = maxPhi / 30;

            foreach (var f in geom.Faces) {
                foreach (var h in geom.GetAdjacentHalfedges(f)) {
                    var i = h.vid;
                    var j = h.twin.vid;
                    var region1 = math.floor(phi[i] / interval);
                    var region2 = math.floor(phi[j] / interval);
                    if (region1 != region2) {
                        var t = region1 < region2 ?
                        (float)((region2 * interval - phi[i]) / (phi[j] - phi[i])):
                        (float)((region1 * interval - phi[i]) / (phi[j] - phi[i]));
                        sgmts.Add(geom.Pos[i] * (1 - t) + geom.Pos[j] * t + geom.FaceNormal(f).n * 0.01f);
                    }
                }
                if (sgmts.Count == 2) lines.AddRange(sgmts);
                sgmts.Clear();
            }
            return lines;
        }
    }
}
