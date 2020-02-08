using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TextType {
    Undefined,
    Abba, Crate,
    Is,
    You, Push, Stop
}

public class TextBlock : Tile {
    // Properties
    public TextType MyTextType { get; private set; }
    public System.Type MySubject { get; private set; }
    

    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public TextBlock (Board _boardRef, TextBlockData _data) {
        base.InitializeAsTile (_boardRef, _data);
        this.MyTextType = _data.MyTextType;
        //this.MySubject = _data.MySubject;
    }
    
    // Serializing
    override public TileData ToData() {
        return new TextBlockData(MyGuid, BoardPos, MyTextType);
    }





}
