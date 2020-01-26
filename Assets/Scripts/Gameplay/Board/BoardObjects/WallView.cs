using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallView : BoardObjectView {
	// Components
	private BeamRendererCollider beamRendererCollider;


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize (BoardView _myBoardView, Wall _wall) {
		base.InitializeAsBoardObjectView (_myBoardView, _wall);
		beamRendererCollider = new BeamRendererCollider (MyBoardView, this);
	}
//	public void Initialize (Wall _wall) {
//		// Make my beamRendererCollider now that I know what I look like!
//		beamRendererCollider = new BeamRendererCollider (_wall, BoardRef);
//	}
	
}
