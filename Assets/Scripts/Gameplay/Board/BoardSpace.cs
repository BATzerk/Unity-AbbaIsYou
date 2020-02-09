using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpace {
	// Properties
	public Vector2Int ColRow { get; private set; }
	private bool isPlayable = true;
    private bool isWallL, isWallT; // walls can only be on the LEFT and TOP of spaces.
    // References
    public List<Tile> MyTiles { get; private set; } // all the tiles on me.

    // Getters
    public bool IsPlayable { get { return isPlayable; } }
    public int Col { get { return ColRow.x; } }
    public int Row { get { return ColRow.y; } }
    //public bool HasExitSpot() {
    //    foreach (Tile tile in MyTiles) { if (tile.MyType == TileType.ExitSpot) { return true; } }
    //    return false;
    //}
    public bool HasPushTile() {
        foreach (Tile tile in MyTiles) { if (tile.IsPush) { return true; } }
        return false;
    }
    public bool HasStopTile() {
        foreach (Tile tile in MyTiles) { if (tile.IsStop) { return true; } }
        return false;
    }
    //public bool HasImmovableOccupant { get { return MyOccupant!=null && !MyOccupant.IsMovable; } }
    public bool IsWall(int side) {
        switch(side) {
            case Sides.L: return isWallL;
            case Sides.T: return isWallT;
            default: return false;
        }
    }
    public bool MayTileEverExit(Vector2Int dirOut) {
        int side = MathUtils.GetSide(dirOut);
        if (IsWall(side)) { return false; }
        return true;
    }
    public bool MayTileEverEnter(Vector2Int dirIn) {
        int side = MathUtils.GetSide(dirIn);
        return MayTileEverEnter(side);
    }
    /** Side: Relative to ME. */
    private bool MayTileEverEnter(int side) {
        if (!IsPlayable) { return false; }
        if (HasStopTile()) { return false; }
        if (IsWall(side)) { return false; }
        return true;
    }
    
    
	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public BoardSpace (BoardSpaceData _data) {
        ColRow = _data.ColRow;
        isPlayable = _data.isPlayable;
        isWallL = _data.isWallL;
        isWallT = _data.isWallT;
        MyTiles = new List<Tile>();
	}
	public BoardSpaceData ToData () {
        BoardSpaceData data = new BoardSpaceData(Col, Row) {
            isPlayable = isPlayable,
            isWallL = isWallL,
            isWallT = isWallT,
        };
        return data;
	}
    
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    public void AddWall(int side) {
        switch(side) {
            case Sides.L: isWallL = true; break;
            case Sides.T: isWallT = true; break;
            default: Debug.LogError("Whoa, we're calling AddWall for a side that's NOT Top or Left: " + side); break;
        }
    }
    
    //public void SetMyExitSpot(ExitSpot bo) {
    //    if (MyExitSpot != null) {
    //        throw new UnityException ("Oops! Trying to set a Space's MyExitSpot, but that Space already has an ExitSpot! " + Col + ", " + Row);
    //    }
    //    MyExitSpot = bo;
    //}
    
	public void AddTile (Tile _bo) {
		if (MyTiles.Contains(_bo)) {
			throw new UnityException ("Oops! Trying to add a Tile to a Space, but that Space already HAS that Tile: " + _bo.GetType() + ". " + Col + ", " + Row);
		}
        MyTiles.Add(_bo);
	}
	public void RemoveTile (Tile _bo) {
        MyTiles.Remove(_bo);
	}



}
