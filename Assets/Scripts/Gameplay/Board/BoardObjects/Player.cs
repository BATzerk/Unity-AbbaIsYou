using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : BoardOccupant {
	// Properties
	private bool isDead = false;
	public bool DidChangeColRowDuringThisMove; // set to FALSE before every move; set to TRUE if we call SetColRow (which we do every move). While looping through Players to move, if any of them ALREADY moved, we will SKIP their move (e.g. when Player exits a Portal, it'll move all other Players automatically, and we don't want them to ALSO move again.)
	// References
	private Beam beamThatKilledMe;

	// Getters
	public Beam BeamThatKilledMe { get { return beamThatKilledMe; } }
	public bool IsDead { get { return isDead; } }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public Player (Board _boardRef, PlayerData _data) {
		base.InitializeAsBoardOccupant (_boardRef, _data);
		SetPullType (MovementTypes.None); // Players can't pull Players.

		isDead = false; // Look alive.
	}
	public PlayerData SerializeAsData() {
		PlayerData data = new PlayerData (BoardPos);
		data.isMovable = IsMovable;
		data.isPassRotatable = IsPassRotatable;
		data.isSidePull = IsSidePull;
		return data;
	}

	override protected void UpdateCanBeamEnterAndExit () {
		SetBeamCanEnterAndExit (true); // player does NOT block beams at ALL!
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	override protected void OnSetColRow () {
		base.OnSetColRow ();
		DidChangeColRowDuringThisMove = true; // set this to true here!
	}
	public void Die (Beam _beamThatKilledMe) {
		isDead = true;
		beamThatKilledMe = _beamThatKilledMe;
//		RemoveMyFootprint ();
	}


}


