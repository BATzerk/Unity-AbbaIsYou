using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button_UndoMove : Button {
	// Properties
	private bool isButtonHeld = false; // Sigh. Unity's IsPressed() function isn't working.
	// References
	[SerializeField] private UndoMoveInputController undoMoveInputHandler;



	// ----------------------------------------------------------------
	//  Awake / Destroy
	// ----------------------------------------------------------------
	private void Awake () {
		// HACK TEMP
		undoMoveInputHandler = GameObject.FindObjectOfType<UndoMoveInputController>();

		// Add event listeners!
		GameManagers.Instance.EventManager.NumMovesMadeChangedEvent += OnNumMovesMadeChanged;
//		GameManagers.Instance.EventManager.StartGameAtLevelEvent += OnStartGameAtLevel;
	}
	private void OnDestroy () {
		// Remove event listeners!
		GameManagers.Instance.EventManager.NumMovesMadeChangedEvent -= OnNumMovesMadeChanged;
	}

	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	public void Update () {
		if (isButtonHeld) {
			undoMoveInputHandler.OnButton_Undo_Held ();
		}
	}

	// ----------------------------------------------------------------
	//  Button Events
	// ----------------------------------------------------------------
	private void OnNumMovesMadeChanged (int numMovesMade) {
		interactable = numMovesMade>0;
	}
	override public void OnPointerDown (UnityEngine.EventSystems.PointerEventData eventData) {
		undoMoveInputHandler.OnButton_Undo_Down ();
		isButtonHeld = true;
	}
	override public void OnPointerUp (UnityEngine.EventSystems.PointerEventData eventData) {
		undoMoveInputHandler.OnButton_Undo_Up ();
		isButtonHeld = false;
	}

}
