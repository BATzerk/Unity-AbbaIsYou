using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PropData {
}

public class BoardObjectData : PropData {
    public BoardPos boardPos;
}
public class BoardOccupantData : BoardObjectData {
    public bool isMovable = true;
}
public class BoardSpaceData {
    public Vector2Int ColRow;
    public bool isPlayable = true;
    public bool isWallL=false, isWallT=false;
	public BoardSpaceData (int _col,int _row) {
		this.ColRow.x = _col;
		this.ColRow.y = _row;
	}
}
public class CrateData : BoardOccupantData {
    public bool doAutoMove=false;
    public Vector2Int autoMoveDir=Vector2Int.zero;
    public bool[] isDimple;
    public CrateData (BoardPos boardPos, bool[] isDimple, bool doAutoMove, Vector2Int autoMoveDir) {
        this.boardPos = boardPos;
        this.isDimple = isDimple;
        this.doAutoMove = doAutoMove;
        this.autoMoveDir = autoMoveDir;
    }
}
public class CrateGoalData : BoardObjectData {
    public bool doStayOn;
    public bool isOn;
    public int corner;
    public CrateGoalData(BoardPos boardPos, int corner, bool doStayOn, bool isOn) {
        this.boardPos = boardPos;
        this.corner = corner;
        this.doStayOn = doStayOn;
        this.isOn = isOn;
    }
}
public class ExitSpotData : BoardObjectData {
    public ExitSpotData(BoardPos boardPos) {
        this.boardPos = boardPos;
    }
}
public class PlayerData : BoardOccupantData {
    public bool isDead;
    public PlayerData (BoardPos boardPos, bool isDead) {
        this.boardPos = boardPos;
        this.isDead = isDead;
    }
}
