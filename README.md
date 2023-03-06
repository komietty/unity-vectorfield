# unity-vectorfield
 <img src="Documents/vectorheat.jpeg"/>

Unity-vectorfield is vector field library, consisting of vector field generators on mesh and operators for them. It allows you to quick implementation of vector field algorithms on Unity, which instantly applicable to game programming, media art creation, or physical simulation. This library refers a lot from [Lectures at CMU](https://www.cs.cmu.edu/~kmcrane/Projects/DDG/) and [GeometryCentral](http://geometry-central.net/).

## Features
This library is for vector field design specially parallel transport, based on  **Halfedge structure** and **Discrete exterior calclus**. Beware some of the original algorithms refered here supports some geometric representations like point cloud or voxel grid, but this library only supports triangulated surface mesh. The main solvers are below.

- **Helmholtz-Hodge decomposition** - An implementaion of hodge decomposion which you can learn in differencial geometry text book. Discretization process is nicely explained in the [lectures notes](https://www.cs.cmu.edu/~kmcrane/Projects/DDG/).

- **Trivial connection** ([Paper]() by Keenan et al. (2010))- A vector transport method as smooth as possible. Right now this implimentation supports closed surface for any genesis. 

- **Scalar heat method** ([Paper](https://www.cs.cmu.edu/~kmcrane/Projects/HeatMethod/index.html) by Keenan et al. (2017)) - A Distance computing method using heat method for single or multiple-source shortest path problem on both flat and curved domains.

- **Vector Heat Method** ([Paper](https://www.cs.cmu.edu/~kmcrane/Projects/VectorHeatMethod/paper.pdf) by Nicolas et al. (2019)) - Another parallel transport algorithm using heat method. Right now this implimentation supports closed surface for any genesis. You can find C++ implementation by original author [here](https://github.com/nmwsharp/geometry-central).

Other misc includes Curvature culclation (Gausian / Mean / Principal / Normal), Vector field generator by solving Poisson equation on mesh, Basis finder for Hamonic component, Basis finder for Homology group, Ribbon drawer, etc. 


## Installation & Usage
For installation, put the following address to UnitPackageManager.  
`https://github.com/komietty/unity-vectorfield.git?path=/Packages/VectorField`

To check the samples under Assets/Samples, just clone this repo and run.
