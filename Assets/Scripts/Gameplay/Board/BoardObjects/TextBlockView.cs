using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextBlockView : TileView {
    // Components
    //[SerializeField] private TextMeshProUGUI myText=null;
    [SerializeField] private Image i_myIcon=null;
    [SerializeField] private Image i_backing=null;
	// References
    public TextBlock MyTextBlock { get; private set; }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
    override public void Initialize (BoardView _myBoardView, Tile bo) {
        MyTextBlock = bo as TextBlock;
        base.Initialize (_myBoardView, bo);
        
        // Set text!
        i_myIcon.sprite = ResourcesHandler.Instance.GetTileSprite(MyTextBlock.MySubjectType);
	}


    // ----------------------------------------------------------------
    //  Update Visuals
    // ----------------------------------------------------------------
    public override void UpdateVisualsPostMove() {
        base.UpdateVisualsPostMove();
        
        // Set alpha based on IsInSentence!
        float iconAlpha = MyTextBlock.IsInSentence ? 1 : 0.5f;
        float backingAlpha = MyTextBlock.IsInSentence ? 1 : 0f;
        //myText.alpha = alpha;
        GameUtils.SetUIGraphicAlpha(i_myIcon, iconAlpha);
        GameUtils.SetUIGraphicAlpha(i_backing, backingAlpha);
    }



}

