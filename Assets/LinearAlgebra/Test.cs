using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;

public class Test : MonoBehaviour {
    void Start() {
        var triplets = new List<(int, int, double)>();
        triplets.Add((0, 0, 1));
        triplets.Add((0, 1, 2));
        triplets.Add((0, 2, 3));
        triplets.Add((1, 1, 4));
        triplets.Add((2, 2, 5));
        var l = 10;
        //var m = new SparseMatrix(3, 3);
        var vec = DenseVector.Create(l, 1);
        var m = SparseMatrix.OfIndexed(l, l, triplets);
        var d1 = DenseMatrix.CreateDiagonal(l, l, 10);
        var d2 = DenseMatrix.OfIndexed(l, l, triplets);
        var d = (d1 + d2).Inverse();
        var v = d * vec;
        Debug.Log(m.ToString());
        Debug.Log(d.ToString());
        Debug.Log(v.ToString());
    }

}
