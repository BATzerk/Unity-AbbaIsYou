using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardObjectView : MonoBehaviour {
//	// Constants
//	private const float easingSpeed = 2f; // Higher is slower easing.
	// Properties
	private float _rotation; // so we can use the ease-ier (waka waka) system between -180 and 180 with minimal processing effort.
	private float rotation_to;
	private float rotation_from;
	private float baseScaleConstant; // so us with a scale of 1 fits us perfectly in a BoardSpace.
	private float _scale=1;
	protected float scale_to=1;
	protected float scale_from=1;
	private Vector2 pos_to; // visuals. Set right away; x and y ease to this.
	private Vector2 pos_from; // visuals. Set right away; x and y ease to this.
	// References
	private BoardObject myObject;
	private BoardObject mySimulatedMoveObject; // when we're lerping between two movement states, this is the "to" guy that's already completed its move!
	private BoardView myBoardView;

	// Getters
	public BoardView MyBoardView { get { return myBoardView; } }
	public BoardObject MyBoardObject { get { return myObject; } }

	private bool IsAtTargetPos () { return Vector2.Distance (Pos, pos_to) < 3f; }
	private bool IsAtTargetRotation () { return Mathf.Abs(Rotation-rotation_to) < 1f; }
	private bool IsAtTargetScale () { return Mathf.Abs(Scale-scale_to) < 0.05f; }
	public bool IsAnimating () { return !IsAtTargetPos() || !IsAtTargetRotation(); }

	public Vector2 Pos {
		get { return this.transform.localPosition; }
		set {
			this.transform.localPosition = value;
			OnSetPos ();
		}
	}
	/** In degrees. */
	public float Rotation {
		get { return _rotation; }
		set {
			_rotation = value;
			this.transform.localEulerAngles = new Vector3 (this.transform.localEulerAngles.x, this.transform.localEulerAngles.y, _rotation);
			OnSetRotation ();
		}
	}
	public float Scale {
		get { return _scale; }
		set {
			_scale = value;
			this.transform.localScale = Vector2.one * _scale * myBoardView.BaseObjectScaleConstant;
			OnSetScale ();
		}
	}
	static public float GetScaleFromOccupantLayer (int _layer) {
		return Mathf.Pow (0.7f, _layer);
	}
	private Vector2 GetPosFromBO (BoardObject _bo) {
		return new Vector2 (myBoardView.BoardToX (_bo.Col), myBoardView.BoardToY (_bo.Row));
	}
	private float GetRotationFromBO (BoardObject _bo) {
		float returnValue = -90 * _bo.SideFacing;
		if (returnValue<-180) returnValue += 360;
		if (returnValue> 180) returnValue -= 360;
		if (Mathf.Abs (returnValue-Rotation) > 180) {
			if (Rotation<returnValue) { Rotation += 360; }
			else { Rotation -= 360; }
		}
		return returnValue;
	}
	private float GetScaleFromBO (BoardObject _bo) {
		return GetScaleFromOccupantLayer (_bo.Layer);
	}

	virtual protected void OnSetPos () { }
	virtual protected void OnSetRotation () { }
	virtual protected void OnSetScale () { }


	// ----------------------------------------------------------------
	//  Initialize / Destroy
	// ----------------------------------------------------------------
	protected void InitializeAsBoardObjectView (BoardView _myBoardView, BoardObject _myObject) {
		myBoardView = _myBoardView;
		myObject = _myObject;

		// Parent me!
		this.transform.SetParent (myBoardView.transform);
		this.transform.localScale = Vector3.one;

		// Start me in the right spot!
		SetValues_From (_myObject);
		SetValues_To (_myObject); // For safety, default my "to" values to where I already am.
		GoToValues (myBoardView.ObjectsAnimationLocTarget);
	}
	private void DestroySelf () {
		myBoardView.OnObjectViewDestroyedSelf (this);
		// Plan my destruction when I'm done animating, yo!
//		doDestroySelf
		// Legit destroy me, yo!
		GameObject.Destroy (this.gameObject);
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	/** The moment after each move is logically executed, this is called for ALL BoardObjects. 
		This function will update target Pos/Rotation/Scale for Occupants, plus any extra stuff any extensions want to do too (like Player updating its pull arrows). */
	virtual public void UpdateVisualsPreMove () {
		SetValues_To (myObject);
	}
//	/** This is called once all the animation is finished! */
	virtual public void UpdateVisualsPostMove () {
		SetValues_From (myObject);
		GoToValues (myBoardView.ObjectsAnimationLocTarget); // go 100% to the target values, of course! (This could be either 1 *or* 0.)
		if (!myObject.IsInPlay) {
			DestroySelf ();
		}
	}
	public void SetValues_To (BoardObject _bo) {
		if (_bo == null) { return; }
		pos_to = GetPosFromBO (_bo);
		rotation_to = GetRotationFromBO (_bo);
		scale_to = GetScaleFromBO (_bo);
//		myBoardView.OnObjectStartAnimating (); // yes, we know at least *I* am animating!
	}
	private void SetValues_From (BoardObject _bo) {
		if (_bo == null) { return; }
		pos_from = GetPosFromBO (_bo);
		rotation_from = GetRotationFromBO (_bo);
		scale_from = GetScaleFromBO (_bo);
	}
//	public void GoToValues_To () {
//		UpdateVisualsBetweenModels (1);
//	}
//	public void GoToValues_From () {
//		UpdateVisualsBetweenModels (0);
//	}
//	virtual protected void GoToTargetPos () {
//		Pos = pos_to;
//	}
//	virtual protected void GoToTargetRotation () {
//		Rotation = rotation_to;
//	}
//	private void GoToTargetScale () {
//		Scale = scale_to;
//	}

//	public void ClearMySimulatedMoveObject () {
//		mySimulatedMoveObject = null;
//		SetValues_To (myObject); // Go back to my original guy.
//	}
	public void SetMySimulatedMoveObject (BoardObject _object) {
		mySimulatedMoveObject = _object;
	}
	public void SetValues_To_ByMySimulatedMoveBoardObject () {
		SetValues_To (mySimulatedMoveObject);
	}
	public void SetValues_From_ByCurrentValues () {
		pos_from = Pos;
		rotation_from = Rotation;
		scale_from = Scale;
//		SetValues_From (myObject);
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	/*
	public void OnSetColRow () {
		posTarget = new Vector2 (myBoardView.BoardToX (Col), myBoardView.BoardToY (Row));
		myBoardView.OnObjectStartAnimating (); // yes, we know at least *I* am animating!
	}
	public void OnSetSideFacing () {
		rotationTarget = -90 * SideFacing;
		if (rotationTarget<-180) rotationTarget += 360;
		if (rotationTarget> 180) rotationTarget -= 360;
		if (Mathf.Abs (rotationTarget-Rotation) > 180) {
			if (Rotation<rotationTarget) { Rotation += 360; }
			else { Rotation -= 360; }
		}
	}
	public void OnSetLayer () {
		UpdateScaleFromLayer ();
	}
	*/
	public void OnGoToPrevBoardPos () {
		// No animating in an undo. It's simply more professional.
		GoToValues (myBoardView.ObjectsAnimationLocTarget);
	}

	public void GoToValues (float lerpLoc) {
//		if (mySimulatedMoveObject == null) { return; }
		Pos = Vector2.Lerp (pos_from, pos_to, lerpLoc);
		Rotation = Mathf.Lerp (rotation_from, rotation_to, lerpLoc);
		Scale = Mathf.Lerp (scale_from, scale_to, lerpLoc);
	}


	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	virtual protected void FixedUpdate () {
//		EaseToTargetValues ();
	}
//	private void EaseToTargetValues () {
//		if (!IsAtTargetRotation ()) {
//			Rotation += (rotation_to-Rotation) / easingSpeed;
//			if (IsAtTargetRotation ()) {
//				OnFinishedAnimating_Rotation ();
//			}
//		}
//		if (!IsAtTargetPos ()) {
//			Pos += (pos_to-Pos) / easingSpeed;
//			if (IsAtTargetPos ()) {
//				OnFinishedAnimating_Pos ();
//			}
//		}
//		if (!IsAtTargetScale ()) {
//			Scale += (scale_to-Scale) / easingSpeed;
//			if (IsAtTargetScale ()) {
//				OnFinishedAnimating_Scale ();
//			}
//		}
//	}
//	private void OnFinishedAnimating_Pos () {
//		GoToValues_To ();
//		myBoardView.OnObjectFinishedMovingAnimation (); // yes! please do check if we're all done!
//	}
//	private void OnFinishedAnimating_Rotation () {
//		GoToValues_To ();
//		myBoardView.OnObjectFinishedMovingAnimation (); // yes! please do check if we're all done!
//	}
//	private void OnFinishedAnimating_Scale () {
//		GoToValues_To (); // Note that we don't have to tell the Board: Scale ever changing is totally dependent on movement within the board.
////		GoToTargetScale (); // Note that we don't have to tell the Board: Scale ever changing is totally dependent on movement within the board.
//	}




}
