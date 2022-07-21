#ifndef Basis_hpp

#define Basis_hpp
#define DllExport __declspec( dllexport )

#include <stdio.h>
#include <vector>
#include "Eigen/Dense"
#include "Eigen/Sparse"

typedef Eigen::Triplet<float> T;
typedef Eigen::SparseMatrix<float> S;
typedef Eigen::MatrixXd D;

#pragma pack(push, 1)
struct StructA {
    int a;
    float b;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct Trpl {
    float v;
    int i;
    int j;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct Vec3 {
    float x;
    float y;
    float z;
};
#pragma pack(pop)

extern "C" {
    DllExport void InverseMat_FullPivLU(int dim, float a[], float ans[]);
    DllExport void SampleIntA(int a)  { a = 2; };
    DllExport void SampleIntB(int* a) { *a = *a + 2; };
    DllExport void SampleStructA(StructA* s) { (*s).a = 1; };
    DllExport StructA SampleStructB() { auto s = StructA{ 1, 1.5 }; return s; };
    DllExport void SampleArrayA(StructA* s, int length) { for(int i =0; i < length; i++) { s[i] = StructA {s[i].a * 2, i * 1.5f}; } };
    DllExport S* CreateSparseMtx() { return new S(10, 10); };
    DllExport void Calc(S* mtx, float* arr, int len, float* ans) { };
   // laplaceMatrix
    DllExport void Integrate(Vec3* posIn, Vec3* posOut, int len);

    // Dense
    DllExport D* GenDenseOnes(int nrows, int ncols) {
        D* m = new D(nrows, ncols);
        (*m).setOnes();
        return m;
        
    }

    DllExport D* GenDenseZeros(int nrows, int ncols) {
        D* m = new D(nrows, ncols);
        (*m).setZero();
        return m;
    }

    DllExport void GetValues(D* m, float* values) {
        int r = (int)(*m).rows();
        int c = (int)(*m).cols();
        for (int i = 0; i < r; i++) {
            for (int j = 0; j < c; j++) {
                values[j + i * c] = (*m)(i,j);
            }
        }
    }

    DllExport void FreeAllocDense(D* m) { delete m; };

    // Sparse
    DllExport S* GenSparseMtx(Trpl* triplets, int len);

    DllExport S* GenDiagMtx(float* diagonals, int len);

    DllExport void FreeAllocSparse(S* m) { delete m; };

    DllExport int nRows(S* m) { return (int)(*m).rows(); };

    DllExport int nCols(S* m) { return (int)(*m).cols(); };


    DllExport S* Plus(S* m1, S* m2) {
        S* v = new S();
        *v = (*m1) + (*m2);
        return v;
        
    };

    DllExport S* Minus(S* m1, S* m2) {
        S* v = new S();
        *v = (*m1) - (*m2);
        return v;
    };

    DllExport S* Times(S* m1, S* m2) {
        S* v = new S();
        *v = (*m1) * (*m2);
        return v;
    };
}

#endif
