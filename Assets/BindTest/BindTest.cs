using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class BindTest : MonoBehaviour {
    [DllImport("libEigenDll")] static extern void SampleIntA(int a);
    [DllImport("libEigenDll")] static extern void SampleIntB(ref int a);
    [DllImport("libEigenDll")] static extern void SampleStructA(out StructA s);
    [DllImport("libEigenDll")] static extern StructA SampleStructB();
    [DllImport("libEigenDll")] static extern void SampleArrayA([In, Out] StructA[] arr, int length);
    [DllImport("libEigenDll")] static extern IntPtr CreateSparseMtx();
    [DllImport("libEigenDll")] static extern void DeleteSparseMtx(IntPtr mtx);
    [DllImport("libEigenDll")] static extern int Size(IntPtr mtx);
    [DllImport("libEigenDll")] static extern void InverseMat_FullPivLU(int dim, float[] a, float[] ans);

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct StructA{
		public int a;
		public float b;
        public StructA(int a, float b) {
            this.a = a;
            this.b = b;
        }
    };


    void Start() {
        float[,] bufMat = new float[3, 3] { { 100f, 2f, 1f }, { 2f, 1f, 0f }, { 1f, 1f, 2f } };
        float[,] AnsMat = new float[3, 3];
        AnsMat = InverseMatrix(bufMat);
        //Debug.Log(AnsMat[0, 0]);
        //Debug.Log(AnsMat[2, 2]);
        var a = 1;
		var b = 1;
        SampleIntA(a);
        SampleIntB(ref b);
        SampleStructA(out StructA s1);
        var s2 = SampleStructB();
        Debug.Log(a);
        Debug.Log(b);
        Debug.Log("s1.a: " + s1.a + ", s1.b: " + s1.b);
        Debug.Log("s2.a: " + s2.a + ", s2.b: " + s2.b);

		var arr = new StructA[5];
        for (var i = 0; i < arr.Length; i++) {
			arr[i] = new StructA(i * 100, i * 100);
		}
		SampleArrayA(arr, arr.Length);
        for (var i = 0; i < arr.Length; i++) {
            Debug.Log("arr.a: " + arr[i].a + ", arr.b: " + arr[i].b);
		}

		var s = CreateSparseMtx();
		Debug.Log(Size(s));
		DeleteSparseMtx(s);
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
