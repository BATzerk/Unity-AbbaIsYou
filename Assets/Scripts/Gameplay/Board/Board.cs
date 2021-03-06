﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Board {
    // Properties
    public int NumCols { get; private set; }
    public int NumRows { get; private set; }
    public bool AreGoalsSatisfied { get; private set; }
    // Objects
    public BoardSpace[,] spaces;
    public List<Tile> allTiles; // includes every object EXCEPT Player!
    private List<TextRule> textRules = new List<TextRule>();
    // Reference Lists
    //public List<IGoalObject> goalObjects; // contains JUST the objects that have winning criteria.
    public List<Tile> objectsAddedThisMove;

	// Getters
    //public int NumGoalObjects { get { return goalObjects.Count; } }
    public Tile GetTile(System.Guid guid) {
        foreach (Tile obj in allTiles) { // Brute-force for now!
            if (Equals(obj.MyGuid, guid)) {
                return obj;
            }
        }
        return null; // Oops.
    }
    public BoardSpace GetSpace(Vector2Int pos) { return GetSpace(pos.x, pos.y); }
    public BoardSpace GetSpace(int col,int row) { return BoardUtils.GetSpace(this, col,row); }
    public List<Tile> GetObjects(int col,int row) { return BoardUtils.GetTiles(this, col,row); }
    public List<Tile> GetObjects(Vector2Int pos) { return GetObjects(pos.x, pos.y); }
	public BoardSpace[,] Spaces { get { return spaces; } }
    public List<Tile> GetPlayers() {
        List<Tile> players = new List<Tile>();
        foreach (Tile obj in allTiles) {
            if (obj.IsYou) { players.Add(obj); }
        }
        return players;
    }
    //public bool IsAnyPlayerOnExitSpot () {
    //    List<Tile> players = GetPlayers();
    //    for (int i=0; i<players.Count; i++) {
    //        if (players[i].MySpace.HasExitSpot()) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    private bool GetAreGoalsSatisfied() {
        ////if (!AreAnyPlayers()) { return false; } // Players are all kaput? Nah, we need at least one alive.TODO: This.
        //if (goalObjects.Count == 0) { return true; } // If there's NO criteria, then sure, we're satisfied! For levels that're just about getting to the exit.
        //for (int i=0; i<goalObjects.Count; i++) {
        //    if (!goalObjects[i].IsOn) { return false; } // return false if any of these guys aren't on.
        //}
        //return true; // Looks like we're soooo satisfied!!
        int numOverlapGoals = 0;
        int numOverlapGoalsHappy = 0;
        foreach (Tile tile in allTiles) {
            if (tile.IsOverlapGoal) {
                numOverlapGoals ++;
                if (tile.IsOverlapGoalSatisfied()) { // if there's another thing on this space, it's happy!
                    numOverlapGoalsHappy ++;
                }
            }
        }
        return numOverlapGoals>0 && numOverlapGoalsHappy>=numOverlapGoals;
    }
    //public bool AreAnyPlayers() {
    //    return GetPlayers().Count > 0;
    //}
    //private bool CheckAreGoalsSatisfied () {
    //    if (goalObjects.Count == 0) { return true; } // If there's NO criteria, then sure, we're satisfied! For levels that're just about getting to the exit.
    //    for (int i=0; i<goalObjects.Count; i++) {
    //        if (!goalObjects[i].IsOn) { return false; } // return false if any of these guys aren't on.
    //    }
    //    return true; // Looks like we're soooo satisfied!!
    //}

    // Serializing
	public Board Clone() {
		BoardData data = ToData();
		return new Board(data);
	}
	public BoardData ToData() {
		BoardData bd = new BoardData(NumCols,NumRows);
		foreach (Tile obj in allTiles) { bd.allTileDatas.Add(obj.ToData()); }
		for (int col=0; col<NumCols; col++) {
			for (int row=0; row<NumRows; row++) {
				bd.spaceDatas[col,row] = GetSpace(col,row).ToData();
			}
		}
		return bd;
	}

    

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public Board (BoardData bd) {
		NumCols = bd.numCols;
		NumRows = bd.numRows;
        
        // Empty out lists.
        allTiles = new List<Tile>();
        //goalObjects = new List<IGoalObject>();
        objectsAddedThisMove = new List<Tile>();

		// Add all gameplay objects!
		MakeBoardSpaces (bd);
		AddPropsFromBoardData (bd);
        RefreshAndApplyTextRules();
        AreGoalsSatisfied = GetAreGoalsSatisfied(); // know from the get-go.
	}

	private void MakeBoardSpaces (BoardData bd) {
		spaces = new BoardSpace[NumCols,NumRows];
		for (int i=0; i<NumCols; i++) {
			for (int j=0; j<NumRows; j++) {
				spaces[i,j] = new BoardSpace (bd.spaceDatas[i,j]);
			}
		}
	}
	private void AddPropsFromBoardData (BoardData bd) {
        //player = new Player(this, bd.playerData);
		foreach (TileData objData in bd.allTileDatas) {
            System.Type type = objData.GetType();
            if (false) {}
            //else if (type == typeof(ExitSpotData)) {
            //    AddExitSpot (objData as ExitSpotData);
            //}
            else if (type == typeof(GenericTileData)) {
                AddGenericTile (objData as GenericTileData);
            }
            else if (type == typeof(TextBlockData)) {
                AddTextBlock (objData as TextBlockData);
            }
            else {
                Debug.LogError("PropData not recognized to add to Board: " + type);
            }
        }
	}
    
    //private void AddExitSpot (ExitSpotData data) {
    //    ExitSpot prop = new ExitSpot (this, data);
    //    allTiles.Add (prop);
    //    objectsAddedThisMove.Add(prop);
    //    goalObjects.Add (prop);
    //}
    private void AddGenericTile (GenericTileData data) {
        GenericTile obj = new GenericTile (this, data);
        allTiles.Add (obj);
        objectsAddedThisMove.Add(obj);
    }
    private void AddTextBlock (TextBlockData data) {
        TextBlock prop = new TextBlock (this, data);
        allTiles.Add (prop);
        objectsAddedThisMove.Add(prop);
    }


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
    public void OnObjectRemovedFromPlay (Tile bo) {
        // Remove it from its lists!
        allTiles.Remove (bo);
        //if (bo is ExitSpot) { NumExitSpots --; }
        //else { Debug.LogError ("Trying to RemoveFromPlay an Object of type " + bo.GetType() + ", but our OnObjectRemovedFromPlay function doesn't recognize this type!"); }
    }


	// ----------------------------------------------------------------
	//  Makin' Moves
	// ----------------------------------------------------------------
    public void ExecuteMoveAttempt(Vector2Int dir) {
        if (BoardUtils.MayExecuteMove(this, dir)) {
            // Clear out the list NOW.
            objectsAddedThisMove.Clear();
            // Reset PrevMoveDelta.
            ResetTilesPrevMoveDelta();
            // Move players!
            BoardUtils.ExecuteMove(this, dir);
            // Tell all other Tiles!
            for (int i=0; i<allTiles.Count; i++) { allTiles[i].OnPlayerMoved(); }
            // Update Goals!
            //foreach (IGoalObject igo in goalObjects) { igo.UpdateIsOn (); }
            AreGoalsSatisfied = GetAreGoalsSatisfied();
            // Dispatch event!
            GameManagers.Instance.EventManager.OnBoardExecutedMove(this);
        }
    }


    private void ResetTilesPrevMoveDelta() {
        for (int i=0; i<allTiles.Count; i++) { allTiles[i].ResetPrevMoveDelta(); }
    }
    
    private TextBlock GetTextWithLoc(int col,int row, TextLoc textLoc) {
        BoardSpace space = GetSpace(col,row);
        if (space == null) { return null; }
        foreach (Tile tile in space.MyTiles) {
            if (tile is TextBlock && (tile as TextBlock).MyTextLoc==textLoc) {
                return tile as TextBlock;
            }
        }
        return null; // Nah, didn't find one.
    }
    
    // ----------------------------------------------------------------
    //  Text Rules
    // ----------------------------------------------------------------
    public void RefreshAndApplyTextRules() {
        // 1) Find all the BlockSentences in the board.
        List<BlockSentence> blockSentences = new List<BlockSentence>();
        foreach (Tile tile in allTiles) {
            TextBlock start = tile as TextBlock;
            if (start == null) { continue; } // Skip all non-TextBlocks.
            if (start.MyTextLoc != TextLoc.Start) { continue; } // Skip all non-Starts.
            
            TextBlock middle, end;
            // Find the HORZ sentence!
            middle = GetTextWithLoc(start.Col+1, start.Row, TextLoc.Middle);
            end = GetTextWithLoc(start.Col+2, start.Row, TextLoc.End);
            // HACK TEMP sloppy
            if (end == null) { end = GetTextWithLoc(start.Col+2, start.Row, TextLoc.Start); }
            if (middle != null && end != null) {
                blockSentences.Add(new BlockSentence(start, middle, end));
            }
            // Find the VERT sentence!
            middle = GetTextWithLoc(start.Col, start.Row-1, TextLoc.Middle);
            end = GetTextWithLoc(start.Col, start.Row-2, TextLoc.End);
            // HACK TEMP sloppy
            if (end == null) { end = GetTextWithLoc(start.Col, start.Row-2, TextLoc.Start); }
            if (middle != null && end != null) {
                blockSentences.Add(new BlockSentence(start, middle, end));
            }
        }
        
        // 2) Update textBlocks' IsInSentence
        foreach (Tile tile in allTiles) {
            if (tile is TextBlock) { (tile as TextBlock).IsInSentence = false; }
        }
        foreach (BlockSentence sentence in blockSentences) {
            sentence.SetMyTextBlocksIsInSentenceToTrue();
        }
        
        // 3) Update textRules from sentences!
        textRules.Clear();
        textRules.Add(new TextRule(TileType.TextBlock, RuleOperator.IsPush)); // Hardcoded: ALL text is always pushable!
        foreach (BlockSentence sentence in blockSentences) {
            textRules.Add(sentence.GetMyRule());
        }
        
        // 4) RELEASE all tiles' rules.
        foreach (Tile obj in allTiles) {
            obj.ReleaseRuleProperties();
        }
        
        // 5) Apply conversions, THEN all other rules.
        foreach (TextRule rule in textRules) {
            if (rule.MyOperator == RuleOperator.Convert) {
                rule.ApplyToBoard(this);
            }
        }
        foreach (TextRule rule in textRules) {
            if (rule.MyOperator != RuleOperator.Convert) {
                rule.ApplyToBoard(this);
            }
        }
    }
    
    
    
    
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //  Debug
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void Debug_PrintSomeBoardLayout() {
        string str = "";
        for (int row=0; row<NumRows; row++) {
            for (int col=0; col<NumCols; col++) {
                str += Debug_GetPrintoutSpaceChar(col,row);
            }
            str += "\n";
        }
        Debug.Log("Board Layout:\n" + str);
    }
    private string Debug_GetPrintoutSpaceChar(int col,int row) {
        //Tile obj = GetObjects(col,row);
        //if (obj == null) { return "."; }
        //if (obj is Player) { return "@"; }
        //if (obj is Crate) { return "o"; }
        return "?";
    }





}

