using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericTileView : TileView {
    // Components
    [SerializeField] private Image i_body=null;
    [SerializeField] private Image i_autoMoveDir=null;
	// References
    public GenericTile MyGenericTile { get; private set; }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
    override public void Initialize (BoardView _myBoardView, Tile bo) {
        MyGenericTile = bo as GenericTile;
        base.Initialize (_myBoardView, bo);
        
        // Set body sprite!
        i_body.sprite = ResourcesHandler.Instance.GetTileSprite(MyGenericTile.MyType);
	}


    // ----------------------------------------------------------------
    //  Update Visuals
    // ----------------------------------------------------------------
    //public override void UpdateVisualsPostMove() {
    //    base.UpdateVisualsPostMove();
        
    //    if (MyCrate.DoAutoMove) {
    //        bool isArrow = MyCrate.AutoMoveDir!=Vector2Int.zero;
    //        i_autoMoveDir.enabled = isArrow;
    //        if (isArrow) {
    //            float dirRot = MathUtils.GetSide(MyCrate.AutoMoveDir) * -90;
    //            i_autoMoveDir.transform.localEulerAngles = new Vector3(0, 0, dirRot);
    //        }
    //    }
    //}



}

