using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpace {
	// Properties
	private BoardPos boardPos;
	private bool isPlayable = true;
	// References
	private List<Beam> beamsOverMe; // We keep a list of Beams, not BeamSegments, because the latter refs can be remade by the parent Beams. If a Beam is over us more than once (from Mirrors and/or Portals), we'll add it to the list again.
	private BoardOccupant myOccupant; // occupants sit on my face. Only one Occupant occupies each space, THOUGH other Occupants can exist INSIDE Occupants. This is just the TOP/OUTER-most Occupant.
	private ExitSpot myExitSpot; // only one allowed per space (obviously). Multiple ExitSpots allowed in the Board, though.
	private Pusher myPusher;
	private Wall[] myWalls; // references to the walls around me! Index = side (T, R, B, L).

	// Getters
	public bool IsPlayable { get { return isPlayable; } }
	public int Col { get { return boardPos.col; } }
	public int Row { get { return boardPos.row; } }
	public List<Beam> BeamsOverMe { get { return beamsOverMe; } }
	public BoardOccupant MyOccupant { get { return myOccupant; } }
	private OccupantContainer MyContainer { get { return myOccupant as OccupantContainer; } }
	public Pusher MyPusher { get { return myPusher; } }
	private bool HasImmovableOccupant() { return myOccupant!=null && !myOccupant.IsMovable; }
	public bool IsOpen() {
		return myOccupant==null;
	}
	public bool HasExitSpot { get { return myExitSpot!=null; } }
	public int NumBeamsOverMe () {
		return beamsOverMe.Count;
	}
	public bool CanBeamEnter (int sideEntering) {
		if (IsWallAtSide(sideEntering)) { return false; } // Wall in the way? Return false.
		if (myOccupant==null) { return true; } // Nobody's on me! Yes, beams are free to fly!
		return myOccupant.CanBeamEnter(sideEntering); // I can accept a beam if there's nothing on me, OR the thing on me doesn't block beams from this side!
	}
	public bool CanBeamExit (int sideExiting) {
		if (IsWallAtSide(sideExiting)) { return false; } // Wall in the way? Return false.
		if (myOccupant==null) { return true; } // Nobody's on me! Yes, beams are free to fly!
		return myOccupant.CanBeamExit(sideExiting); // I can accept a beam if there's nothing on me, OR the thing on me doesn't block beams from this side!
	}
	public int GetSideBeamExits (int sideEntered) {
		if (myOccupant==null) { return BoardUtils.GetOppositeSide (sideEntered); } // No BoardOccupant? Pass straight through me; simply return the other side.
		return myOccupant.SideBeamExits (sideEntered);
	}
	public bool CanOccupantEverEnterMe (Vector2Int dir) { return CanOccupantEverEnterMe (BoardUtils.GetSide (dir)); }
	public bool CanOccupantEverEnterMe (int side) {
		if (!IsPlayable) { return false; } // Unplayable? Return false.
		if (IsWallAtSide (side)) { return false; } // Wall in the way? Return false!
		// I have an immovable Occupant?
		if (HasImmovableOccupant()) {
			OccupantContainer myImmovableContainer = myOccupant as OccupantContainer;
			if (myImmovableContainer==null) { return false; } // I've got an immovable, non-Container. No way anyone else can ever enter me!
			return myImmovableContainer.IsSideEnterable (side); // I DO have an immovable Container! Return if we can ever enter its side this way.
		}
		return true; // Looks good!
	}
	public bool CanOccupantEverExit (int side) {
		// As long as there's no Wall here, we're good!
		return !IsWallAtSide (side);
	}
//	public bool CanOccupantExitMe (BoardOccupant _bo, Vector2Int dir) { return CanOccupantExitMe (_bo, BoardUtils.GetSide (dir)); }
//	/** Returns TRUE if this Occupant can exit me into the next space. This distinction is important for Portals, as Occupants don't exit into the next space from them (so we need another Portal-specific check elsewhere to cover both cases). */
//	public bool CanOccupantExitMe (BoardOccupant _bo, int side) {
//		if (IsWallAtSide(side)) { return false; } // Wall in the way? Return false!
//		// I have an Container (that isn't the same guy who's asking if it can leave)!
//		if (MyContainer!=null && MyContainer!=_bo) {
//			return MyContainer.CanOccupantExit(side);
//		}
//		return true; // Looks good!
//	}
//	/* * dirEntering: If we're 3,0 and 4,0 wants to know if it can pass into me, it will give me dirEntering of (1,0) (in this example, politely pre-converting the value to MY perspective). */
//	public bool CanBeamEverEnterMe (int side) {
//		return IsWallAtSide (side);
//	}
//	public bool CanBeamEverPassThroughSide (int side) {
//		return !IsWallAtSide (side);
//	}
	private bool IsWallAtSide (int side) {
		return myWalls[side] != null;
	}
	public BoardOccupant OccupantAtLayer (int layer) {
		if (layer == 0) { return MyOccupant; } // First layer? Just return myOccupant. :)
		// Layer>0 and I have a Container! Ask it for the Occupant.
		if (MyContainer != null) {
			return MyContainer.GetOccupantInsideMeAtLayer (layer);
		}
		// Layer's greater than 0 and I don't have a Container? Uhh, yeah, return null.
		return null;
//		OccupantContainer container = MyContainer;
//		// Look through every inner layer up to the one BEFORE the one we're looking for! They MUST all be Containers if there's an Occupant at the requested layer.
//		for (int i=1; i<layer; i++) {
//			container = container.ContainerInsideMe;
//			if (container == null) { return null; } // Whoa, there's no Container at this layer (so there's nowhere further to look). Return null.
//		}
//		return container;
	}
//	/** Used for when we make the initial board layout and need to know which layers to put things on. */
//	public int GetNumOccupants () {
//		BoardOccupant thisOccupant = myOccupant;
//		int total = 0;
//		while (thisOccupant != null) {
//			thisOccupant = thisOccupant.OccupantInsideMe;
//			total ++;
//		}
//		return total;
//	}

	/** Returns TRUE if this Beam is over me, AND it's NOT the Beam's first (origin) space! When it comes to satisfying criteria, we don't count the Space the Beam starts at (which is where its Source is too). */
	public bool HasBeamSansSource (int channelID) {
		foreach (Beam b in beamsOverMe) {
			if (b.ChannelID == channelID) {
				if (!b.IsSpaceMyOriginSpace (this)) {
					return true;
				}
			}
		}
		return false;
//		return BoardUtils.IsBeamOfChannelIDInList (beamsOverMe, channelID);
	}

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public BoardSpace (Board _boardRef, BoardSpaceData _data) {
		boardPos = _data.boardPos;
		isPlayable = _data.isPlayable;
		beamsOverMe = new List<Beam> ();
		myWalls = new Wall[4];
	}
	public BoardSpaceData SerializeAsData () {
		BoardSpaceData data = new BoardSpaceData(Col,Row);
		data.isPlayable = isPlayable;
		return data;
	}


	/** fitOutsideMyOccupant: Default is false. If true and I've already got an Occupant, I'll put myOccupant INSIDE this new guy and make the new guy myOccupant! If false and I've already got an Occupant, I'll simply put the new guy inside myOccupant. * /
	public void AddOccupant (BoardOccupant _bo, bool fitOutsideMyOccupant) {
		// I already have a Container!
		if (MyContainer != null) {
			// We want the new Occupant to eat my current Container!
			if (fitOutsideMyOccupant) {
				(_bo as OccupantContainer).AddOccupantInsideMe (myOccupant); // The provided guy MUST be a Container in this scenario.
				myOccupant = _bo; // Swap these guys; I want the outermost one.
			}
			// We want my CURRENT Container to eat the NEW Occupant!
			else {
				MyContainer.AddOccupantInsideMe (_bo);
			}
		}
		// I DON'T yet have an Occupant!...
		else {
			myOccupant = _bo;
		}
	}
	public void RemoveOccupant (BoardOccupant _bo) {
		if (myOccupant != null) {
			if (myOccupant == _bo) { // If my Occupant IS this guy, then just nullify it! (This is usually the case, Buckets/Portals being the exception.)
				myOccupant = null;
			}
			else {
				if (MyContainer == null) { Debug.LogError ("Whoa! We're trying to remove an Occupant from a Space's Container, but there's no Container here! " + Col+", "+Row); return; }
				MyContainer.RemoveOccupantInsideMe (_bo);
			}
		}
//		else {
//			throw new UnityException ("Whoa! We're trying to remove a " + _bo.GetType().ToString() + " from a space that doesn't own it! " + Col + " " + Row + ".");
//		}
	}
	*/
	public void SetMyOccupant (BoardOccupant _bo) {
		if (myOccupant != null) {
			throw new UnityException ("Oops! Trying to set a Space's Occupant, but that Space already has an Occupant! original: " + myOccupant.GetType().ToString() + ", new: " + _bo.GetType().ToString() + ". " + Col + ", " + Row);
		}
		myOccupant = _bo;
	}
	public void RemoveMyOccupant (BoardOccupant _bo) {
		if (myOccupant != _bo) {
			throw new UnityException ("Oops! We're trying to remove a " + _bo.GetType().ToString() + " from a space that doesn't own it! " + Col + " " + Row + ".");
		}
		myOccupant = null;
	}
	/** Use this for when we undo moves and want to really wipe things clean without any extra fuss. */
	public void NullifyMyOccupantAndBeams () {
		myOccupant = null;
		beamsOverMe.Clear();
	}
	public void SetMyExitSpot (ExitSpot _exitSpot) {
		if (myExitSpot != null) { Debug.LogError ("Error! Somehow we're setting the ExitSpot for a space that already has one. (How is that even possible?) " + Col + ", " + Row); return; }
		myExitSpot = _exitSpot;
	}
	public void SetMyPusher (Pusher _pusher) {
		if (_pusher!=null && myPusher!=null) { // We're trying to set a BoardOccupant, but we already HAVE one...
			throw new UnityException ("Whoa! We're trying to add a Pusher to a BoardSpace that ALREADY has one! " + Col + " " + Row);
		}
		myPusher = _pusher;
	}

	public void SetWallOnMe (Wall _wall, int side) {
		myWalls[side] = _wall;
	}

	public void AddBeam (Beam _beam) {
		beamsOverMe.Add (_beam);
	}
	public void RemoveBeam (Beam _beam) {
		beamsOverMe.Remove (_beam);
	}

}
