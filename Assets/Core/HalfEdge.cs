namespace ddg {
    public class HalfEdge {
        public int id;
        public int vid;
        public Face face;
        public Corner corner;
        public HalfEdge next;
        public HalfEdge prev;
        public HalfEdge twin;
        public bool onBoundary;
        public HalfEdge(int id, int vid = -1) { this.id = id; this.vid = vid; }
    }

    public readonly struct Vert {
        public readonly int hid;
        public readonly int vid;
        public Vert(int hid, int vid) {
            this.hid = hid;
            this.vid = vid;
        }
    }

    public readonly struct Face {
        public readonly int hid;
        public Face(int hid) { this.hid = hid; }
    }

    public readonly struct Corner {
        public readonly int hid;
        public Corner(int hid) { this.hid = hid; }
    }
}