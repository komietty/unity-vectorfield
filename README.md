# unity-halfedge

Unity halfedge handler library and some applications based on it. 

## Applications 
### Curvature Culclation

Each of meshes below shows Gausian / Mean / Principal / Normal curvatures respectively. It also contains GPU Gaussian curvature demo.

<img src="Imgs/curvature.gif"/>

### Smoothing Culclation
By solving the Poisson's equation on halfedges it enables mesh smoothing. (This lib contains C#/C++ linear algebra solver, but C# solver is quite slow so I strongly recommend using C++ solver. If the C++ solver(dylib) could not be found, go `[ProjectRoot]/Cpp` folder and include Eigen and build on your machine.)

<img src="Imgs/smoothing.gif"/>

## References
[Discrete Differential Geometry: An Applied Introduction - Keenan Crane](https://www.cs.cmu.edu/~kmcrane/Projects/DDG/)
