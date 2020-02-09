using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PropData {
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



public class TileData : PropData {
    public Guid MyGuid;
    public BoardPos boardPos;
    public TileType MyType;
}
public class GenericTileData : TileData {
    public GenericTileData(Guid myGuid, TileType myType, BoardPos boardPos) {
        this.MyGuid = myGuid;
        this.MyType = myType;
        this.boardPos = boardPos;
    }
}
public class TextBlockData : TileData {
    public TileType MySubjectType;
    public TextBlockData(Guid myGuid, BoardPos boardPos, TileType MySubjectType) {
        this.MyGuid = myGuid;
        this.boardPos = boardPos;
        this.MyType = TileType.TextBlock; // we already know I'm a TextBlock.
        this.MySubjectType = MySubjectType;
    }
}


//public class AbbaData : TileData {
//    public AbbaData (Guid myGuid, BoardPos boardPos) {
//        this.MyGuid = myGuid;
//        this.boardPos = boardPos;
//    }
//}
//public class CrateData : TileData {
//    public bool doAutoMove=false;
//    public Vector2Int autoMoveDir=Vector2Int.zero;
//    public CrateData (Guid myGuid, BoardPos boardPos, bool doAutoMove, Vector2Int autoMoveDir) {
//        this.MyGuid = myGuid;
//        this.boardPos = boardPos;
//        this.doAutoMove = doAutoMove;
//        this.autoMoveDir = autoMoveDir;
//    }
//}
//public class CrateGoalData : TileData {
//    public bool doStayOn;
//    public bool isOn;
//    public CrateGoalData(Guid myGuid, BoardPos boardPos, bool doStayOn, bool isOn) {
//        this.MyGuid = myGuid;
//        this.boardPos = boardPos;
//        this.doStayOn = doStayOn;
//        this.isOn = isOn;
//    }
//}
//public class ExitSpotData : TileData {
//    public ExitSpotData(Guid myGuid, BoardPos boardPos) {
//        this.MyGuid = myGuid;
//        this.boardPos = boardPos;
//    }
//}
