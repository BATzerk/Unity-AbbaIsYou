using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TextType {
    Undefined,
    Abba, Crate,
    Is,
    You, Push, Stop
}
public enum TextLoc {
    Undefined,
    Start, Middle, End
}

public class TextBlock : Tile {
    // Properties
    public TextType MyTextType { get; private set; }
    public TextLoc MyTextLoc { get; private set; }
    public System.Type MySubject { get; private set; } // ONLY exists for Starts.
    public RuleOperator MyOperator { get; private set; } // ONLY exists for Ends.
    public bool IsInSentence;
    
    

    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public TextBlock (Board _boardRef, TextBlockData _data) {
        base.InitializeAsTile (_boardRef, _data);
        this.MyTextType = _data.MyTextType;
        // Infer MyTextLoc and MySubject from my type!
        switch (MyTextType) {
            // Start
            case TextType.Abba:
                this.MyTextLoc = TextLoc.Start;
                this.MySubject = typeof(Abba);
                break;
            case TextType.Crate:
                this.MyTextLoc = TextLoc.Start;
                this.MySubject = typeof(Crate);
                break;
                
            // Middle
            case TextType.Is:
                this.MyTextLoc = TextLoc.Middle;
                break;
                
            // End
            case TextType.Push:
                MyOperator = RuleOperator.IsPush;
                this.MyTextLoc = TextLoc.End;
                break;
            case TextType.Stop:
                MyOperator = RuleOperator.IsStop;
                this.MyTextLoc = TextLoc.End;
                break;
            case TextType.You:
                MyOperator = RuleOperator.IsYou;
                this.MyTextLoc = TextLoc.End;
                break;
        }
    }
    
    // Serializing
    override public TileData ToData() {
        return new TextBlockData(MyGuid, BoardPos, MyTextType);
    }





}
