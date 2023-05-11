# unity-vectorfield
 <img src="Documents/vectorheat.jpeg"/>

Unity-vectorfield is a vector field library, consisting of vector field generators on mesh and operators for them. It allows you to quick implementation of vector field algorithms on Unity, which is instantly applicable to game programming, media art creation, or physical simulation.
<!--* This library refers a lot to [Lectures at CMU](https://www.cs.cmu.edu/~kmcrane/Projects/DDG/) and [GeometryCentral](http://geometry-central.net/). -->

## Features
This library is for designing various vector fields based on  **Halfedge structure** and **Discrete exterior calculus**. Beware some of the original algorithms referred to here support some geometric representations like point cloud or voxel grid, but this library only supports triangulated closed surface mesh with any genesis right now (meaning closed meshes with holes like torus. surface with boundaries will be supported soon). The main solvers are below.

- **Hodge decomposition**[^1] - An implementation of Helmholtz-Hodge decomposition. An arbitrally tangent field is decomposed into exact, coexact, and harmonic space. The discretization process is nicely explained in the lecture note[^7].

- **Trivial connections**[^2] - A vector transport method to compute a smoothest vector field with user input singularity points. Singularities can be placed anywhere as long as they satisfy Gauss-bonnet theorem.

- **Smooth vector field**[^4] -A vector parallel transport method. Unlike the trivial connection, this method put globally optimal singularities automatically. Currently it only implements smoothest field, not the curvature-aligned fields yet. 

- **Scalar heat method**[^5] - A shortest distance computing method using the heat method for single or multiple-source on both flat and curved domains. 

- **Vector heat method**[^6] - Another parallel transport algorithm using the heat method. Note that this library only implements fundamental feature of the paper. The full featured C++ implementation by the original author is [here](https://github.com/nmwsharp/geometry-central).

- **Killing vector filed**[^3] - this method computes an approximated version of [Killig vector filed](https://en.wikipedia.org/wiki/Killing_vector_field). This vector field becomes very important when you want to compute an isometric pattern on a surface.  

Miscellaneous includes curvature calculation, vector field generator by solving Poisson equation on mesh, basis finder for harmonic component, basis finder for homology group, ribbon drawer, etc. 


## Installation 
For installation, put the following address to UnitPackageManager.  
`https://github.com/komietty/unity-vectorfield.git?path=/Packages/VectorField`

To check the samples under Assets/Samples, just clone this repo and run (tested on mac intel, apple silicon, and windows). Belows are images you will see in the samples.

<div display="flex">
 <img src="Documents/p4.png" width="412"/>
 <img src="Documents/p2.png" width="412"/>
 <img src="Documents/p5.png" width="412"/>
 <img src="Documents/p1.png" width="412"/>
 <img src="Documents/p3.png" width="412"/>
 <img src="Documents/p6.png" width="412"/>
 </div>

 ## References
 - [^1]: Design of Tangent Vector Fields, Fisher et al., 2007  
 - [^2]: Trivial Connections on Discrete Surfaces, Keenan et al., 2010  
 - [^3]: On Discrete Killing Vector Fields and Patterns on Surfaces, Ben-Chen et al., 2010   
 - [^4]: Globally Optimal Direction Fields, Keenan et al., 2013  
 - [^5]: The Heat Method for Distance Computation, Keenan et al., 2017  
 - [^6]: The Vector Heat Method, Nicholas et al., 2019   
 - [^7]: [Discrete Differential Geometry: An Applied Introduction](https://www.cs.cmu.edu/~kmcrane/Projects/DDG/)  