using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Complex32;
using UnityEngine;
using VectorField;

public class NewBehaviourScript : MonoBehaviour {
    private HeGeom geom;
    private GameObject go;
    private HalfEdge he;
    private List<Corner> adjCns;
    private List<HalfEdge> adjHEs;
    private int count;

    void Start() {
            var f = GetComponentInChildren<MeshFilter>();
            var mesh = HeComp.Weld(f.sharedMesh);
            geom = new HeGeom(mesh, transform);
            f.sharedMesh = mesh;
            
            var v0 = geom.Verts[5];
            var p0 = geom.Pos[v0.vid];
            var g0 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g0.transform.localScale *= 0.1f;
            g0.transform.position = p0;
             
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.localScale *= 0.1f;
            go.transform.position = p0;
            he = geom.halfedges[v0.hid];
            go.transform.position = p0 + geom.Vector(he) / 2;
            adjHEs = geom.GetAdjacentHalfedges(v0).ToList();
            adjCns = geom.GetAdjacentConers(v0).ToList();
            count = 0;
            var a = new double[]{ 2, -2, -4, 3, -2, 1, -5, 4, };
            var b = new double[]{ -2, 2, 0, 1, };
            var A = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix.OfRowMajor(4, 2, a);
            Debug.Log(A);
            var x = Solver.QR(A, b);
            Debug.Log(x);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            //he = adjHEs[count % adjHEs.Count];
            he = geom.halfedges[adjCns[count % adjCns.Count].hid];
            go.transform.position = geom.Pos[he.vid] + geom.Vector(he) / 2;
            count++;
        }
    }
}
