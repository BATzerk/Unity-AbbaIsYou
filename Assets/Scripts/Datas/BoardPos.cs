public struct BoardPos {
	public int col;
	public int row;
	int _sideFacing; // 0 top, 1 right, 2 bottom, 3 left
	public int layer;

	public int sideFacing {
		get { return _sideFacing; }
		set {
			_sideFacing = value;
			if (_sideFacing<0) { _sideFacing += 4; }
			if (_sideFacing>=4) { _sideFacing -= 4; }
//			UnityEngine.Debug.Log (_sideFacing + "  " + value);
		}
	}

	public BoardPos (int col,int row, int SideFacing, int layer) {
		this.col = col;
		this.row = row;
		this._sideFacing = SideFacing;
		this.layer = layer;
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
