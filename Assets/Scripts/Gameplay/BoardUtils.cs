using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveResults { Undefined, Success, Fail }

public static class BoardUtils {
    // ----------------------------------------------------------------
    //  Translation Info
    // ----------------------------------------------------------------
    public static TranslationInfo GetTranslationInfo(Board b, Vector2Int from, int side) { return GetTranslationInfo(b,from,MathUtils.GetDir(side)); }
    public static TranslationInfo GetTranslationInfo(Board b, Vector2Int from, Vector2Int dir) {
        return new TranslationInfo {
            from = from,
            to = from + dir,
            dirOut = dir,
            dirIn = Vector2Int.Opposite(dir),
        };
    }
    
    
    
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

    public static BoardSpace GetSpace(Board b, Vector2Int pos) { return GetSpace(b, pos.x,pos.y); }
    public static BoardSpace GetSpace(Board b, int col,int row) {
        if (col<0 || row<0  ||  col>=b.NumCols || row>=b.NumRows) { return null; } // Outta bounds? Return NULL.
		return b.Spaces[col,row]; // In bounds! Return Space!
	}
    public static List<Tile> GetTiles(Board b, Vector2Int pos) { return GetTiles(b, pos.x,pos.y); }
	public static List<Tile> GetTiles(Board b, int col,int row) {
		BoardSpace space = GetSpace (b, col,row);
		if (space==null) { return null; }
		return space.MyTiles;
	}
	public static bool IsSpacePlayable(Board b, int col,int row) {
		BoardSpace bs = GetSpace (b, col,row);
		return bs!=null && bs.IsPlayable;
	}
    
    
    // ----------------------------------------------------------------
    //  Making Moves!
    // ----------------------------------------------------------------
    /// This will FAIL if ANY player-controlled-Tile can't do this move.
    public static bool MayExecuteMove(Board b, Vector2Int dir) {
        if (b==null) { return false; } // Safety check.
        // Clone the Board!
        Board boardClone = b.Clone();
        // Execute the move, and return the result!
        return ExecuteMove(boardClone, dir) == MoveResults.Success;
    }
    /// This will FAIL if ANY player-controlled-Tile can't do this move.
    public static MoveResults ExecuteMove(Board b, Vector2Int dir) {
        // No dir?? Do nothing; return success!
        if (dir == Vector2Int.zero) { return MoveResults.Success; }
        
        // Refresh all rules first.
        b.RefreshAndApplyTextRules();
        
        // Get list of all things that're considered a "player".
        List<Tile> players = b.GetPlayers();
        if (players.Count == 0) { return MoveResults.Fail; } // No players?? Return FAIL.
        //if (AreGoalsSatisfied) { return false; } // Already won? Return FAIL.
        
        // Move 'em all, and return is ANY succeeded!
        MoveResults finalResult = MoveResults.Fail;
        foreach (Tile obj in players) {
            if (MayMoveTile(b, obj, dir)) {
                MoveTile(b, obj, dir);
                finalResult = MoveResults.Success;
            }
        }
        return finalResult;
    }
    
    
    
    public static bool MayMoveTile(Board b, Tile tile, Vector2Int dir) {
        if (b==null) { return false; } // Safety check.
        // Clone the Board!
        Board boardClone = b.Clone();
        Tile tileClone = boardClone.GetTile(tile.MyGuid);
        // Move the Tile, and return the result!
        return MoveTile(boardClone, tileClone, dir) == MoveResults.Success;
    }
    
    private static MoveResults MoveTiles(Board b, Vector2Int occPos, Vector2Int dir) {
        // Try to move each one. If ANY one fails, we return a TOTAL fail.
        List<Tile> tilesToMove = new List<Tile>(b.GetSpace(occPos).MyTiles); // copy the list.
        foreach (Tile obj in tilesToMove) {
            MoveResults result = MoveTile(b, obj, dir);
            if (result != MoveResults.Success) {
                return MoveResults.Fail;
            }
        }
        return MoveResults.Success;
    }
    public static MoveResults MoveTile(Board b, Tile tile, Vector2Int dir) {
        // No dir?? Do nothing; return success!
        if (dir == Vector2Int.zero) { return MoveResults.Success; }
        
        TranslationInfo ti = GetTranslationInfo(b, tile.ColRow, dir);
        BoardSpace spaceFrom = GetSpace(b, ti.from);
        BoardSpace spaceTo = GetSpace(b, ti.to);
        
        // Someone's null? Return Fail.
        if (tile==null || spaceTo==null) { return MoveResults.Fail; }
        // We can't EXIT this space? Return Fail.
        if (!spaceFrom.MayTileEverExit(ti.dirOut)) { return MoveResults.Fail; }
        // We can't ENTER this space? Return Fail.
        if (!spaceTo.MayTileEverEnter(ti.dirIn)) { return MoveResults.Fail; }

        // Always remove its footprint first. We're about to move it!
        tile.RemoveMyFootprint();
        
        // Next space has somethin'? Ok, try to move THAT fella, and return if fail!
        if (!spaceTo.IsVacant) {
            MoveResults result = MoveTiles(b, ti.to, ti.dirOut);
            if (result!=MoveResults.Success) { return result; }
        }
        
        // Okay, we're good to move our original fella! Do!
        tile.SetColRow(ti.to, ti.dirOut);
        // Put footprint back down.
        tile.AddMyFootprint();
        
        // Return success!
        return MoveResults.Success;
    }


}
public class TranslationInfo {
    public Vector2Int from, to;
    public Vector2Int dirOut, dirIn;
    //public int chirDeltaH, chirDeltaV;
    //public int sideDelta;
}