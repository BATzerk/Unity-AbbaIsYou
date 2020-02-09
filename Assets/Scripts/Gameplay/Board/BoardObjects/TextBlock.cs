using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType {
    Undefined,
    Abba, Brick, Crate, ExitSpot, TextBlock,
    Is,
    You, Push, Stop
}
public enum TextLoc {
    Undefined,
    Start, Middle, End
}

public class TextBlock : Tile {
    // Properties
    public TileType MySubjectType { get; private set; }
    public TextLoc MyTextLoc { get; private set; }
    //public TileType MySubject { get; private set; } // ONLY exists for Starts.
    public RuleOperator MyOperator { get; private set; } // ONLY exists for Ends.
    public bool IsInSentence;
    
    

    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public TextBlock (Board _boardRef, TextBlockData _data) {
        base.InitializeAsTile (_boardRef, _data);
        this.MyType = _data.MyType;
        this.MySubjectType = _data.MySubjectType;
        // Infer MyTextLoc and MySubject from my type!
        switch (MySubjectType) {
            // Start
            case TileType.Abba:
            case TileType.Brick:
            case TileType.Crate:
            case TileType.ExitSpot:
                this.MyTextLoc = TextLoc.Start;
                break;
            //case TileType.Abba:
            //    this.MyTextLoc = TextLoc.Start;
            //    this.MySubject = TileType.Abba;
            //    break;
            //case TileType.Crate:
                //this.MyTextLoc = TextLoc.Start;
                //this.MySubject = TileType.Crate;
                //break;
                
            // Middle
            case TileType.Is:
                this.MyTextLoc = TextLoc.Middle;
                break;
                
            // End
            case TileType.Push:
                MyOperator = RuleOperator.IsPush;
                this.MyTextLoc = TextLoc.End;
                break;
            case TileType.Stop:
                MyOperator = RuleOperator.IsStop;
                this.MyTextLoc = TextLoc.End;
                break;
            case TileType.You:
                MyOperator = RuleOperator.IsYou;
                this.MyTextLoc = TextLoc.End;
                break;
        }
    }
    
    // Serializing
    override public TileData ToData() {
        return new TextBlockData(MyGuid, BoardPos, MySubjectType);
    }





}
