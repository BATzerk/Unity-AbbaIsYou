using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Currently, GenericTile is any Tile that ISN'T a TextBlock.
public class GenericTile : Tile {

    public bool IsOverlapGoalSatisfied() {
        foreach (Tile tile in MySpace.MyTiles) {
            if (tile!=this && tile.MyType!=TileType.TextBlock) {
                return true;
            }
        }
        return false;
    }

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public GenericTile (Board _boardRef, GenericTileData _data) {
		base.InitializeAsTile (_boardRef, _data);
    }
    
    // Serializing
	override public TileData ToData() {
        return new GenericTileData(MyGuid, MyType, BoardPos);
	}


}
