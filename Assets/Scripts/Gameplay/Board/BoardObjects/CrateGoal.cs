using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateGoal : Tile, IGoalObject {
	// Properties
    public bool IsOn { get; private set; }
    public bool DoStayOn { get; private set; }
    
    
    // Serializing
    override public TileData ToData() {
        return new CrateGoalData (BoardPos, DoStayOn, IsOn);
    }
	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public CrateGoal (Board _boardRef, CrateGoalData data) {
		base.InitializeAsTile (_boardRef, data);
        DoStayOn = data.doStayOn;
        IsOn = data.isOn;
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void UpdateIsOn () {
        if (DoStayOn) { // I STAY on? It's an OR condition!
            IsOn |= GetIsSatisfied();
        }
        else { // I DON'T stay on. Only on if it's true now.
            IsOn = GetIsSatisfied();
        }
    }
    private bool GetIsSatisfied() {
        foreach (Tile obj in MySpace.MyTiles) {
            if (obj is Crate) {
                return true;
            }
        }
        return false;
	}

}
