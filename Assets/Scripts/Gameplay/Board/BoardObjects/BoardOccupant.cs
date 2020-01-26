using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
abstract public class BoardOccupant : BoardObject {
	// Properties
	private bool isMovable;
	private bool isPassRotatable; // I rotate if something passes by me.
	private MovementTypes pullType; // how I can be pulled!
	protected bool[] canBeamEnter = {false,false,false,false}; // one for each side. Default all to false. My extensions will say otherwise.
	protected bool[] canBeamExit = {false,false,false,false}; // one for each side. Default all to false. My extensions will say otherwise.
	public Vector2Int InitializationMoveDir { get; set; } // The view needs to know this so it can animate itself entering a Portal properly. ;)
	// References
	private OccupantContainer containerOutsideMe;

	// Getters
	override public BoardPos BoardPos { get { if (containerOutsideMe!=null) { return new BoardPos(Col,Row, SideFacing, Layer); } return base.BoardPos; } } // Slave my position to any Container I'm in!
	override public int Col { get { if (containerOutsideMe!=null) { return containerOutsideMe.Col; } return base.Col; } } // Slave my position to any Container I'm in!
	override public int Row { get { if (containerOutsideMe!=null) { return containerOutsideMe.Row; } return base.Row; } } // Slave my position to any Container I'm in!
	public bool IsPassRotatable { get { return isPassRotatable; } }
	public bool IsMovable { get { return isMovable; } }
	public bool IsSidePull { get { return pullType==MovementTypes.PullSide; } }
	public MovementTypes PullType { get { return pullType; } }
	protected void SetPullType (MovementTypes _type) { pullType = _type; }

	public bool CanBeamEnter (int sideEntered) { return canBeamEnter[sideEntered]; }
	public bool CanBeamExit (int sideExited) { return canBeamExit[sideExited]; }
	virtual public int SideBeamExits (int sideEntered) {
		return BoardUtils.GetOppositeSide (sideEntered);
	}
	// Getters / Setters
	public System.Guid PortalCopyGuid { get; set; } // This is shared by all copies in a Portal when they're made (and the original BO too). Note that it's never cleared out, only overwritten. Used to determine if an Occupant is a copy or some rando who just wandered into this Portal.
	public OccupantContainer ContainerOutsideMe {
		get { return containerOutsideMe; }
		set {
			if (value == null) { RefreshBoardPos(); } // If we're nulling it out, then update my col/row to what it "was" (but wasn't actively set to)!
			containerOutsideMe = value;
		}
	}
	/** Returns ContainerOutsideMe, cast as a Portal. If the guy immediately outside of me is NOT a Portal (or is null), this is null. */
	public Portal PortalDirectlyOutsideMe { get { return ContainerOutsideMe as Portal; } }
	/** Keeps looking outward until we find a Portal. Nesting-friendly! */
	public Portal FirstPortalOutsideMe() {
		OccupantContainer thisContainer = ContainerOutsideMe;
		while (thisContainer != null) {
			if (thisContainer is Portal) { return thisContainer as Portal; }
			thisContainer = thisContainer.ContainerOutsideMe;
		}
		return null;
	}


//	private int GetNumOccupantsOutsideMe () {
//		int total = 0;
//		BoardOccupant nextOccupantOutsideMe = OccupantOutsideMe; // start with the first guy I'm inside (if it even exists).
//		while (true) {
//			if (nextOccupantOutsideMe == null) { break; }
//			nextOccupantOutsideMe = nextOccupantOutsideMe.OccupantOutsideMe;
//			total ++;
//		}
//		return total;
//	}


//	public void UpdateLayerFromNumOccupantsOutsideMe () {
//		int numOccupantsOutsideMe = GetNumOccupantsOutsideMe ();
//		SetLayer (numOccupantsOutsideMe);
//	}
//	private void SetIsMovable (bool _value) { isMovable = _value; }
//	private void SetIsPassRotatable (bool _value) { isPassRotatable = _value; }
//	private void SetIsSidePull (bool _value) { pullType = _value ? MovementTypes.PullSide : MovementTypes.PullBehind; }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	protected void InitializeAsBoardOccupant (Board _boardRef, BoardOccupantData _data) {
		base.InitializeAsBoardObject (_boardRef, _data.boardPos);
		// Set my basic Occupant properties!
		isMovable = _data.isMovable;
		isPassRotatable = _data.isPassRotatable;
		pullType = _data.isSidePull ? MovementTypes.PullSide : MovementTypes.PullBehind;
	}
	/** This simply sets me (and all inner Occupants) removed from play. Doesn't affect footprints or outer Occupants. Called by BoardUtils when we move through the non-exit side of a Portal. */
	virtual public void RemoveFromPlayFromPortalPassage () {
		base.RemoveFromPlay ();
		// TEMP but maybe okay to keep forever: Move me to the other side of my Portal, for my view to animate me out.
		Portal firstPortalOutsideMe = FirstPortalOutsideMe();
		// If I'm inside a Container, remove me from it!
		if (ContainerOutsideMe != null) {
			BoardUtils.RemoveOccupantFromItsContainer (this, true);
		}
		if (firstPortalOutsideMe != null) {
			Vector2Int moveDir = BoardUtils.GetOppositeDir (firstPortalOutsideMe.SideFacing);
			SetColRow (firstPortalOutsideMe.Col+moveDir.x, firstPortalOutsideMe.Row+moveDir.y);
		} else { Debug.LogError ("Weird, we're removing an Occupant from play from a Portal passage... but it's not in a Portal."); }
	}
		
//		if (isDeepRemoval) {
//			// Destroy any Occupant inside me FIRST!
//			if (occupantInsideMe != null) {
//				occupantInsideMe.RemoveFromPlayFromPortalPassage ();
//				occupantInsideMe = null;
//			}
//			// Remove me from the guy outside me without affecting my layer.
//			if (occupantOutsideMe != null) {
//	//			occupantOutsideMe..RemoveOccupantInsideMe (this);
//				occupantOutsideMe.occupantInsideMe = null;
//				occupantOutsideMe = null;
//			}
//		}
//		base.RemoveFromPlay ();


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	/** Extensions of this class will call this if they totally don't obstruct beams OR other Occupants. */
	virtual protected void UpdateCanBeamEnterAndExit () { }
	protected void SetBeamCanEnterAndExit (bool _canEnterAndExit) {
		for (int i=0;i<4;i++) { canBeamEnter[i]=canBeamExit[i]=_canEnterAndExit; }
	}
	override protected void OnSetSideFacing () {
		base.OnSetSideFacing ();
		UpdateCanBeamEnterAndExit ();
	}

//	/** Call this pos-insensitive version of this function as a result from another Occupant that's moving out of a Portal! */
//	public void Move (Vector2Int moveDir) {
//		Move (Col+moveDir.x, Row+moveDir.y, moveDir);
//	}
//	public void Move (int _col,int _row, Vector2Int moveDir) {
//		SetColRow (_col,_row);
//		// Hey! Maybe move all other Occupants within my Portal's PortalChannel!
//		if (portalOutsideMe != null) {
//			portalOutsideMe.MyChannel.MoveAllOccupantsInMyPortalsInDir (moveDir);
//		}
//	}
	override public void SetColRow (int _col,int _row) {
		if (ContainerOutsideMe != null) {
			Debug.LogError ("We're trying to set the Col,Row of an Occupant that's INSIDE a Container! This isn't allowed. " + Col+", "+Row);
			return;
		}
		base.SetColRow (_col,_row);
	}

	virtual public void AddMyFootprint () {
		// If I'm the outermost Occupant, then tell my Space I'm it's guy!
		if (Layer == 0) {
			MySpace.SetMyOccupant (this);
		}
	}
	virtual public void RemoveMyFootprint () {
		// If I'm the outermost Occupant, then tell my Space I'm outta here!
		if (Layer == 0) {
			MySpace.RemoveMyOccupant (this);
		}
	}



}
