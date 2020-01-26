using System.Collections;
using UnityEngine;

public struct Line {
	public Vector2 start,end;


	public Line (Vector2 _start, Vector2 _end) {
		start = _start;
		end = _end;
	}
	public Line (float startX,float startY, float endX,float endY) {
		start = new Vector2 (startX, startY);
		end = new Vector2 (endX, endY);
	}


	/** In RADIANS. */
	public float GetAngleDeg () {
		return GameMathUtils.GetVector2AngleDegrees (start-end);
	}
	public Line Rotate (float radians) {
		return new Line(GameMathUtils.GetRotatedVector2Rad(start, radians), GameMathUtils.GetRotatedVector2Rad(end, radians));
	}
}
