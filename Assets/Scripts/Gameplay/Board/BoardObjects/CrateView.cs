//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class CrateView : TileView {
//    // Components
//    [SerializeField] private Image i_body=null;
//    [SerializeField] private Image i_autoMoveDir=null;
//	// References
//    public Crate MyCrate { get; private set; }


//	// ----------------------------------------------------------------
//	//  Initialize
//	// ----------------------------------------------------------------
//    override public void Initialize (BoardView _myBoardView, Tile bo) {
//        MyCrate = bo as Crate;
//        base.Initialize (_myBoardView, bo);
        
//        // Auto-move visuals!
//        if (MyCrate.DoAutoMove) {
//            i_body.color = new ColorHSB(0.5f, 0.3f, 1f).ToColor();
//        }
//        else {
//            Destroy(i_autoMoveDir.gameObject);
//            i_autoMoveDir = null;
//        }
//	}


//    // ----------------------------------------------------------------
//    //  Update Visuals
//    // ----------------------------------------------------------------
//    public override void UpdateVisualsPostMove() {
//        base.UpdateVisualsPostMove();
        
//        if (MyCrate.DoAutoMove) {
//            bool isArrow = MyCrate.AutoMoveDir!=Vector2Int.zero;
//            i_autoMoveDir.enabled = isArrow;
//            if (isArrow) {
//                float dirRot = MathUtils.GetSide(MyCrate.AutoMoveDir) * -90;
//                i_autoMoveDir.transform.localEulerAngles = new Vector3(0, 0, dirRot);
//            }
//        }
//    }



//}

