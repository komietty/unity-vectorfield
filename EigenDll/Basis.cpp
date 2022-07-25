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

DllExport void SolveLU(int ntrps, int nvrts, Trp* trps, Vec* vrts, Vec* outs){
    std::vector<Eigen::Triplet<double>> triplets;
    for(int i = 0; i < ntrps; i++){
        triplets.push_back(Eigen::Triplet<double>(trps[i].i, trps[i].j, trps[i].v));
    }
    Eigen::SparseMatrix<double> X(nvrts, nvrts);
    X.setFromTriplets(triplets.begin(), triplets.end());
    Eigen::MatrixXd inputs(nvrts, 3);
    inputs.setZero();
    for(int i = 0; i < nvrts; i++){
        inputs(i, 0) = vrts[i].x;
        inputs(i, 1) = vrts[i].y;
        inputs(i, 2) = vrts[i].z;
    }
    Eigen::SparseLU<Eigen::SparseMatrix<double>> solver;
    solver.compute(X);
    Eigen::MatrixXd outputs = solver.solve(inputs);
    for(int i = 0; i < nvrts; i++){
        Vec v;
        v.x = outputs(i, 0);
        v.y = outputs(i, 1);
        v.z = outputs(i, 2);
        outs[i] = v;
    }
}
