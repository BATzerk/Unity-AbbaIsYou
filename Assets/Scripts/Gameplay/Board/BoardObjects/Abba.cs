using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abba : Tile {


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public Abba (Board _boardRef, AbbaData _data) {
		base.InitializeAsTile (_boardRef, _data);
        //IsDead = _data.isDead;
	}
    // Serializing
    override public TileData ToData() {
		AbbaData data = new AbbaData(MyGuid, BoardPos);//, IsDead);
		return data;
	}
    
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    //public void Die() {
    //    IsDead = true;
    //}



}
