using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateView : BoardOccupantView {

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize (BoardView _myBoardView, Crate _myCrate) {
		base.InitializeAsBoardOccupantView (_myBoardView, _myCrate);
	}

}
