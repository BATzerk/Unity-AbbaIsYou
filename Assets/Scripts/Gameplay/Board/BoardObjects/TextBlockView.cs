using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextBlockView : TileView {
    // Components
    [SerializeField] private TextMeshProUGUI myText=null;
	// References
    public TextBlock MyTextBlock { get; private set; }
    
    private string GetDisplayText() {
        switch (MyTextBlock.MyTextType) {
            case TextType.Abba: return "Abba";
            case TextType.Crate: return "Crate";
            
            case TextType.Is: return "=";
            
            case TextType.Push: return "push";
            case TextType.Stop: return "stop";
            case TextType.You: return "you";
            default: return "UNDEFINED";
        }
    }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
    override public void Initialize (BoardView _myBoardView, Tile bo) {
        MyTextBlock = bo as TextBlock;
        base.Initialize (_myBoardView, bo);
        
        // Set text!
        myText.text = GetDisplayText();
	}


    // ----------------------------------------------------------------
    //  Update Visuals
    // ----------------------------------------------------------------
    public override void UpdateVisualsPostMove() {
        base.UpdateVisualsPostMove();
        
        // Set alpha based on IsInSentence!
        float alpha = MyTextBlock.IsInSentence ? 1 : 0.5f;
        myText.alpha = alpha;
    }



}

