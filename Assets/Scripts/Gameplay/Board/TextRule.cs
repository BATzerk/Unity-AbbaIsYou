using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Move these outta here?
public enum RuleOperator {
    Undefined,
    IsYou, IsPush, IsStop
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
        return new TextRule(start.MySubject, end.MyOperator);
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
    public System.Type MySubject { get; private set; }
    public RuleOperator MyOperator { get; private set; }
    
    
    // Constructor
    public TextRule(System.Type _mySubject, RuleOperator _myOperator) {
        this.MySubject = _mySubject;
        this.MyOperator = _myOperator;
    }
    
    
    // Doers
    public void ApplyToBoard(Board b) {
        foreach (Tile obj in b.allTiles) {
            if (obj.GetType() != MySubject) { continue; } // Skip tiles that aren't my subject.
            switch (MyOperator) {
                case RuleOperator.IsPush: obj.IsPush = true; break;
                case RuleOperator.IsStop: obj.IsStop = true; break;
                case RuleOperator.IsYou: obj.IsYou = true; break;
            }
        }
    }
}
