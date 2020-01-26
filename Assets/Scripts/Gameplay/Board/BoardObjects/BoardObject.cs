using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardObject {
	// Properties
	private bool didRotateThisStep = false; // reset at the end of every step. So we don't rotate 180 from Player AND Crate passing by us in one step. That doesn't feel right.
	private bool isInPlay = true; // set to false when RemoveFromPlay is called. Set to true in RemoveFromPlay, where we also tell my Board to remove me from its lists. I'm outta here. If true, our matching view will also destroy itself.
	private BoardPos boardPos; // col, row, and sideFacing!
	// References
	protected Board boardRef;

	// Getters
	virtual public BoardPos BoardPos { get { return boardPos; } }
	virtual public int Col { get { return boardPos.col; } }
	virtual public int Row { get { return boardPos.row; } }
	public int Layer { get { return boardPos.layer; } }
	public int SideFacing { get { return boardPos.sideFacing; } }
	public Board BoardRef { get { return boardRef; } }
	protected BoardSpace GetSpace (int _col,int _row) { return BoardUtils.GetSpace (boardRef, _col,_row); }
	public BoardSpace MySpace { get { return GetSpace (Col,Row); } }
	public bool DidRotateThisStep { get { return didRotateThisStep; } }
	public bool IsInPlay { get { return isInPlay; } }

	virtual public void SetLayer (int _layer) {
		boardPos = new BoardPos(Col,Row, SideFacing, _layer);
	}
	/** Call this when we get kicked out of a Container, and want to "awaken" our actual BoardPos (as we were just parroting it until now). */
	protected void RefreshBoardPos () {
		boardPos = BoardPos;
	}

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	protected void InitializeAsBoardObject (Board _boardRef, BoardPos _boardPos) {//BoardObjectData _data
		boardRef = _boardRef;
		boardPos = _boardPos;

		// Call my "official" set-property functions so the accompanying stuff happens too.
		SetColRow (boardPos.col,boardPos.row);
		SetSideFacing (boardPos.sideFacing);
		SetLayer (boardPos.layer);
		// NOTE: Don't automatically add me to the Board. We need the flexibility of having Occupants starting out in limbo.
//		AddMyFootprint (false); // add me to the board! And default me being inside whatever's already here.
	}

	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void ResetDidRotateThisStep () { didRotateThisStep = false; }

	virtual public void SetColRow (int _col, int _row) {
		boardPos.col = _col;
		boardPos.row = _row;
		OnSetColRow ();
	}
	public void SetSideFacing (int _sideFacing) {
		boardPos.sideFacing = _sideFacing;
		OnSetSideFacing ();
	}
	virtual protected void OnSetColRow () { }
	virtual protected void OnSetSideFacing () { }

	virtual public void RotateMe (int dir) {
		// Just update sideFacing! My extensions will do the rest.
		SetSideFacing (SideFacing+dir);
		didRotateThisStep = true;
	}

//	// Override these!
//	virtual public void AddMyFootprint () { }
//	virtual public void RemoveMyFootprint () { }

	/** This removes me from the Board completely and permanently. */
	protected void RemoveFromPlay () {
		// I'm donezo.
		isInPlay = false;
		// Tell my boardRef I'm toast!
		boardRef.OnObjectRemovedFromPlay (this);
	}



}
