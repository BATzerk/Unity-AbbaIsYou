using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardOccupantView : BoardObjectView {
	// Properties
	virtual protected Color GetPrimaryFillMovable () { return new Color (1,1,1); } // Override this if we want to auto-color our body (without adding code)!
	virtual protected Color GetPrimaryFillImmovable () {
		ColorHSB colorMovableHSB = new ColorHSB(GetPrimaryFillMovable ()); // Default us to a darker version of the movable color!
		return new ColorHSB(colorMovableHSB.h,colorMovableHSB.s*1.3f,colorMovableHSB.b*0.5f, colorMovableHSB.a).ToColor();
	}
	protected float beamSegmentRendererOriginOffsetLoc = 0.5f; // extensions of this class will modify this when they're made.
	private SpriteMaskInteraction allSpriteRenderersMaskInteraction = SpriteMaskInteraction.None;
	// Components
	[SerializeField] private SpriteRenderer sr_body; // everyone has a primary body sprite for simplicity! Less code.
	private BeamRendererCollider beamRendererCollider;
	private GameObject go_sidePullOverlay; // will add if we are Side-Pull!
	private PassRotatableArcs passRotatableArcs; // this will be made (from a prefab) if we're pass-rotatable!
	// References
	protected BoardOccupant myOccupant; // a direct reference to my model. Doesn't change.
	[SerializeField] private Sprite s_bodyMovable;
	[SerializeField] private Sprite s_bodyImmovable;

	// Getters
	public float BeamSegmentRendererOriginOffsetLoc { get { return beamSegmentRendererOriginOffsetLoc; } }
//	protected Board MyBoard { get { return MyBoardView.MyBoard; } }


	// DEBUG
	private void OnDrawGizmos () {
		if (beamRendererCollider == null) { return; }
		Gizmos.color = Color.magenta;
		foreach (BeamRendererColliderLine line in beamRendererCollider.Debug_colliderLines) {
			Gizmos.DrawLine (line.line.start*GameVisualProperties.WORLD_SCALE, line.line.end*GameVisualProperties.WORLD_SCALE);
		}
	}

	// ----------------------------------------------------------------
	//  Initialize / Destroy
	// ----------------------------------------------------------------
	protected void InitializeAsBoardOccupantView (BoardView _myBoardView, BoardOccupant _myOccupant) {
		base.InitializeAsBoardObjectView (_myBoardView, _myOccupant);
		myOccupant = _myOccupant;

		ApplyFundamentalVisualProperties ();
		// Make my beamRendererCollider!
		beamRendererCollider = new BeamRendererCollider (MyBoardView, this);

		// If we've been initialized in a Portal, animate like that!
		OffsetPositionForInitializationMoveDir ();
		UpdateSpriteMaskInteraction (); // In case I'm being initialized in a Portal, make sure I'm masked for this first move!
	}
	/** If we initialized because of a Portal (which we know if our Occupant has InitializationMoveDir), offset our initial position so we animate into place! */
	private void OffsetPositionForInitializationMoveDir () {
		Vector2Int initMoveDir = myOccupant.InitializationMoveDir;
		if (initMoveDir != Vector2Int.zero) {
			Pos += new Vector2 (initMoveDir.x*MyBoardView.UnitSize, -initMoveDir.y*MyBoardView.UnitSize); // don't forget to flip Y!
		}
	}
	virtual protected void OnDestroy () {
		// Make sure to destroy my Collider! It's outta the BoardView now. :)
		if (beamRendererCollider != null) {
			beamRendererCollider.Destroy ();
		}
	}

	virtual protected void ApplyFundamentalVisualProperties () { // Note: We can make this overridable if we want unique visuals per object.
		// If we DO have a bodySprite!...
		if (sr_body != null) {
			// Color me impressed!
			if (myOccupant.IsMovable) {
				sr_body.sprite = s_bodyMovable;
				sr_body.color = GetPrimaryFillMovable ();
			}
			else {
				sr_body.sprite = s_bodyImmovable;
				sr_body.color = GetPrimaryFillImmovable ();
			}
		}
		// Pass-rotatable??
		if (myOccupant.IsPassRotatable) { AddPassRotatableArcs (); }
		else { RemovePassRotatableArcs (); }
		// Side-Pull?
		if (myOccupant.IsSidePull) {
			AddSidePullOverlay ();
		}
		else { RemoveSidePullOverlay (); }
	}

	private void AddPassRotatableArcs () {
		if (passRotatableArcs == null) { // if it doesn't yet exist...
			GameObject prefabGO = Resources.Load<GameObject>(FilePaths.BOARD_OBJECTS+"PassRotatableArcs");
			passRotatableArcs = Instantiate (prefabGO).GetComponent<PassRotatableArcs>();
			passRotatableArcs.Initialize (this);
		}
	}
	private void RemovePassRotatableArcs () {
		if (passRotatableArcs != null) {
			Destroy (passRotatableArcs.gameObject);
			passRotatableArcs = null;
		}
	}
	private void AddSidePullOverlay () {
		if (go_sidePullOverlay == null) { // if it doesn't yet exist...
			GameObject prefabGO = Resources.Load<GameObject>(FilePaths.BOARD_OBJECTS+"SidePullOccupantOverlay");
			go_sidePullOverlay = Instantiate (prefabGO);
			go_sidePullOverlay.transform.SetParent (this.transform);
			go_sidePullOverlay.transform.localPosition = Vector3.zero;
			go_sidePullOverlay.transform.localScale = Vector3.one;
		}
	}
	private void RemoveSidePullOverlay () {
		if (go_sidePullOverlay != null) {
			Destroy (go_sidePullOverlay);
			go_sidePullOverlay = null;
		}
	}

	// Events
	private void UpdateSpriteMaskInteraction () {
		Portal firstPortalOutsideMe = myOccupant.FirstPortalOutsideMe();
		// This Occupant is somewhere in a Portal!! We're gonna want to get masked by the Portal. :)
		if (firstPortalOutsideMe != null) {
			SetAllSpriteRenderersMaskInteraction (SpriteMaskInteraction.VisibleInsideMask);
		}
		// NOT a Portal. No mask necessary!
		else {
			SetAllSpriteRenderersMaskInteraction (SpriteMaskInteraction.None);
		}
		// todo: #future Layer Portal masking for different channels... eventually. :)
	}
	private void SetAllSpriteRenderersMaskInteraction (SpriteMaskInteraction interactionType) {
		// If this type is different, then update the type and all my SpriteRenderers!
		if (allSpriteRenderersMaskInteraction != interactionType) {
			allSpriteRenderersMaskInteraction = interactionType;
			SpriteRenderer[] allSRs = GetComponentsInChildren<SpriteRenderer>();
			foreach (SpriteRenderer sr in allSRs) {
				sr.maskInteraction = allSpriteRenderersMaskInteraction;
			}
		}
	}

	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
//	override public void UpdateVisualsPreMove () {
//		base.UpdateVisualsPreMove ();
//	}
	override public void UpdateVisualsPostMove () {
		base.UpdateVisualsPostMove ();
		UpdateVisuals_RotatableArcs ();
		UpdateSpriteMaskInteraction ();
	}
//	private void UpdateVisuals_SidePullOverlay (BoardPos playerPos) {
//		if (sr_pullArrows != null) {
//			//			bool doShow
//			bool isHorz = Mathf.Abs(playerPos.col-Col) > Mathf.Abs(playerPos.row-Row);
//			float arrowsRotation = isHorz ? 0 : 90;
//			sr_pullArrows.transform.localEulerAngles = new Vector3 (sr_pullArrows.transform.localEulerAngles.x,sr_pullArrows.transform.localEulerAngles.y,arrowsRotation);
//		}
//	}
	private void UpdateVisuals_RotatableArcs () {
		if (myOccupant.IsPassRotatable) {
			passRotatableArcs.UpdateArcVisuals (MyBoardView.MyBoard, myOccupant);
		}
	}


	override protected void OnSetPos () {
		base.OnSetPos ();
		if (beamRendererCollider!=null) { beamRendererCollider.UpdateLines (); }
	}
	override protected void OnSetRotation () {
		base.OnSetRotation ();
		if (beamRendererCollider!=null) { beamRendererCollider.UpdateLines (); }
		if (passRotatableArcs!=null) { passRotatableArcs.OffsetRotation (Rotation); }
	}

	/*
	public void OnSetOccupantOutsideMe () {
		// Update my scale (and the scale of the Occupants inside me)!
		int total = myOccupant.GetTotalOccupantsOutsideMe ();
		UpdateScaleFromNumOccupantsOutsideMe (total);
	}
	public void UpdateScaleFromNumOccupantsOutsideMe (int total) {
		// Update scale!
		scaleTarget = GetScaleFromOccupantLayer (total);
//		scaleTarget = Vector2.one * (1 - 0.26f*total);
		// Trickle it down to all the dudes inside me!
		if (myOccupant.OccupantInsideMe != null) {
			(myOccupant.OccupantInsideMe.Body as BoardOccupantBody).UpdateScaleFromNumOccupantsOutsideMe (total+1);
		}
	}
	//*/



	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	override protected void FixedUpdate () {
		base.FixedUpdate ();
		// If I'm moving at all, update my BeamRendererCollider!
		if (MyBoardView.AreObjectsAnimating && IsAnimating()) {
			beamRendererCollider.UpdateLines ();
		}
	}

}
