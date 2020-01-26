using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FlipTypes { Horizontal, Vertical }

public class BoardData {
	// Constants
	private readonly char[] LINE_BREAKS_CHARS = { ',' }; // our board layouts are comma-separated (because XML's don't encode line breaks).
	// Properties
    public int numCols,numRows;
    public WrapTypes wrapH,wrapV;
    // BoardObjects
    public List<PlayerData> playerDatas;// { get; private set; }
    public BoardSpaceData[,] spaceDatas { get; private set; }
    public List<BoardObjectData> allObjectDatas;
    private BoardObjectData[,] objectsInBoard; // this is SOLELY so we can go easily back and modify properties of an object we've already announced.
    
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
        wrapH = BoardUtils.IntToWrapType(ldxml.wrapH);
        wrapV = BoardUtils.IntToWrapType(ldxml.wrapV);
        
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
        
		objectsInBoard = new BoardObjectData[numCols,numRows];

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
                    // Player!
                    case '@': AddPlayerData(col,row); break;
                    // Crates!
                    case 'q': AddCrateGoalData (col,row, Corners.TL, false); break;
                    case 'w': AddCrateGoalData (col,row, Corners.TR, false); break;
                    case 's': AddCrateGoalData (col,row, Corners.BR, false); break;
                    case 'a': AddCrateGoalData (col,row, Corners.BL, false); break;
                    case 'e': AddCrateGoalData (col,row, Corners.TL, true); break;
                    case 'r': AddCrateGoalData (col,row, Corners.TR, true); break;
                    case 'f': AddCrateGoalData (col,row, Corners.BR, true); break;
                    case 'd': AddCrateGoalData (col,row, Corners.BL, true); break;
                    case 'Q': AddCrateData (col,row, Corners.TL, false); break;
                    case 'W': AddCrateData (col,row, Corners.TR, false); break;
                    case 'S': AddCrateData (col,row, Corners.BR, false); break;
                    case 'A': AddCrateData (col,row, Corners.BL, false); break;
                    case 'E': AddCrateData (col,row, Corners.TL, true); break;
                    case 'R': AddCrateData (col,row, Corners.TR, true); break;
                    case 'F': AddCrateData (col,row, Corners.BR, true); break;
                    case 'D': AddCrateData (col,row, Corners.BL, true); break;
                    //case 'O': AddCrateData (col,row, true); break;
                    //case '#': AddCrateData (col,row, false); break;
					// Walls!
					case '_': SetIsWallT (col,row-1); break; // note: because the underscore looks lower, consider it in the next row (so layout text file looks more intuitive).
					case '|': SetIsWallL (col,row); break;
                    
                    // MODIFYING properties...
                    case 'Z': FlipChirH(col,row); break;
                    case 'X': FlipChirV(col,row); break;
                    case 'm': SetNotMovable(col,row); break;
					}
				}
			}
		}
        
        // Safety check.
        if (playerDatas.Count == 0) { AddPlayerData(0,0); }

		// We can empty out those lists now.
		objectsInBoard = null;
	}

	/** Initializes a totally empty BoardData. */
	public BoardData (int _numCols,int _numRows) {
		numCols = _numCols;
		numRows = _numRows;
		MakeEmptyBoardSpaces ();
		MakeEmptyLists ();
	}

	private void MakeEmptyLists () {
        playerDatas = new List<PlayerData>();
		allObjectDatas = new List<BoardObjectData>();
		objectsInBoard = new BoardObjectData[numCols,numRows];
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
	private BoardObjectData GetObjectInBoard (int col,int row) { return objectsInBoard[col,row]; }
	private void SetOccupantInBoard (BoardObjectData data) { objectsInBoard[data.boardPos.ColRow.x,data.boardPos.ColRow.y] = data; }


    
    //void SetPlayerData(int col,int row) {
    //    if (playerData != null) { Debug.LogError("Whoa! Two players defined in Level XML layout."); return; } // Safety check.
    //    playerData = new PlayerData(new BoardPos(col,row), false);
    //    //allObjectDatas.Add (playerData);
    //    SetOccupantInBoard (playerData);
    //}
    void AddPlayerData(int col,int row) {
        PlayerData data = new PlayerData(new BoardPos(col,row), false);
        playerDatas.Add(data);
        allObjectDatas.Add (data);
        SetOccupantInBoard (data);
    }
    
    void AddCrateData (int col,int row, int dimpleCorner, bool doAutoMove) {
        bool[] isDimple = new bool[Corners.NumCorners];
        isDimple[dimpleCorner] = true;
        CrateData newData = new CrateData (new BoardPos(col,row), isDimple, doAutoMove, Vector2Int.zero);
        allObjectDatas.Add (newData);
        SetOccupantInBoard (newData);
    }
    //void AddCrateData (int col,int row, bool isMovable) {
    //    CrateData newData = new CrateData (new Vector2Int(col,row), isMovable);
    //    allObjectDatas.Add (newData);
    //    SetOccupantInBoard (newData);
    //}
    void AddExitSpotData (int col,int row) {
        ExitSpotData newData = new ExitSpotData (new BoardPos(col,row));
        allObjectDatas.Add (newData);
        SetOccupantInBoard (newData);
    }
    void AddCrateGoalData (int col,int row, int corner, bool doStayOn) {
        CrateGoalData newData = new CrateGoalData (new BoardPos(col,row), corner, doStayOn, false);
        allObjectDatas.Add (newData);
        SetOccupantInBoard (newData);
    }
    
    void SetIsWallL(int col,int row) {
        if (!IsInBounds(col,row)) { return; } // Safety check.
        spaceDatas[col,row].isWallL = true;
    }
    void SetIsWallT(int col,int row) {
        if (row<0) { row += numRows; } // Hardcoded. Wrap bottom walls to top of screen.
        if (!IsInBounds(col,row)) { return; } // Safety check.
        spaceDatas[col,row].isWallT = true;
    }
    
    private void FlipChirH(int col,int row) {
        BoardObjectData bo = GetObjectInBoard(col,row);
        if (bo != null) { bo.boardPos.ChirH *= -1; }
    }
    private void FlipChirV(int col,int row) {
        BoardObjectData bo = GetObjectInBoard(col,row);
        if (bo != null) { bo.boardPos.ChirV *= -1; }
    }
    private void SetNotMovable(int col,int row) {
        BoardOccupantData bo = GetObjectInBoard(col,row) as BoardOccupantData;
        if (bo != null) { bo.isMovable = false; }
    }



}