# unity-vectorfield
 <img src="Documents/ribbon3.png"/>

Unity-vectorfield is vector field utility library, consisting of vector field generators on mesh and operators for them. It allows you to quick implementation of differential geometry algorithms on Unity, which instantly applicable to game programming, media art creation, or physical simulation. This library is referencing a lot from [Keenan Crane's Lectures at CMU](https://www.cs.cmu.edu/~kmcrane/Projects/DDG/) and [GeometryCentral](http://geometry-central.net/).

## Solvers

### Random vector field generator and is decomposition

<img src="Documents/hodgedecomp.png"/>

### Distance computing with heat method


<img src="Documents/scalarheat.png"/>

### Trivial connection generator  
Vector transport method as smooth as possible. From the thesis by Keenan et al 2010. 

<img src="Documents/trivialconn.png"/>

### Vector Heat Method

Another parallel transport method using heat method. From the thesis by Nicolas et al 2019.

 <img src="Documents/vectorheat.jpeg"/>

### And other misc!

<!--
This library contains features below: 
- Halfedge structure (as very core and standalone module)
- Curvature culclation (Gausian / Mean / Principal / Normal)
- Vector field generator by solving Poisson equation on mesh
- Hodge decomposition for a given tangent field 
- Basis finder for Hamonic component
- Basis finder for Homology group
- Trivial connection generator
- Ribbon drawer on a given vector field 
-->

## Installation & Usage
For installation, put the following address to UnitPackageManager.  
`https://github.com/komietty/unity-vectorfield.git?path=/Packages/VectorField`

To check the samples under Assets/Samples, just clone this repo and run.
