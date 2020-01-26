using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeInstructions : MonoBehaviour {
	// Components
	[SerializeField] private Image i_swipeInstructions;
	// Properties
	private bool isSwipeInstructionsLevel;

	// ----------------------------------------------------------------
	//  Awake / Destroy
	// ----------------------------------------------------------------
	private void Awake () {
		// Add event listeners!
		GameManagers.Instance.EventManager.NumMovesMadeChangedEvent += OnNumMovesMadeChanged;
		GameManagers.Instance.EventManager.StartGameAtLevelEvent += OnStartGameAtLevel;
	}
	private void OnDestroy () {
		// Remove event listeners!
		GameManagers.Instance.EventManager.NumMovesMadeChangedEvent -= OnNumMovesMadeChanged;
		GameManagers.Instance.EventManager.StartGameAtLevelEvent -= OnStartGameAtLevel;
	}

	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void Update () {
		if (isSwipeInstructionsLevel) {
			// Move us about!
			i_swipeInstructions.rectTransform.anchoredPosition = new Vector3(Mathf.Sin(Time.time*5f)*20, 0);
		}
	}

	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnNumMovesMadeChanged (int numMovesMade) {
		if (!isSwipeInstructionsLevel) { return; }
		// Once we make a move, hide the instructions! We got this.
		if (numMovesMade > 0) {
			i_swipeInstructions.enabled = false;
		}
	}
	private void OnStartGameAtLevel (Level _level) {
		isSwipeInstructionsLevel = GameProperties.IsSwipeInstructionsLevel (_level);
		i_swipeInstructions.enabled = isSwipeInstructionsLevel;
	}
}
