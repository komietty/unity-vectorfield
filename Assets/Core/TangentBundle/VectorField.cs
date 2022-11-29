using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ddg {
    public class VectorField: System.IDisposable {
        protected GraphicsBuffer lineBuffer;
        protected Material mat;

        public VectorField(){

        }

        public void Draw(){

        }

        public void Dispose() {
            lineBuffer.Dispose();
        }
    }
}
