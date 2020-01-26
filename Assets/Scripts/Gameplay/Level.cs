using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {
	// Components
	private Board board; // this reference ONLY changes when we undo a move, where we remake-from-scratch both board and boardView.
	private BoardView boardView;
	// Constant Properties
	private bool isBonus;
	private int worldIndex;
	private int levelIndex; // index within the entire world.
	private int parMoves;
	private string levelKey;
	// Variable Properties
	private bool isLevelComplete = false;
	private int numMovesMade; // reset to 0 at the start of each level. Undoing a move will decrement this.
	private List<BoardData> boardSnapshots; // for undoing moves! Before each move, we add a snapshot of the board to this list (and remove from list when we undo).
	// References
	private Board preMoveSnapshot; // a serialized version of me taken right before we try to do a move! If the move fails, we revert back to this.

	// Getters
	public bool IsBonus { get { return isBonus; } }
	public bool IsLevelComplete { get { return isLevelComplete; } }
	public int WorldIndex { get { return worldIndex; } }
	public int LevelIndex { get { return levelIndex; } }
	public int ParMoves { get { return parMoves; } }
	public string LevelKey { get { return levelKey; } }
	public bool IsEveryPlayerDead () { if(board==null){return false;} return board.IsEveryPlayerDead (); }

	/** If you ask ME, I'll report what's happened with ME, not if we've ever achieved par here. */
	public bool DidAchieveParMoves () { return numMovesMade <= parMoves; }
	public bool DidEverAchieveParMoves { get { return GameManagers.Instance.DataManager.GetLevelData(worldIndex,levelIndex).DidAchieveParMoves; } }

	private bool CanMakeAnyMove () {
		if (isLevelComplete) { return false; } // Once we beat the level, don't allow further movement. :)
		if (IsEveryPlayerDead()) { return false; } // If it's a ghost-town, don't allow further movement.
		// DISABLED waiting for the animation to finish. So we can make moves super quickly! And things will animate their best immediately to the next pos, which could cause visual weirdness. But it will feel snappier!
//		if (boardView.AreAnyOccupantsAnimating) { return false; } // Don't let me move until everyone's done animating to their positions!
		return true;
	}
	private bool CanUndoMove () {
		if (NumMovesMade <= 0) { return false; } // Can't go before time started, duhh.
		return true;
	}

	private int NumMovesMade {
		get { return numMovesMade; }
		set {
			numMovesMade = value;
			// Dispatch event!
			GameManagers.Instance.EventManager.OnNumMovesMadeChanged (numMovesMade);
		}
	}

//	public Board PreMoveSnapshot { get { return preMoveSnapshot; } }
//	public void SetPreMoveSnapshot () {
//		preMoveSnapshot = board.SerializeAsData ();
//	}


	// ----------------------------------------------------------------
	//  Initialize / Destroy
	// ----------------------------------------------------------------
	public void Initialize (GameController _gameControllerRef, Transform _tfWorld, LevelData _levelData) {
//		gameControllerRef = _gameControllerRef;
		this.transform.SetParent (_tfWorld);
		this.transform.localPosition = Vector3.zero;
		this.transform.localScale = Vector3.one;

		levelIndex = _levelData.levelIndex;
		levelKey = _levelData.levelKey;
		worldIndex = _levelData.worldIndex;
		parMoves = _levelData.parMoves;
		isBonus = _levelData.isBonus;

		// Reset easy stuff
		boardSnapshots = new List<BoardData>();
		NumMovesMade = 0;

		BoardData bd = _levelData.boardData;
		RemakeModelAndViewFromData (bd);
		// test
//		string jsonString = JsonUtility.ToJson (_levelData.boardData);
//		Debug.Log (jsonString);
	}
	public void DestroySelf () {
		// Tell my boardView it's toast!
		DestroyBoardModelAndView ();
		// Destroy my whole GO.
		Destroy (this.gameObject);
	}

	private void RemakeModelAndViewFromData (BoardData bd) {
		// Destroy them first!
		DestroyBoardModelAndView ();
		// Make them afresh!
		// If we're in PORTRAIT and our Board is too wide, ROTATE the entire board!
		if (bd.numCols>bd.numRows && ScreenHandler.IsPortrait()) {
			bd.RotateCCW ();
		}
		// If we're in LANDSCAPE and our Board is too tall, ROTATE the entire board!
		else if (bd.numCols<bd.numRows && ScreenHandler.IsLandscape()) {
			bd.RotateCW ();
		}
		board = new Board (bd);
		boardView = Instantiate (ResourcesHandler.Instance.prefabGO_boardView).GetComponent<BoardView>();
		boardView.Initialize (this, board);
	}
	private void DestroyBoardModelAndView () {
		// Nullify the model (there's nothing to destroy).
		board = null;
		// Destroy view.
		if (boardView != null) {
			boardView.DestroySelf ();
			boardView = null;
		}
	}

	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnBoardMoveComplete () {
		// Update BoardView visuals!!
		boardView.UpdateAllViewsMoveStart ();
		// If our goals are satisfied AND the player's at the exit spot, advance to the next level!!
		if (board.AreGoalsSatisfied && board.IsAnyPlayerOnExitSpot()) {
			CompleteLevel ();
		}
		// Dispatch success/not-yet-success event!
		GameManagers.Instance.EventManager.OnSetIsLevelCompleted (isLevelComplete);
	}
	private void CompleteLevel () {
		// Tell the WorldData what's up!
		GameManagers.Instance.DataManager.GetWorldData(worldIndex).OnCompleteLevel (levelKey, NumMovesMade);
		// Yes, we have!
		isLevelComplete = true;
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void MovePlayerInstances (int dirX,int dirY) { MovePlayerInstances(new Vector2Int(dirX,dirY)); }
	public void MovePlayerInstances (Vector2Int dir) {
		// If we can't move anywhere right now, stop.
		if (!CanMakeAnyMove ()) { return; }
		// If we can't make this specific move, also stop.
		if (!BoardUtils.CanExecuteMove (board, dir)) {
			return;
		}
		// We CAN make this move!
		else {
			// Take a snapshot and add it to our list!
			BoardData preMoveSnapshot = board.SerializeAsData();
			boardSnapshots.Add (preMoveSnapshot);
			// Move it, move it! :D
			board.ExecuteMove (dir); // This will always return success, because we already asked if this move was possible.
			// We make moves.
			NumMovesMade ++;
			// Complete this move!
			OnBoardMoveComplete ();
		}
	}

	public void UndoLastMove () {
		if (!CanUndoMove ()) { return; }
		// Get the snapshot to restore to, restore, and decrement moves made!
		BoardData boardSnapshotData = boardSnapshots[boardSnapshots.Count-1];
		// Remake my model and view from scratch!
		RemakeModelAndViewFromData (boardSnapshotData);
		boardSnapshots.Remove (boardSnapshotData);
		NumMovesMade --; // decrement this here!
		// No, the level is definitely not complete anymore.
		isLevelComplete = false;
		// Tie up loose ends by "completing" this move!
		OnBoardMoveComplete ();
	}


	public void Debug_PrintBoardAttributes() {
		board.Debug_PrintBoardAttributes();
	}

	public void UpdateSimulatedMove (Vector2Int _simulatedMoveDir, float _simulatedMovePercent) {
		if (boardView != null) { // Only for the editor.
			boardView.UpdateSimulatedMove (_simulatedMoveDir, _simulatedMovePercent);
		}
	}
//	/** When this happens, we want to cancel any animations already taking place. */
//	public void OnVJTouchDown () {
//		boardView.OnVJ_TouchDown ();
//	}


}
