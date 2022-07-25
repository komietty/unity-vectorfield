using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ddg {
    public class HalfEdge {
        public int id;
        public int vid;
        public Vert vert;
        public Face face;
        public Corner corner;
        public HalfEdge next;
        public HalfEdge prev;
        public HalfEdge twin;
        public bool onBoundary;
        public HalfEdge(int id) { this.id = id; }
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

        public IEnumerable<Face> GetAdjacentFaces(HalfEdge[] hes) {
            var tgt = hes[hid];
            while(tgt.onBoundary){ tgt = tgt.twin.next; }
            var curr = tgt;
            var endId = tgt.id;
            var once = false;
            while (true) {
                while (curr.onBoundary) { curr = curr.twin.next; }
                if (once && curr.id == tgt.id) break;
                once = true;
                yield return curr.next.face;
                curr = curr.twin.next;
            };
        }

        public IEnumerable<Corner> GetAdjacentConers(HalfEdge[] hes) {
            var tgt = hes[hid];
            while(tgt.onBoundary){ tgt = tgt.twin.next; }
            var curr = tgt;
            var endId = tgt.id;
            var once = false;
            while (true) {
                while (curr.onBoundary) { curr = curr.twin.next; }
                if (once && curr.id == tgt.id) break;
                once = true;
                yield return curr.next.corner;
                curr = curr.twin.next;
            };
        }

        public IEnumerable<HalfEdge> GetAdjacentHalfedges(HalfEdge[] hes) {
            var tgt = hes[hid];
            var curr = tgt;
            var endId = tgt.id;
            var once = false;
            while (true) {
                if (once && curr.id == endId) break;
                yield return curr;
                curr = curr.twin.next;
                once = true;
            };
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