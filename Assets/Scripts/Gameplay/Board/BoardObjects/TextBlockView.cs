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
    
    //private string GetDisplayText() {
    //    switch (MyTextBlock.MyTextType) {
    //        case TextType.Abba: return "Abba";
    //        case TextType.Crate: return "Crate";
            
    //        case TextType.Is: return "=";
            
    //        case TextType.Push: return "push";
    //        case TextType.Stop: return "stop";
    //        case TextType.You: return "you";
    //        default: return "UNDEFINED";
    //    }
    //}


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
    override public void Initialize (BoardView _myBoardView, Tile bo) {
        MyTextBlock = bo as TextBlock;
        base.Initialize (_myBoardView, bo);
        
        // Set text!
        //myText.text = GetDisplayText();
        i_myIcon.sprite = ResourcesHandler.Instance.GetTextBlockViewIconSprite(MyTextBlock.MyTextType);
        // HACK hardcoded color some sprites.
        switch (MyTextBlock.MyTextType) {
            case TextType.Abba: i_myIcon.color = new Color255(100,190,155).ToColor(); break;
            case TextType.Crate: i_myIcon.color = new Color255(209,188,159).ToColor(); break;
        }
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

