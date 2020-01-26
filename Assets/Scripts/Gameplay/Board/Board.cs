using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Board {
	// Properties
	private bool areGoalsSatisfied;
	private int numCols,numRows;
	// Reference Lists
	public List<IGoalObject> goalObjects; // UNUSED currently still!
	public List<BoardObject> allObjects; // includes EVERY BoardObject in every other list!
	public List<BoardOccupant> allOccupants; // JUST every BoardOccupant in the Board.
	public List<BoardObject> objectsAddedThisMove;
	// Objects
	public BoardSpace[,] spaces;
	public List<Crate> crates;
	public List<ExitSpot> exitSpots;
	public List<Player> players;
	public List<Wall> walls;

	// Getters
	public bool AreGoalsSatisfied { get { return areGoalsSatisfied; } }
	public int NumCols { get { return numCols; } }
	public int NumRows { get { return numRows; } }

	public BoardSpace GetSpace(int col,int row) { return BoardUtils.GetSpace(this, col,row); }
	private BoardOccupant GetOccupant(int col,int row) { return BoardUtils.GetOccupant(this, col,row); }
	public BoardSpace[,] Spaces { get { return spaces; } }
	public List<BoardOccupant> AllOccupants { get { return allOccupants; } }
	public List<Player> Players { get { return players; } }

	public Board Clone () {
		BoardData data = SerializeAsData();
		return new Board(data);
//		string jsonString = JsonUtility.ToJson (this); test.
//		return JsonUtility.FromJson<Board> (jsonString);
	}
	public BoardData SerializeAsData() {
		BoardData bd = new BoardData(numCols,numRows);
		foreach (Crate p in crates) { bd.crateDatas.Add (p.SerializeAsData()); }
		foreach (ExitSpot p in exitSpots) { bd.exitSpotDatas.Add (p.SerializeAsData()); }
		foreach (Player p in players) { bd.playerDatas.Add (p.SerializeAsData()); }
		foreach (Wall p in walls) { bd.wallDatas.Add (p.SerializeAsData()); }
		for (int col=0; col<numCols; col++) {
			for (int row=0; row<numRows; row++) {
				bd.spaceDatas[col,row] = GetSpace(col,row).SerializeAsData();
			}
		}
		return bd;
	}

	// Getters
	public bool IsAnyPlayerOnExitSpot () {
		foreach (Player p in players) {
			if (p.MySpace.HasExitSpot) { return true; }
		}
		return false;
	}
	private bool CheckAreGoalsSatisfied () {
		if (IsEveryPlayerDead ()) { return false; } // All Players are kaput? Nah, we need at least one alive to consider the level completed. :)
		if (goalObjects.Count == 0) { return true; } // If there's NO criteria, then sure, we're satisfied! For levels that're just about getting to the exit.
		for (int i=0; i<goalObjects.Count; i++) {
			if (!goalObjects[i].IsOn) { return false; } // return false if any of these guys aren't on.
		}
		return true; // Looks like we're soooo satisfied!!
	}
	public bool IsEveryPlayerDead () {
		foreach (Player p in players) {
			if (!p.IsDead) { return false; } // No, this guy is alive!
		}
		return true; // Yeah, they're all garlic toast.
	}


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public Board (BoardData bd) {
		numCols = bd.numCols;
		numRows = bd.numRows;

		// Add all gameplay objects!
		MakeEmptyPropLists ();
		MakeBoardSpaces (bd);
		AddPropsFromBoardData (bd);

		// Make everything all jiggy fresh!
		OnMoveComplete ();
	}

	private void MakeBoardSpaces (BoardData bd) {
		spaces = new BoardSpace[numCols,numRows];
		for (int i=0; i<numCols; i++) {
			for (int j=0; j<numRows; j++) {
				spaces[i,j] = new BoardSpace (this, bd.spaceDatas[i,j]);
			}
		}
	}
	private void MakeEmptyPropLists () {
		allObjects = new List<BoardObject>();
		allOccupants = new List<BoardOccupant>();
		goalObjects = new List<IGoalObject>();
		objectsAddedThisMove = new List<BoardObject>();

		crates = new List<Crate>();
		exitSpots = new List<ExitSpot>();
		players = new List<Player>();
		walls = new List<Wall>();

//		exitSpot = new ExitSpot (this, new BoardPos(numCols-1,0, 0,0)); // default it to the top-right.
	}
	private void AddPropsFromBoardData (BoardData bd) {
		// Add Props to the lists!
		foreach (ExitSpotData data in bd.exitSpotDatas) { AddExitSpot (data); }
		foreach (CrateData data in bd.crateDatas) { AddCrate (data); }
		foreach (CrateGoalData data in bd.crateGoalDatas) { AddCrateGoal (data); }
		foreach (PlayerData data in bd.playerDatas) { AddPlayer (data); }
		foreach (PusherData data in bd.pusherDatas) { AddPusher (data); }
		foreach (WallData data in bd.wallDatas) { AddWall (data); }
		// Add them to the actual Board, now!
		int layer = 0;
		int numOccupantsPlaced = 0; // once this hits the number of total Occupants, we will break out of the while loop. :)
		// VERY inefficiently loop through the ENTIRE list of Occupants, adding ONLY those of the layer we're on. This ensures Occupants are always correctly inside each other.
		while (true) {
			foreach (BoardOccupant bo in allOccupants) {
				// This is this guy's layer! Place it in the Board!
				if (bo.Layer == layer) {
					BoardUtils.PlaceNewOccupantInBoard (this, bo);
					numOccupantsPlaced ++;
				}
			}
			// If we've added everyone in the board, stop!
			if (numOccupantsPlaced >= allOccupants.Count) { break; }
			layer ++; // Look at the next layer!
		}
	}


	void AddExitSpot (ExitSpotData data) {
		ExitSpot prop = new ExitSpot (this, data.boardPos);
		exitSpots.Add (prop);
		allObjects.Add (prop);
	}
	void AddWall (WallData data) {
		Wall prop = new Wall (this, data.boardPos);
		walls.Add (prop);
		allObjects.Add (prop);
	}

	Crate AddCrate (CrateData data) {
		Crate prop = new Crate (this, data);
		crates.Add (prop);
		allOccupants.Add (prop);
		allObjects.Add (prop);
		return prop;
	}
	Player AddPlayer (PlayerData data) {
		Player prop = new Player (this, data);
		players.Add (prop);
		allOccupants.Add (prop);
		allObjects.Add (prop);
		return prop;
	}

	public void OnObjectRemovedFromPlay (BoardObject bo) {
//		// Make sure to remove its footprint in the grid!
//		bo.RemoveMyFootprint (); Note: This has already been done by this point.
		// Remove it from its lists!
		if (bo is BoardOccupant) { // Is it an Occupant? Remove it from allOccupants list!
			allOccupants.Remove (bo as BoardOccupant);
		}
		allObjects.Remove (bo);
        if (false) {}
		else if (bo is Crate) { crates.Remove (bo as Crate); }
		else if (bo is ExitSpot) { exitSpots.Remove (bo as ExitSpot); }
		else if (bo is Player) { players.Remove (bo as Player); }
		else if (bo is Wall) { walls.Remove (bo as Wall); }
		else { Debug.LogError ("Trying to RemoveFromPlay an Object of type " + bo.GetType().ToString() + ", but our OnObjectRemovedFromPlay function doesn't recognize this type!"); }
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	/** Moves all Players and the Occupants they'll pull with them (and pushes Occupants too of course).
	Returns TRUE if we made a successful, legal move, and false if we couldn't move anything. */
	public MoveResults ExecuteMove (Vector2Int dir) {
		// Clear out the Objects-added list just before the move.
		objectsAddedThisMove.Clear ();

//		// Controlling other objects instead testtt
//		BoardOccupant occupantToMove = allOccupants[0];
//		BoardUtils.MoveOccupant (this, occupantToMove, dir, true, true);
//		/*
		// Move Players!
		List<Player> playersToMove = new List<Player>(players); // make an independent list of Players to move, as the Board's list can change.

		foreach (Player p in playersToMove) { p.DidChangeColRowDuringThisMove = false; } // Reset all DidChangeColRowDuringThisMove.
		bool didAnyPlayerMove = false; // I'll say otherwise next!
		foreach (Player p in playersToMove) {
			if (!p.IsInPlay) { continue; } // If this one was destroyed by this point, just skip it.
			if (p.DidChangeColRowDuringThisMove) { continue; } // Oh, this guy's already been moved? Totally skip it.
			if (!BoardUtils.CanPlayerMoveInDir (this, p, dir)) { continue; } // If this Player can't move like this, skip it.
			// We CAN move this Player! Move it, and say we did move a Player.
			MoveResults result = BoardUtils.MoveOccupant (this, p, dir, true, true);
			if (result == MoveResults.Success) {
				didAnyPlayerMove = true; // Yes, at least this player moved!
			}
		}
		// If no Player was able to move, then return a Fail.
		if (!didAnyPlayerMove) { return MoveResults.Fail; }
		// Check that there are still Players.
		if (players.Count == 0) { return MoveResults.Fail; }
//		*/

		// Wowie, Chef! Looks like a success!
		OnMoveComplete ();
		return MoveResults.Success;
	}
	private void ApplyBoardChanges () {
		BoardUtils.ResetDidRotateThisStepForAllOccupants (this);
		// Redundantly check all the IGoalObjects, too.
		foreach (IGoalObject igo in goalObjects) { igo.UpdateIsOn (); }
	}
	private void OnMoveComplete () {
		// Apply changes to the Board!
		ApplyBoardChanges ();
		// First, check if all players are a toaster oven!
		UpdatePlayersIsDead ();
//		if (!IsEveryPlayerDead ()) { // todo: Eventually re-enable Pushers.
//			// First, move everyone that's over a Pusher!
//			BoardUtils.MoveAllOccupantsOverPushers (this);
//		}
		areGoalsSatisfied = CheckAreGoalsSatisfied ();
//		// Dispatch event!
//		GameManagers.Instance.EventManager.OnBoardFinishedMoveStep (this);
//		GameManagers.Instance.EventManager.OnBoardMoveComplete (this);
	}

	private void UpdatePlayersIsDead () {
		foreach (Player p in players) {
			BoardSpace bs = BoardUtils.GetSpace(this, p.Col,p.Row);
			if (bs.NumBeamsOverMe() > 0) {
				p.Die (bs.GetFirstBeam ());
			}
		}
	}



}
