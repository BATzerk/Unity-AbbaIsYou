using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayUI : MonoBehaviour {
	// Components
	[SerializeField] private Button b_nextLevel; // shows up when we complete the level!
	[SerializeField] private Button b_restartLevel;
	[SerializeField] private Button b_undoMove;
	[SerializeField] private Text t_levelIndex;
	[SerializeField] private Text t_levelKey;//MeshProUGUI

	// ----------------------------------------------------------------
	//  Awake / Destroy
	// ----------------------------------------------------------------
	private void Awake () {
		// Add event listeners!
		GameManagers.Instance.EventManager.SetIsLevelCompletedEvent += OnSetIsLevelCompleted;
		GameManagers.Instance.EventManager.StartGameAtLevelEvent += OnStartGameAtLevel;
	}
	private void OnDestroy () {
		// Remove event listeners!
		GameManagers.Instance.EventManager.SetIsLevelCompletedEvent -= OnSetIsLevelCompleted;
		GameManagers.Instance.EventManager.StartGameAtLevelEvent -= OnStartGameAtLevel;
	}

	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnSetIsLevelCompleted (bool isLevelCompleted) {
		// Enable/disable thingies
		b_nextLevel.gameObject.SetActive (isLevelCompleted);
	}
	private void OnStartGameAtLevel (Level _level) {
		// Show my elements!
		t_levelIndex.enabled = true;
		t_levelKey.enabled = true;
		// Update level text!
		t_levelIndex.text = _level.WorldIndex + "-" + _level.LevelIndex;
		t_levelKey.text = _level.LevelKey;
		t_levelIndex.color = _level.IsBonus ? new Color(1, 0.8f, 0.1f, 0.6f) : new Color(1,1,1, 0.4f);
		// Hide level-completed;-next-level button!
		b_nextLevel.gameObject.SetActive (false);

		// Hey, if this is a special level, do special things instead!
		bool isSwipeInstructionsLevel = GameProperties.IsSwipeInstructionsLevel (_level);
		b_undoMove.gameObject.SetActive (!isSwipeInstructionsLevel);
		b_restartLevel.gameObject.SetActive (!isSwipeInstructionsLevel);
	}

}
