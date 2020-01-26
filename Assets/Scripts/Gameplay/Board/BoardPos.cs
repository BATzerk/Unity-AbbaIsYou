public struct BoardPos {
    public Vector2Int ColRow;
    private int _sideFacing; // corresponds to Sides.cs.
    public int SideFacing {
        get { return _sideFacing; }
        set {
            _sideFacing = value;
            if (_sideFacing<Sides.Min) { _sideFacing += Sides.Max; }
            if (_sideFacing>=Sides.Max) { _sideFacing -= Sides.Max; }
        }
    }
    
    public BoardPos(int col,int row) {
        this.ColRow = new Vector2Int(col,row);
        _sideFacing = 0;
    }
    public BoardPos(Vector2Int colRow, int sideFacing) {
        this.ColRow = colRow;
        this._sideFacing = sideFacing;
    }
    public BoardPos(int col,int row, int sideFacing) {
        this.ColRow = new Vector2Int(col,row);
        this._sideFacing = sideFacing;
    }
    
    public override bool Equals(object o) { return base.Equals (o); } // NOTE: Just added these to appease compiler warnings. I don't suggest their usage (because idk what they even do).
    public override int GetHashCode() { return base.GetHashCode(); } // NOTE: Just added these to appease compiler warnings. I don't suggest their usage (because idk what they even do).

    public static bool operator == (BoardPos b1, BoardPos b2) {
        return b1.Equals(b2);
    }
    public static bool operator != (BoardPos b1, BoardPos b2) {
        return !b1.Equals(b2);
    }
    
    
}
