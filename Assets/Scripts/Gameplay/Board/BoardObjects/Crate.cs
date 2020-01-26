﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : Tile {
    // Properties
    public bool DoAutoMove { get; private set; } // NOTE: NOT IMPLEMENTED!
    public Vector2Int AutoMoveDir { get; private set; }
    
    // Getters
    private BoardSpace GetSpaceAutoMoveTo() {
        TranslationInfo ti = BoardUtils.GetTranslationInfo(BoardRef, ColRow, MathUtils.GetSide(AutoMoveDir));
        return GetSpace(ti.to);
    }
    

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public Crate (Board _boardRef, CrateData _data) {
		base.InitializeAsTile (_boardRef, _data);
        DoAutoMove = _data.doAutoMove;
        AutoMoveDir = _data.autoMoveDir;
    }
    
    // Serializing
	override public TileData ToData() {
        return new CrateData(BoardPos, DoAutoMove, AutoMoveDir);
	}


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    override public void SetColRow(Vector2Int _colRow, Vector2Int _moveDir) {
        base.SetColRow(_colRow, _moveDir);
        AutoMoveDir = _moveDir;
    }
    public override void OnPlayerMoved() {
        base.OnPlayerMoved();
        //// I auto-move, I HAVE a dir to auto-move, AND I DIDN'T just move?...
        //if (DoAutoMove && AutoMoveDir!=Vector2Int.zero && PrevMoveDelta==Vector2Int.zero) {
        //    BoardSpace spaceTo = GetSpaceAutoMoveTo();
        //    bool doMove = !spaceTo.HasOccupant;
        //    doMove &= BoardUtils.MayMoveTile(BoardRef, ColRow, AutoMoveDir);
        //    // We CAN move. Do!
        //    if (doMove) {
        //        BoardUtils.MoveOccupant(BoardRef, ColRow, AutoMoveDir);
        //    }
        //    // We CANNOT move. Zero-out AutoMoveDir.
        //    else {
        //        AutoMoveDir = Vector2Int.zero;
        //    }
        //}
    }




}
