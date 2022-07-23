using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ddg {
    public class CurvatureFlow {
        [Test]
        public void CurvatureFlowSimplePasses() {
            var m = TestAssets.Primitive();
            var g = new HalfEdgeGeom(m);
            var f = new MeanCurvatureFlow(g, MeanCurvatureFlow.Type.Simple);
            var h = 0.1;
            var result_1 = new Vector3[]{
                new Vector3(0.000000f, -0.445251f, -0.841730f),
                new Vector3(0.728959f, -0.445251f, 0.420865f),
                new Vector3(-0.728959f, -0.445251f, 0.420865f),
                new Vector3(0.000000f, 0.317048f, 0.000000f),
            };
            var result_2 = new Vector3[]{
                new Vector3(0.000000f, -0.387368f, -0.662757f),
                new Vector3(0.573964f, -0.387368f, 0.331378f),
                new Vector3(-0.573964f, -0.387368f, 0.331378f),
                new Vector3(0.000000f, 0.117351f, 0.000000f)
            };

            f.Integrate(h);

            for (var i = 0; i < g.mesh.verts.Length; i++) {
                var v = g.mesh.verts[i];
                var d = Vector3.Distance(v.pos, result_1[i]);
                Assert.IsTrue(d < 1e-3f);
            }

            f.Integrate(h);

            for (var i = 0; i < g.mesh.verts.Length; i++) {
                var v = g.mesh.verts[i];
                var d = Vector3.Distance(v.pos, result_2[i]);
                Assert.IsTrue(d < 1e-3f);
            }
            /*
            */
        }
    }
}
