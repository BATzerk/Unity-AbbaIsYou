using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardViewUtils {

//	/** Relative visuals are any visuals dependent on where Occupants are in relation to each other. e.g. rotatables' movable neighbors. */
//	public static void UpdateOccupantsRelativeVisuals (BoardView bv) {
//		foreach (BoardOccupantView bov in bv.AllOccupantViews) {
//			bov.UpdateRelativeVisuals ();
//		}
//	}


	public static bool GetAreAnyObjectsAnimating (BoardView bv) {
		foreach (BoardObjectView bov in bv.AllObjectViews) {
			if (bov.IsAnimating ()) { return true; }
		}
		return false;
	}

}
