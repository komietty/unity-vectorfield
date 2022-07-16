using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ddg {
    public class HalfEdge {
        public int id;
        public Vert vert;
        public Edge edge;
        public Face face;
        public Corner corner;
        public HalfEdge next;
        public HalfEdge prev;
        public HalfEdge twin;
        public bool onBoundary;

        public HalfEdge(int id){
            this.id = id;
        }
        public Vector3 Vector() { return next.vert.pos - vert.pos; }
    }

    public struct Vert {
        public int hid;
        public int vid;
        public Vector3 pos;

        public Vert(int hid, int vid, Vector3 pos) {
            this.hid = hid;
            this.vid = vid;
            this.pos = pos;
        }

        public IEnumerable<Corner> GetAdjacentConers(HalfEdge[] hes) {
            var tgt = hes[hid];
            while(tgt.onBoundary){ tgt = tgt.twin.next; }
            var curr = tgt;
            var endId = tgt.id;
            var once = false;
            while (true) {
                while (curr.onBoundary) { curr = curr.twin.next; }
                if (curr.id == tgt.id && once) break;
                yield return curr.next.corner;
                curr = curr.twin.next;
                once = true;
            };
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

    public struct Corner {
        public int hid;
        public Corner(int hid) {
            this.hid = hid;
        }
    }
}