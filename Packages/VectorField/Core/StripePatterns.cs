
namespace VectorField {
    using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
    
    public class StripePattern {
        private readonly HeGeom G;

        public StripePattern(HeGeom g) {
            G = g;
        }

        public Vector VertAngles() {
            var v = Vector.Build.Dense(G.nVerts);
            return v;
        }
    }
}
