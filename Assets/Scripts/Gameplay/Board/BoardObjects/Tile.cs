using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public class Tile {
    // Properties
    public BoardPos BoardPos { get; private set; }
    public bool IsPush { get; private set; }
    public bool IsStop { get; private set; }
    public bool IsYou { get; private set; }
    private bool isInPlay = true; // we set this to false when I'm removed from the Board!
    public Vector2Int PrevMoveDelta { get; private set; } // how far I moved the last move.
    // References
    public Board BoardRef { get; private set; }


    // Getters
	public bool IsInPlay { get { return isInPlay; } }
    public Vector2Int ColRow { get { return BoardPos.ColRow; } }
    public int Col { get { return BoardPos.ColRow.x; } }
    public int Row { get { return BoardPos.ColRow.y; } }
    public int SideFacing { get { return BoardPos.SideFacing; } }
    protected BoardSpace GetSpace (Vector2Int _colRow) { return BoardUtils.GetSpace (BoardRef, _colRow); }
    protected BoardSpace GetSpace (int _col,int _row) { return BoardUtils.GetSpace (BoardRef, _col,_row); }
	public BoardSpace MySpace { get { return GetSpace (Col,Row); } }
    public bool IsOrientationMatch(Tile other) {
        return BoardPos == other.BoardPos;
    }
    
    // Serializing
    abstract public TileData ToData();
    
    
	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	protected void InitializeAsTile (Board _boardRef, TileData data) {
		this.BoardRef = _boardRef;
		this.BoardPos = data.boardPos;
        this.IsPush = data.isPush;
        this.IsStop = data.isStop;
        this.IsYou = data.isYou;
        
		// Automatically add me to the board!
		AddMyFootprint ();
	}
    
    
	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
    virtual public void SetColRow(Vector2Int _colRow, Vector2Int _moveDir) {
        //RemoveMyFootprint();
        PrevMoveDelta = _moveDir;
        BoardPos = new BoardPos(_colRow, SideFacing);
        //AddMyFootprint();
	}
    private void SetSideFacing(int _sideFacing) {
        BoardPos = new BoardPos(ColRow, _sideFacing);
        //BoardPos.SideFacing = sideFacing;
        OnSetSideFacing();
    }
    public void ChangeSideFacing(int delta) { SetSideFacing(SideFacing+delta); }
    
    public void ResetPrevMoveDelta() {
        PrevMoveDelta = Vector2Int.zero;
    }

	/** This removes me from the Board completely and permanently. */
	public void RemoveFromPlay () {
		// Gemme outta here!
		isInPlay = false;
		RemoveMyFootprint();
		// Tell my boardRef I'm toast!
		BoardRef.OnObjectRemovedFromPlay (this);
	}
    
    public void AddMyFootprint () {
        MySpace.AddTile (this);
    }
    public void RemoveMyFootprint () {
        MySpace.RemoveTile (this);
    }

	// Override these!
    virtual public void OnPlayerMoved () {}
    virtual protected void OnSetSideFacing () {}


}
