#ifndef Basis_hpp

#define Basis_hpp
#define DllExport __declspec( dllexport )

#include <stdio.h>
#include <vector>
#include <memory>
#include "Eigen/Dense"
#include "Eigen/Sparse"

#pragma pack(push, 1)
struct Trp {
    double v;
    int i;
    int j;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct Vec {
    float x;
    float y;
    float z;
};
#pragma pack(pop)

extern "C" {
    DllExport void SolveLU(int ntrps, int nvrts, Trp* trps, Vec* vrts, Vec* outs);
}

#endif
