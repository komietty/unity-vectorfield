using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using Unity.Mathematics;

namespace VectorField {
    public static class Utility {
        
        public static Complex Div(Complex u, Complex v) {
            var deno = v.Real * v.Real + v.Imaginary * v.Imaginary;
            return new Complex(
                u.Real * v.Real + u.Imaginary * v.Imaginary,
                u.Imaginary * v.Real - u.Real * v.Imaginary
            ) / deno;
        }
            
        public static double2 Div(double2 u, double2 v) {
            var deno = v.x * v.x + v.y * v.y;
            return new double2(
                u.x * v.x + u.y * v.y,
                u.y * v.x - u.x * v.y
            ) / deno;
        }
    }
}
