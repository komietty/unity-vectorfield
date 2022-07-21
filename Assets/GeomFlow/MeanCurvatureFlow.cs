using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Linq;

namespace ddg {
    public class MeanCurvatureFlow: IDisposable {
        [DllImport("libEigenDll")] static extern IntPtr GenMassMtx(float[] barycentricDualAreas, int len);
        //[DllImport("libEigenDll")] static extern IntPtr GenLplcMtx(Triplet[] triplets, int len);
        //[DllImport("libEigenDll")] static extern IntPtr GenFlowMtx(IntPtr massMtx, IntPtr lplcMtx);
        [DllImport("libEigenDll")] static extern void FreeMtx(IntPtr m);
        //[DllImport("libEigenDll")] static extern void Integrate(Vector3[] posIn, out Vector3[] posOut, int len);
        HalfEdgeGeom geom;
        IntPtr massMtx;
        IntPtr lplcMtx;
        IntPtr flowMtx;
        
        public MeanCurvatureFlow(HalfEdgeGeom geom) {
            this.geom = geom;
            InitMassMtx();
            //InitLplcMtx();
            //flowMtx = GenFlowMtx(massMtx, lplcMtx);
        }

        public void InitMassMtx(){
            var l = geom.mesh.verts.Length;
            var a = new float[l];
            for (var i = 0; i < l; i++) { a[i] = geom.BarycentricDualArea(geom.mesh.verts[i]); }
            massMtx = GenMassMtx(a, l);
        }

        /*
        public void InitLplcMtx(){
            var l = geom.mesh.verts.Length;
            var t = new Triplet[l];
            for (var i = 0; i < l; i++) {
                var v = geom.mesh.verts[i];
                foreach (var h in v.GetAdjacentHalfedges(geom.halfedges)) {

                }
            }
            lplcMtx = GenLplcMtx(t, l);
        }


        public void Integrate(){
            var l = geom.mesh.verts.Length;
            var posIn = geom.mesh.verts.Select(v => v.pos).ToArray();
            var posOut = new Vector3[l];
            Integrate(posIn, out posOut, l);
            var a = new float[l];
            for (var i = 0; i < l; i++) {
                var p = posOut[i];
            }
        }
        */

        public void Dispose(){
            FreeMtx(massMtx);
            //FreeMtx(lplcMtx);
            //FreeMtx(flowMtx);
        }
    }
}
