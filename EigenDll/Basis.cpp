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


DllExport S* GenSparseMtx(Trpl* triplets, int len){
    S* M = new S();
    std::vector<T> entries;
    for(auto i = 0; i < len; i++){
        auto a = triplets[i];
        entries.push_back(T(a.v, a.i, a.j));
    }
    (*M).setFromTriplets(entries.begin(), entries.end());
    return M;
}

DllExport S* GenDiagMtx(float* diagonals, int len){
    S* M = new S(len, len);
    std::vector<T> entries;
    for(auto i = 0; i < len; i++){
        auto d = diagonals[i];
        entries.push_back(T(d, i, i));
    }
    (*M).setFromTriplets(entries.begin(), entries.end());
    return M;
}

