using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : BoardOccupant {

	public Crate (Board _boardRef, CrateData _data) {
		base.InitializeAsBoardOccupant (_boardRef, _data);
	}
	public CrateData SerializeAsData() {
		CrateData data = new CrateData (BoardPos);
		data.isMovable = IsMovable;
		data.isPassRotatable = IsPassRotatable;
		data.isSidePull = IsSidePull;
		return data;
	}

}



