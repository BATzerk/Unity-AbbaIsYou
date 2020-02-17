using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericTileView : TileView {
    // Components
    [SerializeField] private Image i_body=null;
    //[SerializeField] private Image i_autoMoveDir=null;
	// References
    public GenericTile MyGenericTile { get; private set; }
    // Properties
    private bool isOverlapGoal;
    private bool isGoalSatisfied;


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
    override public void Initialize (BoardView _myBoardView, Tile bo) {
        MyGenericTile = bo as GenericTile;
        base.Initialize (_myBoardView, bo);
        gameObject.name = "Tile_" + bo.MyType;
    }
    
    private void UpdateBodyImage() {
        // Set sprite!
        i_body.sprite = ResourcesHandler.Instance.GetTileSprite(MyGenericTile.MyType);
        
        // Set sorting order! HARDCODED.
        Canvas myCanvas = GetComponent<Canvas>();
        switch(MyGenericTile.MyType) {
            case TileType.Abba: myCanvas.sortingOrder = 40; break;
            case TileType.Brick: myCanvas.sortingOrder = 2; break;
            case TileType.Crate: myCanvas.sortingOrder = 5; break;
            case TileType.ExitSpot: myCanvas.sortingOrder = 50; break;
            // NOTE: TextBlocks have TextBlockView, so their case is not handled here.
            default: Debug.LogWarning("Yo! TileType not handled for sorting order in GenericTileView.cs. Type: " + MyGenericTile.MyType); break;
        }
	}


    // ----------------------------------------------------------------
    //  Update Visuals
    // ----------------------------------------------------------------
    public override void UpdateVisualsPostMove() {
        base.UpdateVisualsPostMove();
        
        UpdateBodyImage();
        
        //if (MyCrate.DoAutoMove) {
        //    bool isArrow = MyCrate.AutoMoveDir!=Vector2Int.zero;
        //    i_autoMoveDir.enabled = isArrow;
        //    if (isArrow) {
        //        float dirRot = MathUtils.GetSide(MyCrate.AutoMoveDir) * -90;
        //        i_autoMoveDir.transform.localEulerAngles = new Vector3(0, 0, dirRot);
        //    }
        //}
        
        // Reset rotation and scale.
        i_body.transform.localScale = Vector3.one;
        i_body.transform.localEulerAngles = Vector3.zero;
        
        isOverlapGoal = MyGenericTile.IsOverlapGoal;
        isGoalSatisfied = MyGenericTile.IsOverlapGoalSatisfied();
        float alpha = isOverlapGoal&&!isGoalSatisfied ? 0.6f : 1f;
        GameUtils.SetUIGraphicAlpha(i_body, alpha);
        //// Put OverlapGoals above other things!
        //if (isOverlapGoal) {
        //    this.transform.SetAsFirstSibling();
        //}
    }

    private void Update() {
        if (isOverlapGoal) {
            if (isGoalSatisfied) {
                float rot = ((MyGenericTile.Col+MyGenericTile.Row + Time.time)*300) % 360;
                i_body.transform.localEulerAngles = new Vector3(0, 0, rot);
                i_body.transform.localScale = Vector3.one;
            }
            else {
                float rot = ((MyGenericTile.Col+MyGenericTile.Row + Time.time)*30) % 360;
                i_body.transform.localEulerAngles = new Vector3(0, 0, rot);
                float sinLoc = (Time.time+MyGenericTile.Col+MyGenericTile.Row) * 5f;
                i_body.transform.localScale = Vector3.one * MathUtils.SinRange(0.85f,1.05f, sinLoc);
            }
        }
    }



}

