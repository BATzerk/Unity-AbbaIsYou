using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : BoardOccupantView {
	// Components
	[SerializeField] private GameObject go_body;
	[SerializeField] private GameObject go_pullArrows;
	[SerializeField] private ParticleSystem ps_blowUp;
	[SerializeField] private SpriteRenderer[] srs_pullArrows; // these SpriteRenderer Sprites are set dynamically.
	// References
	private Player myPlayer;
	private BoardOccupant[] occupantsVisuallyGrabbing; // this will ALWAYS have 4 slots, with values typically null. Index = side.
	[SerializeField] private Sprite s_pullArrow_behind;
	[SerializeField] private Sprite s_pullArrow_alongside;

	// Getters
	override protected Color GetPrimaryFillMovable () { return new Color(0.6f,0.6f,0.6f); }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize (BoardView _myBoardView, Player _myPlayer) {
		base.InitializeAsBoardOccupantView (_myBoardView, _myPlayer);
		myPlayer = _myPlayer;

		occupantsVisuallyGrabbing = new BoardOccupant[4];
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	override public void UpdateVisualsPreMove () {
		base.UpdateVisualsPreMove ();
		ClearOccupantsVisuallyGrabbing ();
		// Whoa, we're blowing up here??
		if (myPlayer.IsDead) {
			OnBlowUp (myPlayer.BeamThatKilledMe);
		}
	}
	override public void UpdateVisualsPostMove () {
		base.UpdateVisualsPostMove ();
		UpdateOccupantsVisuallyGrabbing ();
	}

	private void ClearOccupantsVisuallyGrabbing () {
		for (int i=0; i<occupantsVisuallyGrabbing.Length; i++) {
			occupantsVisuallyGrabbing[i] = null;
		}
		UpdatePullArrows ();
	}
	public void UpdateOccupantsVisuallyGrabbing () {
		AssignOccupantVisuallyGrabbing (0);
		AssignOccupantVisuallyGrabbing (1);
		AssignOccupantVisuallyGrabbing (2);
		AssignOccupantVisuallyGrabbing (3);
		UpdatePullArrows ();
	}
	private void AssignOccupantVisuallyGrabbing (int side) {
		occupantsVisuallyGrabbing[side] = BoardUtils.OccupantPlayerCanPullAtSide (MyBoardView.MyBoard, myPlayer, side, MovementTypes.Any);// BoardUtils.GetDir(index), index, MovementTypes.Any);//GetMovableOrRotatableOccupant
	}

	private void UpdatePullArrows () {
		// Offset the rotation and scale of the whole GO so my rotation doesn't affect it.
		go_pullArrows.transform.localEulerAngles = new Vector3 (0, 0, -Rotation);
		go_pullArrows.transform.localScale = new Vector3 (1/Scale,1/Scale,1/Scale);
		for (int i=0; i<srs_pullArrows.Length; i++) {
			UpdatePullArrow (i);
		}
	}
	private void UpdatePullArrow (int index) {
		SpriteRenderer sr_arrow = srs_pullArrows[index];
		BoardOccupant occupantGrabbing = GetGrabbableOccupant(index);
		sr_arrow.enabled = occupantGrabbing!=null && !myPlayer.IsDead;
		if (occupantGrabbing != null) {
			// Set the sprite!
			sr_arrow.sprite = occupantGrabbing.IsSidePull ? s_pullArrow_alongside : s_pullArrow_behind;
			// Pos and rotation!
			float radians = index * Mathf.PI*0.5f;
			Vector2 posOffset = GameMathUtils.GetVectorFromAngleRad (radians) * 0.64f; // NOTE: Hardcoded offset.
			sr_arrow.transform.localPosition = new Vector2 (posOffset.x, posOffset.y);
			sr_arrow.transform.localEulerAngles = new Vector3 (0, 0, -radians*Mathf.Rad2Deg+180); // note: idk what's up with this angle, but it's nbd lmao!
		}

		/*
		SpriteLine line = sl_grabbingLines[index];

		line.IsVisible = occupant != null;
		if (occupant != null) {
			Vector2 endPos = BoardUtils.GetDirFromSide(index).ToVector2() * BoardRef.UnitSize; // end pos is actually where the board space is, not where the occupant is.
			endPos = new Vector2 (endPos.x, -endPos.y); // flip y to match board space.
			line.SetStartAndEndPos (Vector2.zero, endPos);
		}
		*/
	}
	private BoardOccupant GetGrabbableOccupant (int side) {
		BoardOccupant occupant = occupantsVisuallyGrabbing[side];
		if (occupant == null) { return null; } // No Occupant? Simply return null.
		if (!myPlayer.MySpace.CanOccupantEverEnterMe(side)) { return null; }
//		  || !occupant.MySpace.CanOccupantEverExitMe(dirExiting)) { return null; }
		return occupant; // Looks like this Occupant can go from where it is to where I am! Return it!
	}



	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnBlowUp (Beam beamThatKilledMe) {
		StartCoroutine (BlowUpWithDelayCoroutine(beamThatKilledMe));
	}
	// This is a quick hack-in to get a blow-up delay. Fine for our purposes now.
	private IEnumerator BlowUpWithDelayCoroutine (Beam beamThatKilledMe) {
		yield return new WaitForSeconds (0.1f);

		// Particle burst!
		Color beamColor = Colors.GetBeamColor (MyBoardView.WorldIndex, beamThatKilledMe.ChannelID);
		GameUtils.SetParticleSystemColor (ps_blowUp, beamColor);
		ps_blowUp.Emit (12);
		// Hide my body!
		go_body.SetActive (false);
	}
//	public void OnSetIsDead () {
//		go_body.SetActive (!myPlayer.IsDead);
//		if (!myPlayer.IsDead) { // If I've been brought back to life, nix all active particles!
//			ps_blowUp.Clear ();
//		}
//	}

}
