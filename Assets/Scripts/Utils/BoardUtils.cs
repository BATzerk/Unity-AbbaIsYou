using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardUtils {
	static private int numTimesCheckedCanOccupantMoveInDirSinceLastMoveExecutionCheck = 0; // so we don't get caught in an infinite loop when trying to make a move. As this code grows, this check should become less and less necessary, but it's worth it to leave it in for forever, juuust in case.

	// ----------------------------------------------------------------
	//  Basic Getters
	// ----------------------------------------------------------------
	public static bool IsSpaceEven (int col,int row) {
		bool isColEven = col%2 == 0;
		if (row%2 == 0) { // If it's an EVEN row, return if it's an even col!
			return isColEven;
		}
		return !isColEven; // If it's an ODD row, return if it's NOT an even col!
	}
	private static bool IsPlayableSpace (Board b, int col,int row) {
//		if (!(col>=0 && row>=0 && col<b.NumCols && row<b.NumRows)) { return false; } // Not in bounds?? So false.
		BoardSpace bs = GetSpace (b, col,row);
		return bs!=null && bs.IsPlayable;
	}


	public static List<BoardOccupant> GetOccupantsToPullWithPlayer (Board b, Player p, Vector2Int dir) {
		List<BoardOccupant> bosToPull = new List<BoardOccupant>();
		AddPullableOccupantToList (b, ref bosToPull, p, new Vector2Int(-dir.x,-dir.y), MovementTypes.PullBehind); // The guy behind the Player!
		AddPullableOccupantToList (b, ref bosToPull, p, new Vector2Int(-dir.y,-dir.x), MovementTypes.PullSide); // A Side-Pull guy!
		AddPullableOccupantToList (b, ref bosToPull, p, new Vector2Int( dir.y, dir.x), MovementTypes.PullSide); // The other Side-Pull guy!
		return bosToPull;
	}
	/*
	public static List<BoardOccupant> GetOccupantsToPullWithPlayers (Board b, Vector2Int dir) {
		List<BoardOccupant> bosToPull = new List<BoardOccupant>();
		foreach (Player p in b.players) {
			AddPullableOccupantToList (b, ref bosToPull, p, new Vector2Int(-dir.x,-dir.y), MovementTypes.PullBehind); // The guy behind the Player!
			AddPullableOccupantToList (b, ref bosToPull, p, new Vector2Int(-dir.y,-dir.x), MovementTypes.PullSide); // A Side-Pull guy!
			AddPullableOccupantToList (b, ref bosToPull, p, new Vector2Int( dir.y, dir.x), MovementTypes.PullSide); // The other Side-Pull guy!
		}
		return bosToPull;
	}*/

	/** Returns TRUE if the Player is inside an Occupant that has a Side here. */ // TODO: Does this return the same result as IsPlayerInsideContainerWithNonExitableSide? If so, merge these two functions.
	private static bool IsPlayerShelteredInsideContainerAtSide (Board b, Player player, int side) {
		// This condition is FALSE if the Player is the outermost Occupant at this side!
		return GetOutermostOccupantAtSide (b, player.Col,player.Row, side) != player;
	}

	public static bool AreOccupantsSamePortalCopies (BoardOccupant boA, BoardOccupant boB) {
		if (boA==null || boB==null) { return false; } // If either is null, return false.
		return System.Guid.Equals (boA.PortalCopyGuid, boB.PortalCopyGuid);
	}
	/** Returns true if they're the same type, and all properties match (excluding col, row, sideFacing, and layer). Also checks nested Occupants. * /
	public static bool AreOccupantsIdentical (BoardOccupant boA, BoardOccupant boB) {
		if (boA==null || boB==null) { return false; } // If either is null, return false.
		if (boA.GetType() != boB.GetType()) { return false; } // If they're not even the same type, return false, duh.
		// Compare their properties!
//		if (!boA.IsOccupantIdenticalToMe (boB)) { return false; }
		if (boA.IsMovable != boB.IsMovable
			|| boA.IsSidePull != boB.IsSidePull
			|| boA.IsPassRotatable != boB.IsPassRotatable) {
			return false;
		}

		// They match so far! If either of them has anything inside us, compare THOSE guys instead!!
		if (boA.OccupantInsideMe!=null || boB.OccupantInsideMe!=null) {
			return BoardUtils.AreOccupantsIdentical (boA.OccupantInsideMe, boB.OccupantInsideMe);
		}
		// All relevant properties match
		return true;
	}
	*/
	/** Returns true if both Occupants are Portals and of the same channel. */
	public static bool AreOccupantsSameChannelPortals (BoardOccupant boA, BoardOccupant boB) {
		if (boA is Portal && boB is Portal) {
			return ((boA as Portal).MyChannel == (boB as Portal).MyChannel);
		}
		return false;
	}
	private static bool IsPlayerInsideSamePortalChannel (Player player, BoardOccupant bo) {
		// This other Occupant IS a Portal!
		if (bo!=null && bo is Portal) {
			int channelID = (bo as Portal).MyChannel.ChannelID;
			return IsOccupantInsidePortalOfChannel (player, channelID);
		}
		// This other Occupant isn't even a Portal, so return false. :)
		return false;
	}
	private static bool IsOccupantInsidePortalOfChannel (BoardOccupant occupant, int channelID) {
		if (occupant==null || occupant.ContainerOutsideMe==null) { return false; }
		if (occupant.ContainerOutsideMe is Portal && (occupant.ContainerOutsideMe as Portal).MyChannel.ChannelID == channelID) { return true; }
		// Try the next Occupant outside this guy.
		return IsOccupantInsidePortalOfChannel (occupant.ContainerOutsideMe, channelID);
	}

	/** Players aren't like other Occupants-- they can't move the Container they're inside. We wanna make sure that every Player isn't inside a Container that prohibits this move. */
	private static bool AreAllPlayersInsideContainerWithNonExitableSide (List<Player> players, Vector2Int dir) {
		int side = GetSide (dir);
		foreach (Player p in players) {
			if (!IsPlayerInsideContainerWithNonExitableSide (p, side)) { // There's an opening here!
				return false;
			}
		}
		return true; // Alas, they're all surrounded.
	}
	private static bool IsPlayerInsideContainerWithNonExitableSide (Player p, int side) {
		return p.ContainerOutsideMe!=null && !p.ContainerOutsideMe.IsSideExitable(side);
	}

	/** Returns true simply if this Container's side is exitable. */
	public static bool CanOccupantExitContainerOutsideIt (BoardOccupant bo, Vector2Int dir) {
		return bo.ContainerOutsideMe.IsExitSide(dir);
	}
	public static bool CanOccupantExitContainerOutsideItIntoNextSpace (Board b, BoardOccupant bo, Vector2Int dir) {
		// If this Occupant can't even exit the Container in this direction, return false.
		if (!CanOccupantExitContainerOutsideIt(bo, dir)) { return false; }
		// Okay, is there an open space in this direction?
		return CanOccupantMoveInDir (b, bo, dir);
	}


	public static bool CanExecuteMove (Board originalBoard, Vector2Int dir) {
		numTimesCheckedCanOccupantMoveInDirSinceLastMoveExecutionCheck = 0; // reset this counter now!
		// FIRST off, if every Player instance is inside an Occupant with a side here, we can't move anywhere.
		if (AreAllPlayersInsideContainerWithNonExitableSide (originalBoard.players, dir)) {
			return false;
		}
		// Clone the Board, execute the exact move, and return if it worked.
		Board testBoard = originalBoard.Clone ();
		MoveResults moveResult = testBoard.ExecuteMove (dir);
		return moveResult != MoveResults.Fail;
	}
	public static bool CanOccupantMoveInDir (Board originalBoard, BoardOccupant originalBO, Vector2Int moveDir) {
		// Increment our static counter! If we're caught in an infinite loop, stop it here.
		if (numTimesCheckedCanOccupantMoveInDirSinceLastMoveExecutionCheck++ > 200) {
			Debug.LogError ("Infinite loop of Board creating! Stopping checks now. Time to fix some Portal code! :D");
			return false;
		}
		Board testBoard = originalBoard.Clone ();
		BoardOccupant testBO = GetOccupant (testBoard, originalBO.BoardPos.col,originalBO.BoardPos.row, originalBO.BoardPos.layer);
		MoveResults moveResult = MoveOccupant (testBoard, testBO, moveDir, true, false); // testBoard.MovePlayersAndAccompanyingOccupants (moveDir);
		return moveResult == MoveResults.Success;//!= MoveResults.Fail
	}
	public static bool CanPlayerMoveInDir (Board b, Player p, Vector2Int dir) {
		// If the Player can't escape a Container, then no, it can't move. Players alone have this extra restriction, for gameplay sensibility.
		if (IsPlayerInsideContainerWithNonExitableSide(p, GetSide(dir))) { return false; }
		return CanOccupantMoveInDir (b, p, dir);
	}

	/*
	private static bool CanAnyPlayerMoveInDir (Board b, Vector2Int dir) {
		// Players aren't like other Occupants-- they can't move the Container they're inside. Make sure that every Player isn't inside a Container that prohibits this move.
		if (AreAllPlayersInsideOccupantWithSide (b.players, dir)) {
			return false;
		}
		foreach (Player p in b.players) {
			if (CanOccupantMoveInDir (b, p, dir)) {
				return true;
			}
		}
		return false; // Alas, there's nowhere to go for any of 'em.
	}
	*/

	/** NOTE: This is part of uneven logic. This function can ONLY return BoardOCCUPANTS, and that's what it's used for. */
	public static BoardObject GetObjectInClonedBoard (BoardObject sourceBO, Board newBoard) {
		return GetOccupant (newBoard, sourceBO.Col,sourceBO.Row, sourceBO.Layer);
	}


	private static void AddPullableOccupantToList (Board b, ref List<BoardOccupant> boList, Player player, Vector2Int boDir, MovementTypes pullType) {
		AddPullableOccupantToList (b, ref boList, player, GetSide(boDir), pullType);
	}
	private static void AddPullableOccupantToList (Board b, ref List<BoardOccupant> boList, Player player, int side, MovementTypes pullType) {
		// If there's a guy we can pull here, add it to da list!
		BoardOccupant occupant = OccupantPlayerCanPullAtSide (b, player, side, pullType);
		if (occupant != null) {
			boList.Add (occupant);
		}
	}
	public static BoardOccupant OccupantPlayerCanPullAtSide (Board b, Player player, int playerSide, MovementTypes pullType) {
		Vector2Int boDir = GetDir (playerSide);
		// Can the Player not ACCESS this side because it's inside an Occupant??
		if (IsPlayerShelteredInsideContainerAtSide (b, player, GetSide(boDir))) {
			return null;
		}
		int col = player.Col + boDir.x;
		int row = player.Row + boDir.y;
//		// Nobody can ever move this way? Do nothin'.
//		if (!CanAnOccupantHereEverMoveInDir(b, col,row, moveDir)) { return; }
		int boSide = GetOppositeSide(playerSide);
		BoardOccupant occupant = GetOutermostMovableOccupantAtSide (b, col,row, boSide);
		if (occupant == null) { return null; } // No occupant? Return null (obviously).
		if (occupant.PullType == MovementTypes.None) { return null; } // This Occupant cannot be pulled.
		if (pullType!=MovementTypes.Any && occupant.PullType != pullType) { return null; } // We DO care about the pullType, AND they don't match? Return null, Em.
		// Is the Player INSIDE a Portal, AND this guy is a Portal in the same Channel?? Return NULL! We can't pull Portals inside themselves.
		if (IsPlayerInsideSamePortalChannel (player, occupant)) { return null; }
		// Oh! This guy is suitable! :)
		return occupant;
	}

	public static void RotateOccupantsFromPassing (Board b, int pcol,int prow, int dirX,int dirY) {
		// HORZ move!
		if (dirX!=0) {
			RotateOccupant (b, pcol,prow-1, -dirX, 0); // passing AWAY from
			RotateOccupant (b, pcol,prow+1, dirX, 2); // passing AWAY from
			RotateOccupant (b, pcol+dirX,prow-1, -dirX, 0); // passing INTO
			RotateOccupant (b, pcol+dirX,prow+1, dirX, 2); // passing INTO
		}
		// VERT move!
		else {
			RotateOccupant (b, pcol-1,prow, dirY, 3); // passing AWAY from
			RotateOccupant (b, pcol+1,prow, -dirY, 1); // passing AWAY from
			RotateOccupant (b, pcol-1,prow+dirY, dirY, 3); // passing INTO
			RotateOccupant (b, pcol+1,prow+dirY, -dirY, 1); // passing INTO
		}
		//  BoardOccupant[] occupantsToRotate = getBoardOccupantsToRotateFromPassing (dirX,dirY);
		//  for (int i=0; i<occupantsToRotate.length; i++) {
		//    getBoardOccupantsToRotateFromMePassingBy.rotateFromPass (col,row, dirX,dirY);
		//  }
	}
	public static void RotateOccupant (Board b, int col,int row, int rotationDir, int sideFacingRequired) {
		BoardOccupant bo = GetOccupant (b, col,row);
		if (bo!=null && bo.IsPassRotatable) {// && bo.sideFacing==sideFacingRequired) {
			if (!bo.DidRotateThisStep) {
				bo.RotateMe (rotationDir);
			}
		}
	}

	public static void MoveAllOccupantsOverPushers (Board b) {
		// Make a LIST of all the occupants to move, THEN move them, so we don't move one object multiple times because we're going through the Pusher list.
		List<BoardOccupant> bosToPush = new List<BoardOccupant> ();
		List<Vector2Int> bosToPushPoses = new List<Vector2Int> (); // we'll ALSO make a list of where all the BO's to push ARE. If their position has changed by the time we get to them, then don't do anything with them! They've already been pushed by another chain reaction.
		foreach (Pusher p in b.Pushers) {
			BoardOccupant bo = GetOutermostMovableOccupantAtSide (b, p.Col,p.Row, p.SideFacing);
			// This pusher has a BoardOccupant over it! Add it to the list.
			if (bo != null) {
				bosToPush.Add (bo);
				bosToPushPoses.Add (new Vector2Int(bo.Col,bo.Row));
			}
		}
		// Push the ones I promised I'd push!
		for (int i=0; i<bosToPush.Count; i++) {
			BoardOccupant bo = bosToPush[i];
			Vector2Int boPos = bosToPushPoses[i];
			// FIRST, if this BO has MOVED since we added it to our list, that means it's been pushed in a chain reaction! Skip it-- it's no longer where we thought it was.
			if (bo.Col!=boPos.x || bo.Row!=boPos.y) { continue; }
			Pusher pusher = GetPusher (b, bo.Col,bo.Row);
			if (CanOccupantMoveInDir (b, bo, pusher.Dir)) {
				MoveOccupant (b, bo, pusher.Dir, true, true);
			}
		}
	}

	public static MoveResults MoveOccupant (Board b, BoardOccupant bo, Vector2Int dir, bool canInnerOccupantsFallOut, bool doMoveCopiesInPortals) {
		if (bo == null) {
			Debug.LogError ("Oops! We're trying to move a null Occupant! dir: " + dir.ToString());
			return MoveResults.Fail;
		}
		// Is this the Player?? Rotate the objects it's passing by!
		if (bo is Player) {
			RotateOccupantsFromPassing (b, bo.Col,bo.Row, dir.x,dir.y);
		}
		// Is this the Player? Plan to pull some dudeees!
		List<BoardOccupant> occupantsToPullAlong = null;
		if (bo is Player) {
			occupantsToPullAlong = GetOccupantsToPullWithPlayer (b, bo as Player, dir);
		}

		int newCol = bo.Col+dir.x;
		int newRow = bo.Row+dir.y;

//		Debug.Log ("Moving Occupant. " + bo.GetType().ToString() + "  " + bo.Col + "," + bo.Row + " to " + newCol + "," + newRow);

		// When we FIRST try to move an Occupant inside a Portal, ask the PortalChannel if ANY of its Occupants can exit a Portal in this dir.
		//		if TRUE, then move all Occupants in the PortalChannel
		//		else, don't do any portal stuff.

		// We're inside a Portal??
		Portal boPortalOutsideMe = bo.PortalDirectlyOutsideMe;
		if (boPortalOutsideMe != null) { // We're moving inside a Portal!
			PortalChannel pc = boPortalOutsideMe.MyChannel;
			bool canAnyOccupantExitInDir = true; // Assume they can, unless we explicitly check and it turns out they can't.
			// We can move ALL Occupants in this PortalChannel!
			if (doMoveCopiesInPortals) {
				if (!pc.IsMovingAllOccupants) { // This channel ISN'T already moving all its Occupants.
					// Can ANY Occupants exit a Portal in this direction??
					if (pc.CanAnyOccupantExitInDir (dir, boPortalOutsideMe)) {
						// Great! Move (or destroy) EACH of them!
						pc.MoveOrDestroyEachOccupant (dir, boPortalOutsideMe);
						// Stop the Move function here, as we've already taken care of this Occupant's move along with all other Occupants in this PortalChannel.
						return MoveResults.Success; // Note: We can definitely return Success, because we wouldn't be here unless at least one Occupant in the PortalChannel could move in this dir.
					}
					else {
						// Ah, no Occupants can exit any Portals in this direction? Cool. Do NOTHING Portal-related; Portals will continue to be treated just as OccupantContainers.
						canAnyOccupantExitInDir = false; // No, they can't.
					}
				}
			}
			// Portaling action is happening! Do further Portal-related stuff.
			if (canAnyOccupantExitInDir) {
				// There's a Portal outside me, and I'm NOT moving out of its Exit Side?? This means I'm going off the Board and staying out of play (though we still want to continue this function so I can pull along Occupants I planned on pulling, etc.).
				if (boPortalOutsideMe.IsWarpSide (GetSide(dir))) {
					bo.RemoveFromPlayFromPortalPassage ();
				}
			}
		}


		// Are we trying to move OUT of, or INTO a blocked side of an immovable Occupant? Also return Fail.
		if (IsImmovableOccupantWithNonExitableSide(b, bo.Col,bo.Row, GetSide(dir))) {
			return MoveResults.Fail;
		}
		if (IsImmovableOccupantWithNonEnterableSide(b, newCol,newRow, GetOppositeSide(dir))) {
			return MoveResults.Fail;
		}
		// Are we trying to pass through a Wall? Return Fail.
		if (bo.MySpace!=null && !bo.MySpace.CanOccupantEverExit(GetSide(dir))) {
			return MoveResults.Fail;
		}

		// Always remove its footprint first. We're about to move it!
		bo.RemoveMyFootprint ();

		// Can any Occupants in this guy fall out?
		if (canInnerOccupantsFallOut) {
			MakeInnerOccupantsFallOutOfContainer (bo as OccupantContainer, dir);
		}


		// It's still in play! (Hasn't been removed from play by warping through a Portal!)
		if (bo.IsInPlay) {
			// BO *IS* in a Container! Either move its Container, OR remove it from its Container!
			if (bo.ContainerOutsideMe != null) {
				// BO CAN exit this side!
				if (CanOccupantExitContainerOutsideIt(bo, dir)) {
					// Remove it from its container and update its layer!
					RemoveOccupantFromItsContainer (bo, true);
				}
				// BO *CANNOT* exit this side.
				else {
					// ...AND BO's in an immovable Container! Stop here; return fail.
					if (!bo.ContainerOutsideMe.IsMovable) {
						return MoveResults.Fail;
					}
					// If this is the Player, DON'T move the Container outside it. To prevent Portal scooching. NOTE: Not thoroughly tested!
					if (bo is Player) {
						return MoveResults.Fail;
					}
					// Otherwise, move the container (*without* letting the inner Occupants fall out), returning that result!
					return MoveOccupant (b, bo.ContainerOutsideMe, dir, false, doMoveCopiesInPortals);
				}
			}

			// BO is the outermost Occupant! Update its position directly, then. :)
			if (bo.ContainerOutsideMe == null) { //note: do we wanna still do this even if it's removed from play, just for the view?
				bo.SetColRow (newCol,newRow);
			}

			// Is there another movable Occupant in this next space that we might displace??
			BoardOccupant nextBO_touching = GetOutermostOccupantAtSide (b, newCol,newRow, GetOppositeSide(dir));
			if (nextBO_touching != null) {
				// Can the outermost Occupant in the next space fit inside US??
				BoardOccupant nextBO_outermost = GetOccupant (b, newCol,newRow); // Note that we don't use boInNextSpace! Instead, we want to try to swallow the WHOLE Occupant in the next space.
				if (CanOccupantEnterContainer (nextBO_outermost, bo as OccupantContainer, GetSide(dir))) {
					nextBO_outermost.RemoveMyFootprint (); // Remove its footprint! It's about to get pwned.
					PutOccupantInContainer (nextBO_outermost, bo as OccupantContainer); // Put de lime in de coconut.
				}
				// Can WE fit inside this next Occupant??
				else if (CanOccupantEnterContainer (bo, nextBO_touching as OccupantContainer, GetOppositeSide(dir))) {
					PutOccupantInContainer (bo, nextBO_touching as OccupantContainer); // Put de coconut in de lime.
				}
				// Neither of us can fit inside the other, BUT the guy we're touching is movable!...
				else if (nextBO_touching.IsMovable) {
					// PUSH the next guy! Interrupt this function with a recursive call! (And we'll add everyone back to the board at the end.)
					MoveResults nextResult = MoveOccupant (b, nextBO_touching, dir, true, true); // Despite if WE could let Occupants fall out, we DO want this guy's Occupants to be able to fall out.
					// Whoa, if the recursive move we just made DIDN'T work, then stop here and return that info.
					if (nextResult != MoveResults.Success) {
						return nextResult;
					}
					// Okay, now that we've displaced this guy, check AGAIN if we can enter the Container in the next space! (It could be a Portal that's now vacant!)
					nextBO_touching = GetOutermostOccupantAtSide (b, newCol,newRow, GetOppositeSide(dir)); // Recalculate it; it coulda changed. (It could now be an empty Portal!)
					if (CanOccupantEnterContainer (bo, nextBO_touching as OccupantContainer, GetOppositeSide(dir))) {
						PutOccupantInContainer (bo, nextBO_touching as OccupantContainer); // Put de coconut in de lime.
					}
				}
			}

			// Is this outside the board? Oops! Return Fail. (NOTE: Moved this down from above the other check at the top of this function. Enables more Portal business to work.)
			if (!IsPlayableSpace (b, newCol,newRow)) {
				return MoveResults.Fail;
			}

			// Add its footprint back now.
			bo.AddMyFootprint ();
		}

		// Did we plan on pulling along stuff with us? Move them, too!!
		if (occupantsToPullAlong != null) {
			for (int i=occupantsToPullAlong.Count-1; i>=0; --i) {
				if (BoardUtils.CanOccupantMoveInDir (b, occupantsToPullAlong[i], dir)) {
					BoardUtils.MoveOccupant (b, occupantsToPullAlong[i], dir, true, true);
				}
			}
		}

		// Success!
		return MoveResults.Success;
	}


	/** Call this to insert a freshly made Occupant into the Board. Will add its footprint, AND add it to any Containers already in this space! */
	public static void PlaceNewOccupantInBoard (Board b, BoardOccupant bo) {
		// Container here? Put it in the innermost one!
		OccupantContainer innermostContainer = GetInnermostEmptyContainer (b, bo.Col,bo.Row);
		if (innermostContainer != null) {
			PutOccupantInContainer (bo, innermostContainer);
		}
		// Finally, add the Occupant's footprint.
		bo.AddMyFootprint ();
	}
	private static OccupantContainer GetInnermostEmptyContainer (Board b, int col,int row) {
		OccupantContainer outermostContainer = GetOutermostContainer (b, col,row);
		OccupantContainer thisContainer = outermostContainer;
		if (thisContainer == null) { return null; } // Oh if there's no Container here, we can just return null right away.
		while (true) {
			// No Occupant inside this Container? Return IT!
			if (thisContainer.OccupantInsideMe == null) {
				return thisContainer;
			}
			// The next guy ISN'T a Container? Uh-oh, that means the bottommost guy isn't empty! Return null!
			if (thisContainer.ContainerInsideMe == null) {
				return null;
			}
			// Otherwise, look in the NEXT Container.
			thisContainer = thisContainer.ContainerInsideMe;
		}
	}


	private static void PutOccupantInContainer (BoardOccupant bo, OccupantContainer container) {
		// If this Container has a Container in it, put the Occupant in that instead!!
		if (container.ContainerInsideMe != null) {
			PutOccupantInContainer (bo, container.ContainerInsideMe);
			return;
		}
		if (container.OccupantInsideMe != null) {
			throw new UnityException ("Oops! We're trying to put a " + bo.GetType().ToString() + " inside a " + container.GetType().ToString() + ", but the Container ALREADY HAS an Occupant inside it!");
		}
		if (!container.CanOccupantBeInsideMe(bo)) {
			throw new UnityException ("Oops! We're trying to put a " + bo.GetType().ToString() + " inside a " + container.GetType().ToString() + ", but the latter Prop isn't allowed inside the former.");
		}
		container.OccupantInsideMe = bo;
		bo.ContainerOutsideMe = container;
		bo.SetLayer (container.Layer+1);
	}
	public static void RemoveOccupantFromItsContainer (BoardOccupant bo, bool doKickDirectlyToTopLayer) {
		OccupantContainer container = bo.ContainerOutsideMe;
		// This ISN'T the guy already inside me? Oops.
		if (container.OccupantInsideMe != bo) { throw new UnityException ("Oops, we tried removing a " + bo.GetType().ToString() + " from a " + container.GetType().ToString() + ", but the former Occupant isn't inside the latter!"); }
		// Remove the affiliation.
		bo.ContainerOutsideMe = null;
		container.OccupantInsideMe = null;
		// Occupant still in play? Kick 'em out to the next top-most layer, and maybe put them in a Container they're still nested in!
		if (bo.IsInPlay) {
			OccupantContainer fartherOuterContainer = container.ContainerOutsideMe;
			if (!doKickDirectlyToTopLayer && fartherOuterContainer != null) {
				PutOccupantInContainer (bo, fartherOuterContainer);
			}
			else { // Otherwise, just kick it to the top layer.
				bo.SetLayer (0);
			}
		}
	}
	private static void MakeInnerOccupantsFallOutOfContainer (OccupantContainer container, Vector2Int moveDir) {
		if (container==null || container.OccupantInsideMe==null) { return; } // Null Container, OR no inner Occupant? Do nothing.
		// If there's an open side here, then release the Occupant within the Container into this space!
		if (container.IsSideEnterable(GetOppositeSide(moveDir))) {// QQQ IsSideExitable
			BoardOccupant innerOccupant = container.OccupantInsideMe;
			innerOccupant.RemoveMyFootprint ();
			RemoveOccupantFromItsContainer (innerOccupant, false);
			innerOccupant.AddMyFootprint (); // Now add it now that it's strong and independent!!
		}
	}

	private static bool CanOccupantEnterContainer (BoardOccupant inner, OccupantContainer outer, int side) {
		// If the outer guy is null, return false, a-doy!
		if (outer == null) { return false; }
		// If the proposed inner is immovable, then, uh, no.
		if (!inner.IsMovable) { return false; }
		// If they're both Portals of the same channel, DON'T allow it.
		if (AreOccupantsSameChannelPortals (inner, outer)) {
			return false;
		}
		return outer.CanOccupantEnter(side);
	}

	public static void ResetDidRotateThisStepForAllOccupants (Board b) {
		foreach (BoardOccupant bo in b.AllOccupants) {
			bo.ResetDidRotateThisStep ();
		}
	}



	/** corner: 0 top-left; 1 top-right; 2 bottom-right; 3 bottom-left. */
	public static bool DoesOccupantHaveMovableNeighborAtCorner (Board b, BoardOccupant bo, int corner) {
		Vector2Int cornerDir = GetCornerDir (corner);
//		return IsMovableOuterOccupant(b, bo.Col+cornerDir.x, bo.Row)
//			|| IsMovableOuterOccupant(b, bo.Col,			 bo.Row+cornerDir.y)
//			|| IsMovableOuterOccupant(b, bo.Col+cornerDir.x, bo.Row+cornerDir.y);
		return IsPlayerOuterOccupant(b, bo.Col+cornerDir.x, bo.Row)
			|| IsPlayerOuterOccupant(b, bo.Col,			 bo.Row+cornerDir.y)
			|| IsPlayerOuterOccupant(b, bo.Col+cornerDir.x, bo.Row+cornerDir.y);
	}
	public static Portal PortalWithWarpSideInSpace (Board b, int col,int row, int side) {
		BoardOccupant innermostBOFromOppositeDir = GetOutermostOccupantAtSideForBeam (b, col,row, GetOppositeSide(side));
		Portal portalHere = innermostBOFromOppositeDir as Portal;
//		if (innermostBOFromOppositeDir==null || !(innermostBOFromOppositeDir is Portal)) { return false; } // Nobody here, or it's not a Portal.
		if (portalHere == null) { return null; } // No Portal here.
		if (!portalHere.IsWarpSide (side)) { return null; } // Ah, it's a Portal, but it's not the warp side.
		return portalHere; // It's a Portal and the provided side is its warp side!
	}


	public static BoardSpace GetSpace(Board b, int col, int row) {
		if (col<0 || row<0  ||  col>=b.NumCols || row>=b.NumRows) return null;
		return b.Spaces[col,row];
	}
	public static BoardOccupant GetOccupant(Board b, int col,int row) {
		return GetOccupant (b, col,row, 0);
	}
	private static BoardOccupant GetOccupant(Board b, int col,int row, int layer) {
		BoardSpace space = GetSpace (b, col,row);
		if (space==null) { return null; }
		return space.OccupantAtLayer (layer);
	}
	private static OccupantContainer GetOutermostContainer (Board b, int col,int row) {
		return GetOccupant(b, col,row) as OccupantContainer;
	}
	public static Bucket GetBucket(Board b, int col,int row, int layer) {
		return GetOccupant(b, col,row, layer) as Bucket;
//		OccupantContainer outermostContainer = GetOutermostContainer(b, col,row);
//		if (outermostContainer==null) { return null; } // No outermost Container? Return null.
//		BoardOccupant correctLayerOccupant = outermostContainer.GetOccupantInsideMeAtLayer (bucketLayer);
//		if (correctLayerOccupant==null) { return null; } // No dude even at this layer. Return null!
//		return correctLayerOccupant as Bucket; // We got some Occupant at the right layer! Return it cast as a Bucket.
	}
	public static Pusher GetPusher(Board b, int col,int row) {
		BoardSpace space = GetSpace (b, col,row);
		if (space==null) { return null; }
		return space.MyPusher;
	}
	public static bool IsBeam(Board b, int col,int row) {
		BoardSpace space = GetSpace (b, col,row);
		if (space==null) { return false; }
		return space.NumBeamsOverMe() > 0;
	}
	public static bool IsOccupantPortalOfChannel(BoardOccupant bo, int channelID) {
		return bo is Portal && (bo as Portal).MyChannel.ChannelID == channelID;
	}
//	private static bool IsMovableOuterOccupant(Board b, int col,int row) {
//		BoardOccupant bo = GetOccupant(b, col,row);
//		return bo!=null && bo.IsMovable;
//	}
	private static bool IsPlayerOuterOccupant(Board b, int col,int row) {
		BoardOccupant bo = GetOccupant(b, col,row);
		return bo!=null && bo is Player;
	}
	private static bool IsImmovableOccupantWithNonEnterableSide(Board b, int col,int row, int side) {
		BoardOccupant bo = GetOccupant (b, col,row);
		if (bo==null || bo.IsMovable) { return false; } // Nobody here, OR a movable Occupant? Return false.
		if (bo is OccupantContainer && (bo as OccupantContainer).IsSideEnterable(side)) { return false; } // A Container is here, AND/BUT we can move through this side! Return false.
		return true; // Looks like there's an immovable Occupant or Container-with-a-side here.
	}
	private static bool IsImmovableOccupantWithNonExitableSide(Board b, int col,int row, int side) {
		BoardOccupant bo = GetOccupant (b, col,row);
		if (bo==null || bo.IsMovable) { return false; } // Nobody here, OR a movable Occupant? Return false.
		if (bo is OccupantContainer && (bo as OccupantContainer).IsSideExitable(side)) { return false; } // A Container is here, AND/BUT we can move through this side! Return false.
		return true; // Looks like there's an immovable Occupant or Container-with-a-side here.
	}

//	/** Will NOT return an Occupant if no Occupants in this space have this side. (Used to determine if we can pull a bucket. We can't pull a bucket if it doesn't have a side to grab.) */
//	public static BoardOccupant GetOutermostMovableOccupantAtSideWithSide (Board b, int col,int row, int side) {
//		BoardOccupant outermostOccupant = GetOutermostMovableOccupantAtSide (b, col,row, side);
//		if (outermostOccupant==null || !outermostOccupant.IsSide(side)) { return null; } // No guy, OR the one we found doesn't have the side we need? Return null.
//		return outermostOccupant; // We found a guy with the side we need! :)
//	}
	/** Returns the first guy it can find by looking inward from this side. Will ALWAYS return an Occupant if any are here. */
	public static BoardOccupant GetOutermostMovableOccupantAtSide (Board b, int col,int row, int side) {
		BoardOccupant outermostOccupant = GetOutermostOccupantAtSide (b, col,row, side);
		// There's nobody here, OR the outermost dude here isn't movable. Return null.
		if (outermostOccupant == null || !outermostOccupant.IsMovable) { return null; }// || bo.IsSidePull
		// The outermost dude is movable! Return it!
		return outermostOccupant;
	}
	private static BoardOccupant GetOutermostOccupantAtSide (Board b, int col,int row, int side) {
		BoardOccupant primaryOccupant = GetOccupant (b, col,row);
		return FindOutermostOccupantAtSideRecursively (primaryOccupant, side);
	}
	private static BoardOccupant FindOutermostOccupantAtSideRecursively (BoardOccupant bo, int side) {
		if (bo == null) { return null; } // There's NOBODY here?? Return null.
		// This guy is a Container with another Occupant inside it!!
		OccupantContainer container = bo as OccupantContainer;
		if (container!=null && container.OccupantInsideMe != null) {
			// We CAN access the inner occupant from outside this side!
			if (container.IsSideEnterable(side)) {
				// Look inside THIS fella!!
				return FindOutermostOccupantAtSideRecursively (container.OccupantInsideMe, side);
			}
		}
		// There's nobody inside this guy OR this guy has a side here (so anything in it can't exit in this direction). Return this guy!
		return bo;
	}
	private static BoardOccupant GetOutermostOccupantAtSideForBeam (Board b, int col,int row, int side) {
		BoardOccupant primaryOccupant = GetOccupant (b, col,row);
		return FindOutermostOccupantForBeamAtSideRecursively (primaryOccupant, side);
	}
	// TODO: This function is incomplete! It doesn't work yet! (It will return a usable value, but not the CORRECT one!)
	private static BoardOccupant FindOutermostOccupantForBeamAtSideRecursively (BoardOccupant bo, int side) {
		if (bo == null) { return null; } // There's NOBODY here?? Return null.
		// This guy is a Container with another Occupant inside it!!
		OccupantContainer container = bo as OccupantContainer;
		if (container!=null && container.OccupantInsideMe != null) {
			// We CAN access the inner occupant from outside this side!
			if (container.CanBeamEnter(side)) {
				// Look inside THIS fella!!
				return FindOutermostOccupantForBeamAtSideRecursively (container.OccupantInsideMe, side);
			}
		}
		// There's nobody inside this guy OR this guy has a side here (so anything in it can't exit in this direction). Return this guy!
		return bo;
	}
	public static BoardOccupant OutermostNonContainer (Board b, int col,int row) {
		int layer = 0;
		while (true) {
			BoardOccupant occupantHere = GetOccupant(b, col,row, layer);
			if (occupantHere == null) { return null; }
			if (occupantHere!=null && !(occupantHere is OccupantContainer)) { return occupantHere; }
			layer ++;
		}
	}

	public static bool CanBeamEnterSpace (BoardSpace bs, int sideEntering) {
		return bs!=null && bs.CanBeamEnter(sideEntering);
	}
	public static bool CanBeamExitSpace (BoardSpace bs, int sideExiting) {
		return bs!=null && bs.CanBeamExit(sideExiting);
	}
	public static Vector2Int GetDir (int side) {
		switch (side) {
			case 0: return new Vector2Int ( 0,-1);
			case 1: return new Vector2Int ( 1, 0);
			case 2: return new Vector2Int ( 0, 1);
			case 3: return new Vector2Int (-1, 0);
			default: throw new UnityException ("Whoa, " + side + " is not a valid side. Try 0, 1, 2, or 3.");
		}
	}
	public static int GetSide (Vector2Int dir) {
		if (dir.x== 0 && dir.y==-1) { return 0; }
		if (dir.x== 1 && dir.y== 0) { return 1; }
		if (dir.x== 0 && dir.y== 1) { return 2; }
		if (dir.x==-1 && dir.y== 0) { return 3; }
		return -1; // Whoops.
	}
	public static int GetOppositeSide (Vector2Int dir) { return GetOppositeSide(GetSide(dir)); }
	public static int GetOppositeSide (int side) {
		switch (side) {
			case 0: return 2;
			case 1: return 3;
			case 2: return 0;
			case 3: return 1;
			default: throw new UnityException ("Whoa, " + side + " is not a valid side. Try 0, 1, 2, or 3.");
		}
	}
	public static Vector2Int GetOppositeDir (int side) { return GetDir(GetOppositeSide(side)); }
	/** Useful for flipping dirEntering to dirExiting, for example. Just returns the original value * -1. */
	public static Vector2Int GetOppositeDir (Vector2Int dir) { return new Vector2Int(-dir.x, -dir.y); }
	/** corner: 0 top-left; 1 top-right; 2 bottom-right; 3 bottom-left. */
	private static Vector2Int GetCornerDir (int corner) {
		switch (corner) {
			case 0: return new Vector2Int (-1,-1);
			case 1: return new Vector2Int ( 1,-1);
			case 2: return new Vector2Int ( 1, 1);
			case 3: return new Vector2Int (-1, 1);
			default: throw new UnityException ("Whoa, " + corner + " is not a valid corner. Try 0, 1, 2, or 3.");
		}
	}
	public static Vector2Int GetInitializationMoveDirFromPortal (int originalPortalSideFacing, int newPortalSideFacing, Vector2Int moveDir) {
		if (moveDir == Vector2Int.zero) { return Vector2Int.zero; } // No moveDir (the case when the board is first made)? Return 0,0 dir.
//		int moveSideOriginal = GetSide (moveDir);
//		int portalSideFacingDiff = originalPortalSideFacing - newPortalSideFacing;
//		int moveSideRelative = GetOppositeSide(moveSideOriginal) + portalSideFacingDiff;
//		moveSideRelative = moveSideRelative % 4; // Keep it between 0 and 3.
//		if (moveSideRelative<0) { moveSideRelative += 4; }
//		return GetDir (moveSideRelative);
		return GetOppositeDir (newPortalSideFacing); // As long as Portals have 3 sides, this will always work.
	}

//	public static bool IsBeamOfChannelIDInList (List<Beam> beams, int channelID) {
//		foreach (Beam b in beams) {
//			if (b.ChannelID == channelID) { return true; }
//		}
//		return false;
//	}



}
