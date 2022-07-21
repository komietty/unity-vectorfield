using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ddg {
    public class Dense {
        [Test]
        public void DenseSimplePasses() {
            var m = new DenseMtx(DenseMtx.Type.Zeros, 3, 3);
            var o = m.GetValues();
            foreach (var v in o) { Debug.Log(v); }
            m.Dispose();
        }
    }
}