//
//  Basis.cpp
//  EigenDll
//

#include "Basis.hpp"

DllExport void InverseMat_FullPivLU(int dim, float a[], float ans[]) {
    Eigen::MatrixXf mat = Eigen::MatrixXf::Zero(dim, dim);
    
    int count = 0;
    for (int c = 0; c < dim; c++) {
        for (int r = 0; r < dim; r++) {
            mat(c, r) = a[count];
            count++;
        }
    }
    
    Eigen::FullPivLU< Eigen::MatrixXf > lu(mat);
    Eigen::MatrixXf InvMat = mat.inverse();
    
    count = 0;
    for (int c = 0; c < dim; c++) {
        for (int r = 0; r < dim; r++) {
            ans[count] = InvMat(c, r);
            count++;
        }
    }
}

