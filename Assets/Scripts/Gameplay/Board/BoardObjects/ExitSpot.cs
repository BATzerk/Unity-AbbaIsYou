//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ExitSpot : Tile, IGoalObject {
//    // Properties
//    public bool IsOn { get; private set; }
    
    
//    // Serializing
//    override public TileData ToData() {
//        return new GenericTileData(MyGuid, MyType, BoardPos);
//    }
    
//    // ----------------------------------------------------------------
//    //  Initialize
//    // ----------------------------------------------------------------
//    public ExitSpot (Board _boardRef, GenericTileData data) {
//        base.InitializeAsTile (_boardRef, data);
//        //IsOn = data.isOn;
//    }
    
    
//    // ----------------------------------------------------------------
//    //  Doers
//    // ----------------------------------------------------------------
//    public void UpdateIsOn () {
//        IsOn = MySpace.MyTiles.Count > 1; // I'll accept anything overlapping me.
//        Debug.Log("UpdateIsOn: " + IsOn);//QQQ
//    }
//    //private bool GetIsSatisfied() {
//        ////return MySpace.MyTiles.Count > 1; // I'll accept anything overlapping me.
//        //foreach (Tile obj in MySpace.MyTiles) {
//        //    if (obj is Crate) {
//        //        return true;
//        //    }
//        //}
//        //return false;
//    //}

//}
