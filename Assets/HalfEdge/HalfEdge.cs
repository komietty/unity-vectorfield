using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ddg {
    public struct HalfEdge {
        public int id;
        public Vert vert;
        public Edge edge;
        public Face face;
        public Coner coner;
        public int nextid;
        public int previd;
        public int twinid;
        public bool onBoundary;
    }

    public struct Vert {
        public int hid;
        public int vid;

        public Vert(int hid, int vid) {
            this.hid = hid;
            this.vid = vid;
        }

        public IEnumerable<Coner> GetAdjacentConers(HalfEdge[] halfedges){
            yield return new Coner(0);
        }
    }

    public struct Edge {
        public int hid;
        
        public Edge(int hid) {
            this.hid = hid;
        }
    }

    public struct Face {
        public int hid;

        public Face(int hid) {
            this.hid = hid;
        }
    }

    public struct Coner {
        public int hid;

        public Coner(int hid) {
            this.hid = hid;
        }
    }
}