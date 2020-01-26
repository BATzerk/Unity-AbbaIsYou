using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropData {
}

public class BoardObjectData : PropData {
	public BoardPos boardPos;
//	public BoardObjectData (BoardPos _boardPos) {
//		boardPos = _boardPos;
//	}
}
public class BoardOccupantData : BoardObjectData {
	public bool isMovable = false; // exensions of this class will set this in their data constructor.
	public bool isPassRotatable = false; // I rotate if something passes by me.
	public bool isSidePull = false; // I'm pulled to the SIDE of things, instead of behind them.
}
public class BoardSpaceData : BoardObjectData {
	public bool isPlayable = true;
	public BoardSpaceData (int _col,int _row) {
		boardPos.col = _col;
		boardPos.row = _row;
	}
}
public class CrateData : BoardOccupantData {
	public CrateData (BoardPos _boardPos) {
		boardPos = _boardPos;
		isMovable = true;
	}
}
public class CrateGoalData : BoardObjectData {
	public CrateGoalData (BoardPos _boardPos) {
		boardPos = _boardPos;
	}
}
public class ExitSpotData : BoardObjectData {
	public ExitSpotData (BoardPos _boardPos) {
		boardPos = _boardPos;
	}
	public ExitSpotData (int _col,int _row) {
		boardPos = new BoardPos (_col,_row, 0, 0);
	}
}
public class PlayerData : BoardOccupantData {
	public PlayerData (BoardPos _boardPos) {
		boardPos = _boardPos;
		isMovable = true;
	}
}
public class PusherData : BoardObjectData {
	public PusherData (BoardPos _boardPos) {
		boardPos = _boardPos;
	}
}
public class WallData : BoardObjectData {
	public WallData (BoardPos _boardPos) {
		boardPos = _boardPos;
	}
}
