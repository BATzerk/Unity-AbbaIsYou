using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType {
    Undefined,
    Abba, Brick, Crate, Star, TextBlock,
    Is,
    You, Push, Stop, OverlapGoal, Destroys,
    
    If, Then, Else, Not, And, Or, True, False, Win, Lose, X, Y, Z,
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
            case TileType.Star:
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
            case TileType.Destroys:
                MyOperator = RuleOperator.IsDestroys;
                this.MyTextLoc = TextLoc.End;
                break;
            case TileType.OverlapGoal:
                MyOperator = RuleOperator.IsOverlapGoal;
                this.MyTextLoc = TextLoc.End;
                break;
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
                
            // TESTING If-Else
            case TileType.If:
            case TileType.Else:
            case TileType.Then:
            case TileType.Not:
            case TileType.And:
            case TileType.Or:
            case TileType.True:
            case TileType.False:
            case TileType.Win:
            case TileType.Lose:
            case TileType.X:
            case TileType.Y:
            case TileType.Z:
                MyOperator = RuleOperator.Undefined;
                this.MyTextLoc = TextLoc.Undefined;
                break;
                
            default: Debug.LogError("Oops, TextBlock doesn't have case handled for type: " + MySubjectType); break;
        }
    }
    
    // Serializing
    override public TileData ToData() {
        return new TextBlockData(MyGuid, BoardPos, MySubjectType);
    }





}
