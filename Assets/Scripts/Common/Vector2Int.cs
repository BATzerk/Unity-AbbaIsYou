using UnityEngine;

public struct Vector2Int {
	static public Vector2Int zero { get { return new Vector2Int(0,0); } }

	public int x;
	public int y;
	public Vector2Int (int _x,int _y) {
		x = _x;
		y = _y;
	}
	public Vector2 ToVector2 () { return new Vector2 (x,y); }

	public override bool Equals(object o) { return base.Equals (o); } // NOTE: Just added these to appease compiler warnings. I don't suggest their usage (because idk what they even do).
	public override int GetHashCode() { return base.GetHashCode(); } // NOTE: Just added these to appease compiler warnings. I don't suggest their usage (because idk what they even do).

	public static bool operator == (Vector2Int a, Vector2Int b) {
		return a.Equals(b);
	}
	public static bool operator != (Vector2Int a, Vector2Int b) {
		return !a.Equals(b);
	}
}