using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitSpot : BoardObject {

	// Getters
	private int GetSideToFaceFromPos (Board _boardRef, int _col,int _row) {
		if (_col == 0) { return 3; } // left!
		if (_col >= _boardRef.NumCols-1) { return 1; } // right!
		if (_row == 0) { return 0; } // up!
		if (_row >= _boardRef.NumRows-1) { return 2; } // down!
		return 0; // default to face up.
	}

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public ExitSpot (Board _boardRef, BoardPos _boardPos) {
		// Override what side I'm facing, based on what wall I'm against!!
		_boardPos.sideFacing = GetSideToFaceFromPos (_boardRef, _boardPos.col,_boardPos.row);
		base.InitializeAsBoardObject (_boardRef, _boardPos);
		// Tell my BoardSpace I'm on it!
		MySpace.SetMyExitSpot (this);
	}
	public ExitSpotData SerializeAsData() {
		ExitSpotData data = new ExitSpotData (BoardPos);
		return data;
	}


}
