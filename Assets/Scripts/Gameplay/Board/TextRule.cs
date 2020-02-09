using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Move these outta here?
public enum RuleOperator {
    Undefined,
    IsYou, IsPush, IsStop, IsOverlapGoal,
    Convert
}
public struct BlockSentence {
    // Properties
    public TextBlock start;
    public TextBlock middle;
    public TextBlock end;
    
    // Constructor
    public BlockSentence(TextBlock start, TextBlock middle, TextBlock end) {
        this.start = start;
        this.middle = middle;
        this.end = end;
    }
    
    // Getters
    public TextRule GetMyRule() {
        // IF my end is a START type, then it's a conversion! Return that.
        if (end.MyTextLoc == TextLoc.Start) {
            return new TextRule(start.MySubjectType, RuleOperator.Convert, end.MySubjectType);
        }
        return new TextRule(start.MySubjectType, end.MyOperator);
    }
    
    // Doers
    public void SetMyTextBlocksIsInSentenceToTrue() {
        start.IsInSentence = true;
        middle.IsInSentence = true;
        end.IsInSentence = true;
    }
}


public class TextRule {
    // Properties
    public TileType MySubject { get; private set; }
    public RuleOperator MyOperator { get; private set; }
    public TileType TypeToConvertTo { get; private set; } // ONLY if MyOperator is Convert.
    
    
    // Constructor
    public TextRule(TileType _mySubject, RuleOperator _myOperator, TileType _typeToConvertTo=TileType.Undefined) {
        this.MySubject = _mySubject;
        this.MyOperator = _myOperator;
        this.TypeToConvertTo = _typeToConvertTo;
    }
    
    
    // Doers
    public void ApplyToBoard(Board b) {
        foreach (Tile obj in b.allTiles) {
            if (obj.MyType != MySubject) { continue; } // Skip tiles that aren't my subject.
            switch (MyOperator) {
                case RuleOperator.IsOverlapGoal: obj.IsOverlapGoal = true; break;
                case RuleOperator.IsPush: obj.IsPush = true; break;
                case RuleOperator.IsStop: obj.IsStop = true; break;
                case RuleOperator.IsYou: obj.IsYou = true; break;
                case RuleOperator.Convert: obj.ConvertMyType(TypeToConvertTo); break;
                default: Debug.LogError("RuleOperator case not handled: " + MyOperator); break;
            }
        }
    }
}
