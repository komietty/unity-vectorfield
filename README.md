# unity-vectorfield
 <img src="Documents/vectorheat.jpeg"/>

Unity-vectorfield is vector field library, consisting of vector field generators on mesh and operators for them. It allows you to quick implementation of vector field algorithms on Unity, which instantly applicable to game programming, media art creation, or physical simulation. This library refers a lot from [Lectures at CMU](https://www.cs.cmu.edu/~kmcrane/Projects/DDG/) and [GeometryCentral](http://geometry-central.net/).

## Solvers

### Hodge decomposition

### Trivial connection  
Vector transport method as smooth as possible. From the thesis by Keenan et al 2010. 

### Scalar heat method

### Vector Heat Method

Another parallel transport method using heat method. From the thesis by Nicolas et al 2019.


### Misc
- Halfedge structure (as very core and standalone module)
- Curvature culclation (Gausian / Mean / Principal / Normal)
- Vector field generator by solving Poisson equation on mesh
- Basis finder for Hamonic component
- Basis finder for Homology group
- Ribbon drawer on a given vector field 


## Installation & Usage
For installation, put the following address to UnitPackageManager.  
`https://github.com/komietty/unity-vectorfield.git?path=/Packages/VectorField`

To check the samples under Assets/Samples, just clone this repo and run.
