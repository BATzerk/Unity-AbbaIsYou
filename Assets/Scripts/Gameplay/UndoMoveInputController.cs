using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** For cleanliness. Handles what happens when we hold down the Undo button.
Key presses are handled internally; UI Undo-Button presses I'm told about by Button_UndoMove.cs. */
public class UndoMoveInputController : MonoBehaviour {
	// Properties
	private float undoLoc; // when this hits past 1, we say to undo a move (and reset its value)!
	private float undoVel;
	// References
	[SerializeField] private GameController gameControllerRef;



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		bool isButton_undo_held = Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.Delete) || Input.GetKey(KeyCode.Z);
		bool isButton_undo_up = Input.GetKeyUp(KeyCode.Backspace) || Input.GetKeyUp(KeyCode.Delete) || Input.GetKeyUp(KeyCode.Z);
		bool isButton_undo_down = Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Z);

		if (isButton_undo_up) { OnButton_Undo_Up (); }
		else if (isButton_undo_down) { OnButton_Undo_Down (); }
		else if (isButton_undo_held) { OnButton_Undo_Held (); }
	}


	// ----------------------------------------------------------------
	//  Button Events
	// ----------------------------------------------------------------
	public void OnButton_Undo_Held () {
		// Update vel
		undoVel += 0.006f;
		if (undoVel > 0.8f) { undoVel = 0.8f; } // Max vel!
		// Apply vel
		undoLoc += undoVel;
		// Maybe undo!
		if (undoLoc > 1) {
			undoLoc = 0; // reset it hard to 0, instead of just subtracting 1, so we DON'T preserve that extra bit of momentum; we want to definitely only allow one undo per frame.
			gameControllerRef.AttemptUndoMove ();
		}
	}
	public void OnButton_Undo_Up () {

	}
	public void OnButton_Undo_Down () {
		// Do the first undo!
		gameControllerRef.AttemptUndoMove ();
		// Reset dees.
		undoLoc = 0; // reset dis.
		undoVel = 0; // reset dis too.
	}


}
