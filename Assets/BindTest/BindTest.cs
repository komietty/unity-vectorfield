using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class BindTest : MonoBehaviour {
    [DllImport("libEigenDll")] private static extern void InverseMat_FullPivLU(int dim, float[] a, float[] ans);

    void Start() {
        float[,] bufMat = new float[3, 3] { { 100f, 2f, 1f }, { 2f, 1f, 0f }, { 1f, 1f, 2f } };
		float[,] AnsMat = new float[3, 3];
		AnsMat = InverseMatrix(bufMat);
        Debug.Log(AnsMat[0, 0]);
        Debug.Log(AnsMat[2, 2]);
    }

    void Matrix2Array(float[,] Mat, ref float[] Arr) {
		int count = 0;
		int dim = (int)Mathf.Sqrt(Mat.Length);
		for (int c = 0; c < dim; c++) {
			for (int r = 0; r < dim; r++) {
				Arr[count] = Mat[c, r];
				count++;
			}
		}
	}

	void Array2Matrix(float[] Arr, float[,] Mat) {
		int count = 0;
		int dim = (int)Mathf.Sqrt(Mat.Length);
        for (int c = 0; c < dim; c++) {
			for (int r = 0; r < dim; r++) {
				Mat[c, r] = Arr[count];
				count++;
			}
		}
	}

	public float[,] InverseMatrix(float[,] Mat) {
		float[] arr = new float[Mat.Length];
		int dim = (int)Mathf.Sqrt(Mat.Length);
		float[,] AnsMat = new float[dim, dim];

		Matrix2Array(Mat, ref arr);
		float[] ansArr = new float[Mat.Length];
		InverseMat_FullPivLU(dim, arr, ansArr);
		Array2Matrix(ansArr, AnsMat);

		return (float[,])AnsMat.Clone();
	}
}
