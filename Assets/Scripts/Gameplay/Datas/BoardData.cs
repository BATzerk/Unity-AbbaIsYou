﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FlipTypes { Horizontal, Vertical }

public class BoardData {
	// Constants
	private readonly char[] LINE_BREAKS_CHARS = { ',' }; // our board layouts are comma-separated (because XML's don't encode line breaks).
	// Properties
    public int numCols,numRows;
    // Board Objects
    public BoardSpaceData[,] spaceDatas { get; private set; }
    public List<TileData> allTileDatas;
    private TileData[,] tilesInBoard; // this is SOLELY so we can go easily back and modify properties of an object we've already announced.
    
    // Getters
	private string[] GetLevelStringArrayFromLayoutString (string layout) {
		List<string> stringList = new List<string>(layout.Split (LINE_BREAKS_CHARS, System.StringSplitOptions.None));
		// Remove the last element, which will be just empty space (because of how we format the layout in the XML).
		stringList.RemoveAt (stringList.Count-1);
		// Cut the excess white space.
		for (int i=0; i<stringList.Count; i++) {
			stringList[i] = TextUtils.RemoveWhitespace (stringList[i]);
		}
		return stringList.ToArray();
	}
    private bool IsInBounds(int col,int row) {
        return col>=0 && col<numCols  &&  row>=0 && row<numRows;
    }
	


    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
	public BoardData (LevelDataXML ldxml) {
		// Layout!
		string[] levelStringArray = GetLevelStringArrayFromLayoutString (ldxml.layout);
		if (levelStringArray.Length == 0) { levelStringArray = new string[]{"."}; } // Safety catch.

		int numLayoutLayers = 1; // will increment for every "", we find.
		for (int i=0; i<levelStringArray.Length; i++) {
			if (levelStringArray[i].Length == 0) { // We found a break that denotes another layer of layout!
				numLayoutLayers ++;
			}
		}

		// Set numCols and numRows!
		if (levelStringArray.Length == 0) {
			Debug.LogError ("Uhh! levelStringArray is empty!");
		}
		numCols = levelStringArray[0].Length;
		numRows = (int)((levelStringArray.Length-numLayoutLayers+1)/numLayoutLayers);

		// Add all gameplay objects!
		MakeEmptyBoardSpaces ();
		MakeEmptyLists ();
        
		tilesInBoard = new TileData[numCols,numRows];

		for (int layer=0; layer<numLayoutLayers; layer++) {
			for (int i=0; i<numCols; i++) {
				for (int j=0; j<numRows; j++) {
					int stringArrayIndex = j + layer*(numRows+1);
					if (stringArrayIndex>=levelStringArray.Length || i>=levelStringArray[stringArrayIndex].Length) {
						Debug.LogError ("Whoops! Mismatch in layout in a board layout XML. " + stringArrayIndex + ", " + i);
						continue;
					}
                    char spaceChar = (char) levelStringArray[stringArrayIndex][i];
                    int col = i;
                    int row = numRows-1 - j;
                    switch (spaceChar) {
                    // BoardSpace properties!
                    case '~': GetSpaceData (col,row).isPlayable = false; break;
                    // ExitSpot!
                    case '$': AddExitSpotData (col,row); break;
                    // Abba!
                    case '@': AddAbbaData(col,row); break;
                    // Crates!
                    case 'Q': AddCrateData (col,row, false); break;
                    case 'E': AddCrateData (col,row, true); break;
                    case 'q': AddCrateGoalData (col,row, false); break;
                    case 'e': AddCrateGoalData (col,row, true); break;
                    //case 'O': AddCrateData (col,row, true); break;
                    //case '#': AddCrateData (col,row, false); break;
					// Walls!
					case '_': SetIsWallT (col,row-1); break; // note: because the underscore looks lower, consider it in the next row (so layout text file looks more intuitive).
					case '|': SetIsWallL (col,row); break;
                    
                    //// MODIFYING properties...
                    //case 'm': SetNotMovable(col,row); break;
					}
				}
			}
		}

		// We can empty out those lists now.
		tilesInBoard = null;
	}

	/** Initializes a totally empty BoardData. */
	public BoardData (int _numCols,int _numRows) {
		numCols = _numCols;
		numRows = _numRows;
		MakeEmptyBoardSpaces ();
		MakeEmptyLists ();
	}

	private void MakeEmptyLists () {
		allTileDatas = new List<TileData>();
		tilesInBoard = new TileData[numCols,numRows];
	}
	private void MakeEmptyBoardSpaces () {
		spaceDatas = new BoardSpaceData[numCols,numRows];
		for (int i=0; i<numCols; i++) {
			for (int j=0; j<numRows; j++) {
				spaceDatas[i,j] = new BoardSpaceData (i,j);
			}
		}
	}

	private BoardSpaceData GetSpaceData (int col,int row) { return spaceDatas[col,row]; }
	private TileData GetObjectInBoard (int col,int row) { return tilesInBoard[col,row]; }
	private void SetTileInBoard (TileData data) { tilesInBoard[data.boardPos.ColRow.x,data.boardPos.ColRow.y] = data; }


    
    void AddAbbaData(int col,int row) {
        AbbaData data = new AbbaData(new BoardPos(col,row));
        allTileDatas.Add (data);
        SetTileInBoard (data);
    }
    
    void AddCrateData (int col,int row, bool doAutoMove) {
        CrateData newData = new CrateData (new BoardPos(col,row), doAutoMove, Vector2Int.zero);
        allTileDatas.Add (newData);
        SetTileInBoard (newData);
    }
    void AddExitSpotData (int col,int row) {
        ExitSpotData newData = new ExitSpotData (new BoardPos(col,row));
        allTileDatas.Add (newData);
        SetTileInBoard (newData);
    }
    void AddCrateGoalData (int col,int row, bool doStayOn) {
        CrateGoalData newData = new CrateGoalData (new BoardPos(col,row), doStayOn, false);
        allTileDatas.Add (newData);
        SetTileInBoard (newData);
    }
    
    void SetIsWallL(int col,int row) {
        if (!IsInBounds(col,row)) { return; } // Safety check.
        spaceDatas[col,row].isWallL = true;
    }
    void SetIsWallT(int col,int row) {
        if (!IsInBounds(col,row)) { return; } // Safety check.
        spaceDatas[col,row].isWallT = true;
    }
    
    //private void SetNotMovable(int col,int row) {
    //    TileData bo = GetObjectInBoard(col,row) as TileData;
    //    if (bo != null) { bo.isMovable = false; }
    //}



}