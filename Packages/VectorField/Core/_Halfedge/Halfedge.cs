namespace VectorField {
    public class HalfEdge {
        public int id;
        public int vid;
        public Edge edge;
        public Face face;
        public Corner corner;
        public HalfEdge next;
        public HalfEdge prev;
        public HalfEdge twin;
        public bool onBoundary;
        public bool IsCanonical() => id == edge.hid;

        public HalfEdge(int id, int vid = -1) {
            this.id = id;
            this.vid = vid;
        }
    }

    public readonly struct Vert {
        public readonly int hid;
        public readonly int vid;
        public Vert(int hid, int vid) {
            this.hid = hid;
            this.vid = vid;
        }
    }

    public readonly struct Edge {
        public readonly int hid;
        public readonly int eid;
        public Edge(int hid, int eid) {
            this.hid = hid;
            this.eid = eid;
        }
    }

    public readonly struct Face {
        public readonly int hid;
        public readonly int fid;
        public Face(int hid, int fid) {
            this.hid = hid;
            this.fid = fid; // if -1, it is boundary face
        }
    }

    /*
     * Another representation of halfedge except boundary one.
     * Each corner stores the halfedge opposite to it.
     */
    public readonly struct Corner {
        public readonly int hid;
        public Corner(int hid) { this.hid = hid; }
    }
}