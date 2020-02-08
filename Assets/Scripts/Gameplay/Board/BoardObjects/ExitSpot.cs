using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitSpot : Tile {

    // Serializing
    override public TileData ToData() {
        return new ExitSpotData (MyGuid, BoardPos);
    }

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public ExitSpot (Board _boardRef, ExitSpotData data) {
		base.InitializeAsTile (_boardRef, data);
		// Tell my BoardSpace I'm on it!
		MySpace.SetMyExitSpot (this);
	}


}
