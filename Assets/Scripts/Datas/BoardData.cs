using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardData {
	// Constants
	private readonly char[] LINE_BREAKS_CHARS = new char[] { ',' }; // our board layouts are comma-separated (because XML's don't encode line breaks).
	// Properties
	public int numCols,numRows;
	// BoardObjects
	public BoardSpaceData[,] spaceDatas;
	private BoardOccupantData[,] occupantsInBoard; // this is SOLELY so we can go easily back and modify properties of an occupant we've already announced.
	private int[,] numOccupantsInBoard; // this is for setting the LAYER of each incoming Occupant.
	private List<BoardObjectData> allObjectDatas;
	public List<CrateData> crateDatas;
	public List<CrateGoalData> crateGoalDatas;
	public List<ExitSpotData> exitSpotDatas;
	public List<PlayerData> playerDatas;
	public List<PusherData> pusherDatas;
	public List<WallData> wallDatas;

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

	static private BoardPos GetRotatedBoardPos (BoardPos _boardPos, int rotOffset, int _numCols,int _numRows) {
		if (rotOffset < 0) { rotOffset += 4; } // keep it in bounds between 1-3.
		// Simple check.
		if (rotOffset==0) { return _boardPos; }

		BoardPos newBoardPos = _boardPos;
		int sin = (int)Mathf.Sin(rotOffset*Mathf.PI*0.5f);
		int cos = (int)Mathf.Cos(rotOffset*Mathf.PI*0.5f);

		int fullColOffset=0;
		int fullRowOffset=0;
		switch(rotOffset) {
			case 1:
				fullColOffset = _numCols-1; break;
			case 2:
				fullColOffset = _numCols-1;
				fullRowOffset = _numRows-1; break;
			case 3:
				fullRowOffset = _numRows-1; break;
			default:
				Debug.LogError ("Passed in an invalid value into GetRotatedBoardPos: " + rotOffset +". Only 1, 2, or 3 are allowed."); break;
		}
		// 0,0 -> numCols,0
		// numCols,0 -> numCols,numRows
		// numCols,numRows -> 0,numRows
		// 0,numRows -> 0,0

		// 0,1 -> 1,numRows
//		{{1,0},{0,1},{-1,0},{0,-1}}

		// col,row!
		newBoardPos.col = fullColOffset + _boardPos.col*cos - _boardPos.row*sin;
		newBoardPos.row = fullRowOffset + _boardPos.col*sin - _boardPos.row*cos;
		// sideFacing!
		newBoardPos.sideFacing += rotOffset;
		return newBoardPos;
	}

	public void RotateCW () { Rotate (1); }
	public void RotateCCW () { Rotate (3); }
	public void Rotate180 () { Rotate (2); }
	private void Rotate (int rotOffset) {
		int pnumCols = numCols;
		int pnumRows = numRows;
		// Update my # of cols/rows!
		if (rotOffset%2==1) {
			numCols = pnumRows;
			numRows = pnumCols;
		}
		// Remake grid spaces!
		BoardSpaceData[,] newSpaceDatas = new BoardSpaceData[numCols,numRows];
		for (int col=0; col<numCols; col++) {
			for (int row=0; row<numRows; row++) {
				BoardPos oldSpaceBoardPos = GetRotatedBoardPos (new BoardPos(col,row, 0,0), -rotOffset, pnumCols,pnumRows); // -rotOffset because we're starting with the *new* col/row and looking for the old one.
				newSpaceDatas[col,row] = GetSpaceData(oldSpaceBoardPos.col, oldSpaceBoardPos.row); // set the new guy to EXACTLY the old guy!
				newSpaceDatas[col,row].boardPos = new BoardPos(col,row, 0,0); // Update its col/row, of course (that hasn't been done yet)!
			}
		}
		spaceDatas = newSpaceDatas;

		// Update BoardPos of all BoardObjects!
		foreach (BoardObjectData data in allObjectDatas) {
			data.boardPos = GetRotatedBoardPos (data.boardPos, rotOffset, numCols,numRows);
		}
		// Unintuitive! Make sure all the Players are facing upright, no matter how we've rotated the Board.
		foreach (PlayerData data in playerDatas) {
			data.boardPos.sideFacing = 0;
		}
	}


	public BoardData (LevelDataXML ldxml) {
		// Layout!
		string[] levelStringArray = GetLevelStringArrayFromLayoutString (ldxml.layout);

		int numLayoutLayers = 1; // will increment for every "", we find.
		for (int i=0; i<levelStringArray.Length; i++) {
			if (levelStringArray[i].Length == 0) { // We found a break that denotes another layer of layout!
				numLayoutLayers ++;
			}
		}

		// Set numCols and numRows!
		if (levelStringArray.Length == 0) {
			Debug.LogError ("Uhh! levelStringArray is empty?? " + ldxml.name);
		}
		numCols = levelStringArray[0].Length;
		numRows = (int)((levelStringArray.Length-numLayoutLayers+1)/numLayoutLayers);

		// Add all gameplay objects!
		MakeEmptyBoardSpaces ();
		MakeEmptyLists ();

		occupantsInBoard = new BoardOccupantData[numCols,numRows];
		numOccupantsInBoard = new int[numCols,numRows];

		for (int layer=0; layer<numLayoutLayers; layer++) {
			for (int i=0; i<numCols; i++) {
				for (int j=0; j<numRows; j++) {
					int stringArrayIndex = j + layer*(numRows+1);
					if (stringArrayIndex>=levelStringArray.Length || i>=levelStringArray[stringArrayIndex].Length) {
						Debug.LogError ("Whoops! Mismatch in layout in a board layout XML. " + ldxml.name + " " + stringArrayIndex + ", " + i);
						continue;
					}
					char spaceChar = (char) levelStringArray[stringArrayIndex][i];
					switch (spaceChar) {
						// Player Start!
						case '@': AddPlayerData (i,j); break;
							// ExitSpot!
						case '$': AddExitSpotData (i,j); break;
							// BoardSpace properties!
						case '~': GetSpaceData (i,j).isPlayable = false; break;
							// Walls!
						case '_': AddWallData (i,j+1, 0); break; // note: because the underscore looks lower, consider it in the next row (so layout text file looks more intuitive).
						case '|': AddWallData (i,j, 3); break;
							// Exiting-object Modifiers!
						case 'm': SetOccupantIsMovable(ldxml.name, i,j, false); break;
						case 'M': SetOccupantIsMovable(ldxml.name, i,j, true); break;
						case 't': SetOccupantIsPassRotatable(ldxml.name, i,j, true); break;
						case 'i': SetOccupantIsSidePull(ldxml.name, i,j, true); break;
							// Crate!
						case 'o': AddCrateData (i,j); break;
						case 'O': AddCrateGoalData (i,j); break;
					}
				}
			}
		}

		// Catch to make sure we have at least one Player.
		if (playerDatas.Count == 0) {
			AddPlayerData (0,0);
		}

		// We can empty out occupantsInBoard now.
		occupantsInBoard = null;
		numOccupantsInBoard = null;
	}
	/** Initializes a totally empty BoardData. */
	public BoardData (int _numCols,int _numRows) {
		numCols = _numCols;
		numRows = _numRows;
		MakeEmptyBoardSpaces ();
		MakeEmptyLists ();
	}

	private void MakeEmptyLists () {
		allObjectDatas = new List<BoardObjectData>();
		crateDatas = new List<CrateData>();
		crateGoalDatas = new List<CrateGoalData>();
		exitSpotDatas = new List<ExitSpotData>();
		playerDatas = new List<PlayerData>();
		pusherDatas = new List<PusherData>();
		wallDatas = new List<WallData>();
	}
	private void MakeEmptyBoardSpaces () {
		spaceDatas = new BoardSpaceData[numCols,numRows];
		for (int i=0; i<numCols; i++) {
			for (int j=0; j<numRows; j++) {
				spaceDatas[i,j] = new BoardSpaceData (i,j);
			}
		}
	}

	private BoardSpaceData GetSpaceData (int col,int row) {
		return spaceDatas[col,row];
	}
	private BoardOccupantData GetOccupantInBoard (int col,int row) {
		return occupantsInBoard[col,row];
	}
	private void SetOccupantInBoard (BoardOccupantData data) {
		occupantsInBoard[data.boardPos.col,data.boardPos.row] = data;
		numOccupantsInBoard[data.boardPos.col,data.boardPos.row] ++;
	}
	private int NumOccupantDatasAtPos (int col,int row) {
		return numOccupantsInBoard[col,row];
	}


	void AddCrateGoalData (int col,int row) {
		CrateGoalData newData = new CrateGoalData (new BoardPos(col,row, 0, 0));
		crateGoalDatas.Add (newData);
		allObjectDatas.Add (newData);
	}
	void AddExitSpotData (int col,int row) {
		ExitSpotData newData = new ExitSpotData (new BoardPos(col,row, 0, 0));
		exitSpotDatas.Add (newData);
		allObjectDatas.Add (newData);
	}
	void AddPlayerData (int col,int row) {
		PlayerData newData = new PlayerData (new BoardPos (col,row, 0, 0));
		playerDatas.Add (newData);
		allObjectDatas.Add (newData);
	}
	void AddPusherData (int col,int row, int sideFacing) {
		PusherData newData = new PusherData (new BoardPos(col,row, sideFacing, 0));
		pusherDatas.Add (newData);
		allObjectDatas.Add (newData);
	}
	void AddWallData (int col,int row, int sideFacing) {
		WallData newData = new WallData (new BoardPos(col,row, sideFacing, 0));
		wallDatas.Add (newData);
		allObjectDatas.Add (newData);
	}

	void AddCrateData (int col,int row) {
		int layer = NumOccupantDatasAtPos(col,row);
		CrateData newData = new CrateData (new BoardPos(col,row, 0, layer));
		crateDatas.Add (newData);
		allObjectDatas.Add (newData);
		SetOccupantInBoard (newData);
	}


	private void SetOccupantIsMovable (string ldName, int col,int row, bool value) {
		BoardOccupantData data = GetOccupantInBoard(col,row);
		if (data == null) { Debug.LogError ("Error in level layout: " + ldName + ". No occupant found at: " + col + "," + row); return; }
		data.isMovable = value;
	}
	private void SetOccupantIsPassRotatable (string ldName, int col,int row, bool value) {
		BoardOccupantData data = GetOccupantInBoard(col,row);
		if (data == null) { Debug.LogError ("Error in level layout: " + ldName + ". No occupant found at: " + col + "," + row); return; }
		data.isPassRotatable = value;
	}
	private void SetOccupantIsSidePull (string ldName, int col,int row, bool value) {
		BoardOccupantData data = GetOccupantInBoard(col,row);
		if (data == null) { Debug.LogError ("Error in level layout: " + ldName + ". No occupant found at: " + col + "," + row); return; }
		data.isSidePull = value;
	}

}