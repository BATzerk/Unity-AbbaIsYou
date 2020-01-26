using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardView : MonoBehaviour {
	// Visual properties
	static int MIN_SCREEN_BORDER = 100; // how far any side of the board is allowed to be from the edge of the screen
	private Vector2 pos; // the top-left corner of the board.
	private float unitSize; // how big each board space is in pixels
	private float baseObjectScaleConstant; // every BoardObjectBody uses this constant when applying its scale. To keep things perfectly flush with BoardSpaces.
	// Variable properties
	private bool areObjectsAnimating;
	private float objectsAnimationLoc; // eases to 0 or 1 while we're animating!
	private float objectsAnimationLocTarget; // either 0 (undoing a halfway animation) or 1 (going to the new, updated position).
//	bool areAnyOccupantsAnimating; // set to true when any occupant starts animating; set to false when we know none are anymore.
//	bool doCheckIfOccupantsFinishedAnimating; // a flag I set to true whenever a BoardOccupant finishes moving! If it's true, I'll check if they're all done moving at the end of the frame.
	private Vector2Int simulatedMoveDir;
	// Components
	[SerializeField] private Transform tf_beamLines;
	[SerializeField] private Transform tf_boardSpaces;
	private BeamRendererColliderArena beamRendererColliderArena;
	// Objects
	private BoardSpaceView[,] spaceViews;
	private List<BoardObjectView> allObjectViews; // includes EVERY single BoardObjectView!
	// References
	private Board myBoard; // this reference does NOT change during our existence! (If we undo a move, I'm destroyed completely and a new BoardView is made along with a new Board.)
	private Board simulatedMoveBoard; // for TOUCH INPUT feedback. Same story as the pre-move dragging in Threes!.
	private Level levelRef;

	// Getters
	public Board MyBoard { get { return myBoard; } }
	public List<BoardObjectView> AllObjectViews { get { return allObjectViews; } }
	public bool AreObjectsAnimating { get { return areObjectsAnimating; } }
	public bool AreGoalsSatisfied { get { return myBoard.AreGoalsSatisfied; } }
	public float UnitSize { get { return unitSize; } }
	public float BaseObjectScaleConstant { get { return baseObjectScaleConstant; } }
	public float ObjectsAnimationLocTarget { get { return objectsAnimationLocTarget; } }
	public BeamRendererColliderArena BeamRendererColliderArena { get { return beamRendererColliderArena; } }

	private ResourcesHandler resourcesHandler { get { return ResourcesHandler.Instance; } }
	public Transform tf_BeamLines { get { return tf_beamLines; } }
	public Transform tf_BoardSpaces { get { return tf_boardSpaces; } }

	public float BoardToX(float col) { return pos.x + col*unitSize; }
	public float BoardToY(float row) { return pos.y - row*unitSize; }
//	public int xToGrid(float x) { return (int)((x-leftGap)/unitSize+0.5); }
//	public int yToGrid(float y) { return (int)((y-topGap)/unitSize+0.5); }
	public int WorldIndex { get { return levelRef.WorldIndex; } }

	/** Very inefficient. Just temporary. */
	public BoardOccupantView TEMP_GetOccupantView (BoardOccupant _occupant) {
		foreach (BoardObjectView objectView in allObjectViews) {
			if (objectView is BoardOccupantView) {
				if (objectView.MyBoardObject == _occupant) {
					return objectView as BoardOccupantView;
				}
//				BoardOccupantView occupantView = objectView as BoardOccupantView;
			}
		}
		return null; // oops.
	}
	private Rect TEMP_availableArea; // QQQ
	private void OnDrawGizmos () {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube (TEMP_availableArea.center*GameVisualProperties.WORLD_SCALE, TEMP_availableArea.size*GameVisualProperties.WORLD_SCALE);
	}

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize (Level _levelRef, Board _myBoard) {
		levelRef = _levelRef;
		myBoard = _myBoard;
		this.transform.SetParent (levelRef.transform);
		this.transform.localScale = Vector3.one;
		this.transform.localPosition = Vector3.zero;

//		areAnyOccupantsAnimating = false;
//		doCheckIfOccupantsFinishedAnimating = false;
		areObjectsAnimating = false;
		objectsAnimationLoc = 0;
		beamRendererColliderArena = new BeamRendererColliderArena ();

		int numCols = myBoard.NumCols;
		int numRows = myBoard.NumRows;

		// Determine unitSize and other board-specific visual stuff
//		float minLeftGap = 0;//120;
		Vector2 screenSize = ScreenHandler.RelativeScreenSize;
		float minGapBottom = 160 / ScreenHandler.ScreenScale;
		float minGapTop = 80 / ScreenHandler.ScreenScale;
		float minGapLeft = 40 / ScreenHandler.ScreenScale;
		float minGapRight = 40 / ScreenHandler.ScreenScale;
		Rect r_availableArea = new Rect(minGapLeft,minGapBottom, screenSize.x-minGapLeft-minGapRight,screenSize.y-minGapBottom-minGapTop);
		TEMP_availableArea = r_availableArea;
//		unitSize = Mathf.Min((boardContainerSize.x-minLeftGap-MIN_SCREEN_BORDER*2)/(float)(numCols), (boardContainerSize.y-MIN_SCREEN_BORDER*2)/(float)(numRows));
		unitSize = Mathf.Min(r_availableArea.size.x/(float)(numCols), r_availableArea.size.y/(float)(numRows));
		baseObjectScaleConstant = UnitSize * 0.78f; // HARDCODED. We scale everything by a constant so they all fit perfectly in the Board.
		float boardWidth = unitSize * numCols;
		float boardHeight = unitSize * numRows;
		// Position us real good!
		pos = new Vector2(r_availableArea.position.x,r_availableArea.yMax) + new Vector2(unitSize*0.5f,-unitSize*0.5f);
		pos += new Vector2(r_availableArea.size.x-boardWidth, boardHeight-r_availableArea.size.y)*0.5f; // center us, dawg!
//		pos = new Vector2 ((ScreenHandler.RelativeScreenSize.x-boardWidth+unitSize) * 0.5f, // width-boardWidth-MIN_SCREEN_BORDER;
//							ScreenHandler.RelativeScreenSize.y - (ScreenHandler.RelativeScreenSize.y-boardHeight+unitSize)*0.5f);

		// Make spaces!
		spaceViews = new BoardSpaceView[numCols,numRows];
		for (int i=0; i<numCols; i++) {
			for (int j=0; j<numRows; j++) {
				spaceViews[i,j] = Instantiate(resourcesHandler.prefabGO_boardSpaceView).GetComponent<BoardSpaceView>();
				spaceViews[i,j].Initialize (this, myBoard.GetSpace(i,j));
			}
		}
		// Clear out all my lists!
		allObjectViews = new List<BoardObjectView> ();

		foreach (BeamGoal bo in myBoard.beamGoals) { AddBeamGoalView (bo); }
		foreach (BeamSource bo in myBoard.beamSources) { AddBeamSourceView (bo); }
		foreach (Bucket bo in myBoard.buckets) { AddBucketView (bo); }
		foreach (BucketGoal bo in myBoard.bucketGoals) { AddBucketGoalView (bo); }
		foreach (Crate bo in myBoard.crates) { AddCrateView (bo); }
		foreach (CrateGoal bo in myBoard.crateGoals) { AddCrateGoalView (bo); }
		foreach (ExitSpot bo in myBoard.exitSpots) { AddExitSpotView (bo); }
		foreach (FloorBeamGoal bo in myBoard.floorBeamGoals) { AddFloorBeamGoalView (bo); }
		foreach (Mirror bo in myBoard.mirrors) { AddMirrorView (bo); }
		foreach (Obstacle bo in myBoard.obstacles) { AddObstacleView (bo); }
		foreach (Player bo in myBoard.players) { AddPlayerView (bo); }
		foreach (Portal bo in myBoard.portals) { AddPortalView (bo); }
		foreach (Pusher bo in myBoard.pushers) { AddPusherView (bo); }
		foreach (Wall bo in myBoard.walls) { AddWallView (bo); }

		// Start off with all the right visual bells and whistles!
		UpdateAllViewsMoveEnd ();

		// Add event listeners!
//		GameManagers.Instance.EventManager.BoardMoveCompleteEvent += OnBoardMoveComplete;
//		GameManagers.Instance.EventManager.BoardFinishedMoveStepEvent += OnBoardFinishedMoveStep;
	}
	public void DestroySelf () {
		// Remove event listeners!
//		GameManagers.Instance.EventManager.BoardMoveCompleteEvent -= OnBoardMoveComplete;
//		GameManagers.Instance.EventManager.BoardFinishedMoveStepEvent -= OnBoardFinishedMoveStep;

		// Destroy my entire GO.
		GameObject.Destroy (this.gameObject);
	}

	BeamGoalView AddBeamGoalView (BeamGoal data) {
		BeamGoalView newObj = Instantiate(resourcesHandler.prefabGO_beamGoalView).GetComponent<BeamGoalView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	BeamSourceView AddBeamSourceView (BeamSource data) {
		BeamSourceView newObj = Instantiate(resourcesHandler.prefabGO_beamSourceView).GetComponent<BeamSourceView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	BucketView AddBucketView (Bucket data) {
		BucketView newObj = Instantiate(resourcesHandler.prefabGO_bucketView).GetComponent<BucketView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	BucketGoalView AddBucketGoalView (BucketGoal data) {
		BucketGoalView newObj = Instantiate(resourcesHandler.prefabGO_bucketGoalView).GetComponent<BucketGoalView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	CrateView AddCrateView (Crate data) {
		CrateView newObj = Instantiate(resourcesHandler.prefabGO_crateView).GetComponent<CrateView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	CrateGoalView AddCrateGoalView (CrateGoal data) {
		CrateGoalView newObj = Instantiate(resourcesHandler.prefabGO_crateGoalView).GetComponent<CrateGoalView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	ExitSpotView AddExitSpotView (ExitSpot data) {
		ExitSpotView newObj = Instantiate(resourcesHandler.prefabGO_exitSpotView).GetComponent<ExitSpotView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	FloorBeamGoalView AddFloorBeamGoalView (FloorBeamGoal data) {
		FloorBeamGoalView newObj = Instantiate(resourcesHandler.prefabGO_floorBeamGoalView).GetComponent<FloorBeamGoalView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	ObstacleView AddObstacleView (Obstacle data) {
		ObstacleView newObj = Instantiate(resourcesHandler.prefabGO_obstacleView).GetComponent<ObstacleView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	MirrorView AddMirrorView (Mirror data) {
		MirrorView newObj = Instantiate(resourcesHandler.prefabGO_mirrorView).GetComponent<MirrorView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	PlayerView AddPlayerView (Player data) {
		PlayerView newObj = Instantiate(resourcesHandler.prefabGO_playerView).GetComponent<PlayerView> ();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	PortalView AddPortalView (Portal data) {
		PortalView newObj = Instantiate(resourcesHandler.prefabGO_portalView).GetComponent<PortalView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	PusherView AddPusherView (Pusher data) {
		PusherView newObj = Instantiate(resourcesHandler.prefabGO_pusherView).GetComponent<PusherView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	WallView AddWallView (Wall data) {
		WallView newObj = Instantiate(resourcesHandler.prefabGO_wallView).GetComponent<WallView>();
		newObj.Initialize (this, data);
		allObjectViews.Add (newObj);
		return newObj;
	}
	/*
		private List<BeamGoalView> beamGoalViews;
		private List<BeamSourceView> beamSourceViews;
		private List<BucketView> bucketViews;
		private List<BucketGoalView> bucketGoalViews;
		private List<CrateView> crateViews;
		private List<CrateGoalView> crateGoalViews;
		private List<ExitSpotView> exitSpotViews;
		private List<MirrorView> mirrorViews;
		private List<ObstacleView> obstacleViews;
		private List<PlayerView> playerViews;
		private List<PortalView> portalViews;
		private List<PusherView> pusherViews;
		private List<WallView> wallViews;
//		goalObjectViews = new List<IGoalObject>();
		beamGoalViews = new List<BeamGoalView>();
		beamSourceViews = new List<BeamSourceView>();
		bucketViews = new List<BucketView>();
		bucketGoalViews = new List<BucketGoalView>();
		crateViews = new List<CrateView>();
		crateGoalViews = new List<CrateGoalView>();
		exitSpotViews = new List<ExitSpotView>();
		mirrorViews = new List<MirrorView>();
		obstacleViews = new List<ObstacleView>();
		playerViews = new List<PlayerView>();
		portalViews = new List<PortalView>();
		pusherViews = new List<PusherView>();
		wallViews = new List<WallView>();
		*/

	private void AddObjectView (BoardObject sourceObject) {
		if (sourceObject is BeamGoal) { AddBeamGoalView (sourceObject as BeamGoal); }
		else if (sourceObject is BeamSource) { AddBeamSourceView (sourceObject as BeamSource); }
		else if (sourceObject is Bucket) { AddBucketView (sourceObject as Bucket); }
		else if (sourceObject is BucketGoal) { AddBucketGoalView (sourceObject as BucketGoal); }
		else if (sourceObject is Crate) { AddCrateView (sourceObject as Crate); }
		else if (sourceObject is CrateGoal) { AddCrateGoalView (sourceObject as CrateGoal); }
		else if (sourceObject is ExitSpot) { AddExitSpotView (sourceObject as ExitSpot); }
		else if (sourceObject is FloorBeamGoal) { AddFloorBeamGoalView (sourceObject as FloorBeamGoal); }
		else if (sourceObject is Obstacle) { AddObjectView (sourceObject as Obstacle); }
		else if (sourceObject is Mirror) { AddMirrorView (sourceObject as Mirror); }
		else if (sourceObject is Player) { AddPlayerView (sourceObject as Player); }
		else if (sourceObject is Portal) { AddPortalView (sourceObject as Portal); }
		else if (sourceObject is Pusher) { AddPusherView (sourceObject as Pusher); }
		else if (sourceObject is Wall) { AddWallView (sourceObject as Wall); }
		else { Debug.LogError ("Trying to add BoardObjectView from BoardObject, but no clause to handle this type! " + sourceObject.GetType().ToString()); }
	}
	public void OnObjectViewDestroyedSelf (BoardObjectView bo) {
		// Remove it from the list of views!
		allObjectViews.Remove (bo);
	}



	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
//	public void OnObjectStartAnimating () {
//		areAnyOccupantsAnimating = true;
//	}
//	public void OnObjectFinishedMovingAnimation () {
//		doCheckIfOccupantsFinishedAnimating = true;
//	}
//	private void CheckIfOccupantsFinishedAnimating () {
//		doCheckIfOccupantsFinishedAnimating = false; // set it back to false, of course.
//		areAnyOccupantsAnimating = BoardViewUtils.GetAreAnyObjectsAnimating (this);
//		// If they're ALL done animating, then call the done-animating function!
//		if (!areAnyOccupantsAnimating) {
//			UpdateAllViewsMoveEnd ();
//		}
//	}
//	private void OnOccupantsFinishedAnimating () {
//	}
//	private void OnBoardFinishedMoveStep (Board _board) {
//		if (_board != myBoard) { throw new UnityException ("Whoa! Somehow our BoardView was listening for a call from its Board, but a DIFFERENT Board dispatched the call!"); }
//		// Now, if we're STILL not animating, then this move is COMPLETE, Sal!!
//		if (!BoardViewUtils.GetAreAnyOccupantsAnimating (this)) {
////			myBoard.OnMoveComplete ();
//		}
//	}
//	private void OnBoardMoveComplete (Board _board) {
//		if (_board != myBoard) { throw new UnityException ("Whoa! Somehow our BoardView was listening for a call from its Board, but a DIFFERENT Board dispatched the call!"); }
//	}

	public void UpdateAllViewsMoveStart () {
		AddViewsForAddedObjects ();
		UpdateBoardObjectViewVisualsMoveStart ();
		// Note that destroyed Objects' views will be removed by the view in the UpdateVisualsMoveEnd.
		// Reset our BoardObjectViews' "from" values to where they *currently* are! Animate from there.
		foreach (BoardObjectView bov in allObjectViews) {
			bov.SetValues_From_ByCurrentValues ();
		}
		areObjectsAnimating = true;
		objectsAnimationLoc = 0;
		objectsAnimationLocTarget = 1;
//		// Blindly confirm that at least someone is moving!
//		areAnyOccupantsAnimating = true;
//		doCheckIfOccupantsFinishedAnimating = true;
	}
	private void UpdateAllViewsMoveEnd () {
		areObjectsAnimating = false;
		objectsAnimationLoc = 0; // reset this back to 0, no matter what the target value is.
		for (int i=allObjectViews.Count-1; i>=0; --i) { // Go through backwards, as objects can be removed from the list as we go!
			allObjectViews[i].UpdateVisualsPostMove ();
		}
	}
	private void UpdateBoardObjectViewVisualsMoveStart () {
		foreach (BoardObjectView bo in allObjectViews) {
			bo.UpdateVisualsPreMove ();
		}
	}

	private void AddViewsForAddedObjects () {
		foreach (BoardObject bo in myBoard.objectsAddedThisMove) {
			AddObjectView (bo);
		}
	}

	public void UpdateSimulatedMove (Vector2Int _simulatedMoveDir, float _simulatedMovePercent) {
		if (simulatedMoveDir != _simulatedMoveDir) { // If the proposed simulated moveDir is *different* from the current one...!
			SetSimulatedMoveDirAndBoard (_simulatedMoveDir);
		}
		else if (simulatedMoveDir != Vector2Int.zero) {
			UpdateViewsTowardsSimulatedMove (_simulatedMovePercent);
		}
	}
	private void UpdateViewsTowardsSimulatedMove (float _simulatedMovePercent) {
		objectsAnimationLocTarget = _simulatedMovePercent;
		objectsAnimationLocTarget *= 0.9f; // don't go all the way to 1.
		// Keep the value locked to the target value.
		objectsAnimationLoc = objectsAnimationLocTarget;
		areObjectsAnimating = false; // ALWAYS say we're not animating here. If we swipe a few times really fast, we don't want competing animations.
		ApplyObjectsAnimationLoc ();
	}
	private void ClearSimulatedMoveDirAndBoard () {
		simulatedMoveDir = Vector2Int.zero;
		// Animate all views back to their original positions.
		areObjectsAnimating = true;
		objectsAnimationLocTarget = 0;
	}
	/** Clones our current Board, and applies the move to it! */
	private void SetSimulatedMoveDirAndBoard (Vector2Int _simulatedMoveDir) {
		// If we accidentally used this function incorrectly, simply do the correct function instead.
		if (_simulatedMoveDir == Vector2Int.zero) {
			ClearSimulatedMoveDirAndBoard ();
			return;
		}

		simulatedMoveDir = _simulatedMoveDir;
		// Clone our current Board.
		simulatedMoveBoard = myBoard.Clone();
		// Set BoardOCCUPANTs' references within the new, simulated Board! NOTE: We DON'T set any references for BoardObjects. Those don't move (plus, there's currently no way to find the matching references, as BoardObjects aren't added to spaces).
		foreach (BoardObjectView bov in allObjectViews) {
			BoardObject thisSimulatedMoveBO = BoardUtils.GetObjectInClonedBoard (bov.MyBoardObject, simulatedMoveBoard);
			bov.SetMySimulatedMoveObject (thisSimulatedMoveBO);
		}
		// Now actually simulate the move!
		simulatedMoveBoard.ExecuteMove (simulatedMoveDir);//MoveResults simulatedMoveResult = 
		// Now that the simulated Board has finished its move, we can set the "to" values for all my OccupantViews!
		foreach (BoardObjectView bov in allObjectViews) {
			bov.SetValues_To_ByMySimulatedMoveBoardObject ();
		}
		// TODO: Different feedback for illegal moves?
	}




	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void FixedUpdate () {
//		MaybeCheckIfOccupantsFinishedAnimating ();
		if (areObjectsAnimating) {
			objectsAnimationLoc += (objectsAnimationLocTarget-objectsAnimationLoc) / 2f;
			ApplyObjectsAnimationLoc ();
			if (Mathf.Abs (objectsAnimationLocTarget-objectsAnimationLoc) < 0.01f) {
				UpdateAllViewsMoveEnd ();
			}
		}
	}
	private void ApplyObjectsAnimationLoc () {
		foreach (BoardObjectView bov in allObjectViews) {
			bov.GoToValues (objectsAnimationLoc);
		}
	}
//	private void MaybeCheckIfOccupantsFinishedAnimating () {
//		if (doCheckIfOccupantsFinishedAnimating) {
//			CheckIfOccupantsFinishedAnimating ();
//		}
//	}


}
