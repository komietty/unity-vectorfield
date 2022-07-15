//
//  Basis.hpp
//  EigenDll
//

#ifndef Basis_hpp
#define Basis_hpp

#define DllExport  __declspec( dllexport )


#include <stdio.h>
#include <vector>
#include "Eigen/Dense"
#include "Eigen/Sparse"

typedef Eigen::Triplet<float> T;
typedef Eigen::SparseMatrix<float> S;

extern "C" {
    DllExport void InverseMat_FullPivLU(int dim, float a[], float ans[]);
    DllExport void CreateSparseFromTriplets(int rows[], int cols[], float vals[], float ans[]);
}

#endif
