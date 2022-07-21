using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ddg {
    public class Sparse {
        [Test]
        public void SparseSimplePasses() {
            /*
            var t = new Triplet[3];
            t[0] = new Triplet(1f, 0, 0);
            t[1] = new Triplet(2f, 1, 1);
            t[2] = new Triplet(3f, 2, 2);
            var m = new SparseMtx(t);
            */
            var a = new float[]{1f, 2f, 3f};
            var m = new SparseMtx(a);
            m.Dispose();
        }
    }
}