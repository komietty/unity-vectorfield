# unity-vectorfield
 <img src="Documents/vectorheat.jpeg"/>

Unity-vectorfield is a vector field library, consisting of vector field generators on mesh and operators for them. It allows you to quick implementation of vector field algorithms on Unity, which is instantly applicable to game programming, media art creation, or physical simulation. This library refers a lot to [Lectures at CMU](https://www.cs.cmu.edu/~kmcrane/Projects/DDG/) and [GeometryCentral](http://geometry-central.net/).

## Features
This library is for vector field design especially parallel transport, based on  **Halfedge structure** and **Discrete exterior calculus**. Beware some of the original algorithms referred to here support some geometric representations like point cloud or voxel grid, but this library only supports triangulated surface mesh. The main solvers are below.

- **Helmholtz-Hodge decomposition** - An implementation of hodge decomposition which you can learn in differential geometry textbooks. The discretization process is nicely explained in the [lecture notes](https://www.cs.cmu.edu/~kmcrane/Projects/DDG/).

- **Trivial connections** ([Paper](https://www.cs.cmu.edu/~kmcrane/Projects/TrivialConnections/) by Keenan et al., 2010) - A vector transport method as smooth as possible. Right now this implementation supports closed surfaces for any genesis. 

- **Scalar heat method** ([Paper](https://www.cs.cmu.edu/~kmcrane/Projects/HeatMethod/index.html) by Keenan et al., 2017) - A shortest distance computing method using the heat method for single or multiple-source on both flat and curved domains.

- **Vector Heat Method** ([Paper](https://www.cs.cmu.edu/~kmcrane/Projects/VectorHeatMethod/paper.pdf) by Nicolas et al., 2019) - Another parallel transport algorithm using the heat method. Right now this implementation supports closed surfaces for any genesis. You can find the C++ implementation by the original author [here](https://github.com/nmwsharp/geometry-central).

Other misc includes curvature calculation (Gaussian / Mean / Principal / Normal), vector field generator by solving Poisson equation on mesh, basis finder for harmonic component, basis finder for homology group, ribbon drawer, etc. 

## Installation & Usage
For installation, put the following address to UnitPackageManager.  
`https://github.com/komietty/unity-vectorfield.git?path=/Packages/VectorField`

To check the samples under Assets/Samples, just clone this repo and run.
