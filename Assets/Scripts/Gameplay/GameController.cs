using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	// Properties
	private bool debug_isSlowMo = false;
	private bool isPaused = false;
	// Objects
	private Level currentLevel;
	// References
	[SerializeField] private GameObject go_levelPrefab;
	[SerializeField] private GameCameraController cameraController;
	[SerializeField] private Transform tf_world;

	// Getters / Setters
	public Level CurrentLevel { get { return currentLevel; } }

	private int currentWorldIndex { get { return currentLevel.WorldIndex; } }
	private int currentLevelIndex { get { return currentLevel.LevelIndex; } }
	private string currentLevelKey { get { return currentLevel.LevelKey; } }
	private InputController inputController { get { return InputController.Instance; } }
	private WorldData CurrentWorldData { get { return GameManagers.Instance.DataManager.GetWorldData(currentWorldIndex); } }



	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	private void Start () {
		// Set application values
		Application.targetFrameRate = GameVisualProperties.TARGET_FRAME_RATE;

		// Start at the level we've most recently played!
		int _worldIndex = SaveStorage.GetInt (SaveKeys.LAST_PLAYED_WORLD_INDEX);
		if (SaveStorage.HasKey (SaveKeys.LAST_PLAYED_LEVEL_INDEX)) { // If we've ever played a level before, start with the most recent one played!
			int _levelIndex = SaveStorage.GetInt (SaveKeys.LAST_PLAYED_LEVEL_INDEX);
			StartGameAtLevel (_worldIndex, _levelIndex);
		}
		else { // Otherwise, just start at the first level in this world.
			StartGameAtLevel (_worldIndex, 0);
		}
	}



	// ----------------------------------------------------------------
	//  Doers - Loading Level
	// ----------------------------------------------------------------
	private void ReloadScene () { OpenScene (SceneNames.Gameplay); }
	public void OpenScene_LevelSelect () { OpenScene (SceneNames.LevelSelect); }
	private void OpenScene (string sceneName) { StartCoroutine (OpenSceneCoroutine (sceneName)); }
	private IEnumerator OpenSceneCoroutine (string sceneName) {
		// First, manually destroy currentLevel if it exists so that its event listeners get removed.
		DestroyCurrentLevel ();

		// First frame: Blur it up.
		cameraController.DarkenScreenForSceneTransition ();
		yield return null;

		// Second frame: Load up that business.
		UnityEngine.SceneManagement.SceneManager.LoadScene (sceneName);
	}

	public void RestartCurrentLevel () { StartGameAtLevel (currentWorldIndex, currentLevelIndex); }
	public void StartPreviousLevel () { StartGameAtLevel (currentWorldIndex, currentLevelIndex-1); }
	public void StartNextLevel () {
		if (currentLevelIndex+1>=CurrentWorldData.NumPlayableLevels) { OpenScene_LevelSelect (); } // If we've reached the end of this world, then boot us back to LevelSelect!
		else { StartGameAtLevel (currentWorldIndex, currentLevelIndex+1); }
	}
	private void Debug_StartGameAtWorld (int _worldIndex) {
		// Don't go outta bounds.
		_worldIndex = Mathf.Clamp (_worldIndex, 0, GameProperties.NUM_WORLDS);
		StartGameAtLevel (_worldIndex, 0);
	}
	private void StartGameAtLevel (int worldIndex, int levelIndex) { StartGameAtLevel (GameManagers.Instance.DataManager.GetLevelData (worldIndex, levelIndex)); }
	private void StartGameAtLevel (int worldIndex, string levelKey) { StartGameAtLevel (GameManagers.Instance.DataManager.GetLevelData (worldIndex, levelKey)); }
	private void StartGameAtLevel (LevelData ld) {
		if (ld == null) {
			Debug.LogError ("Can't load the requested level! Can't find its LevelData.");
			if (currentLevel == null) { // If there's no currentLevel, yikes! Default us to w0 l0.
				ld = GameManagers.Instance.DataManager.GetLevelData (0, 0);
			}
			else { return; } // If there IS a currentLevel, then don't leave it!
		}
		StartCoroutine (StartGameAtLevelCoroutine (ld));
	}
	private void TEMP_ReloadAllLevelDatasFromFile () {
//		string saveLocation = "Assets/Resources/" + FilePaths. (worldIndex, levelKey);
//		string fileName = roomKey + ".txt";

		// Reload the text file right away!! (Otherwise, we'll have to ALT + TAB out of Unity and back in for it to be refreshed.)
//		#if UNITY_EDITOR
//		UnityEditor.AssetDatabase.ImportAsset ("Assets/Resources/Data/Levels");
//		#endif

		GameManagers.Instance.DataManager.ReloadWorldDatas ();
	}
	/** This actually shows "Loading" overlay FIRST, THEN next frame loads the world. */
	private IEnumerator StartGameAtLevelCoroutine (LevelData ld) {
		// TEMP!! reload all levels from file
		TEMP_ReloadAllLevelDatasFromFile ();

		// Show "Loading" overlay!
//		gameHUDRef.ShowLoadingOverlay ();
		yield return null;

		// Reset some values
		DestroyCurrentLevel ();

		// Instantiate the Level from the provided LevelData!
		currentLevel = ((GameObject) Instantiate (go_levelPrefab)).GetComponent<Level> ();
		currentLevel.Initialize (this, tf_world, ld);
		SaveStorage.SetInt (SaveKeys.LAST_PLAYED_LEVEL_INDEX, currentLevelIndex);
		SaveStorage.SetString (SaveKeys.LAST_PLAYED_LEVEL_KEY, currentLevelKey);
		SaveStorage.SetInt (SaveKeys.LAST_PLAYED_WORLD_INDEX, currentWorldIndex);

		// Reset camera!
		cameraController.Reset ();
		// Dispatch event!
		GameManagers.Instance.EventManager.OnStartGameAtLevel (currentLevel);

		yield return null;
	}

	private void DestroyCurrentLevel () {
		if (currentLevel != null) {
			currentLevel.DestroySelf ();
			currentLevel = null;
		}
	}



	// ----------------------------------------------------------------
	//  Doers - Gameplay
	// ----------------------------------------------------------------
	private void TogglePause () {
		isPaused = !isPaused;
		UpdateTimeScale ();
	}
	private void UpdateTimeScale () {
		if (isPaused) { Time.timeScale = 0; }
		else if (debug_isSlowMo) { Time.timeScale = 0.1f; }
		else { Time.timeScale = 1; }
	}
	public void AttemptUndoMove () {
		currentLevel.UndoLastMove ();
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		RegisterButtonInput ();
	}
	private void RegisterButtonInput () {
		bool isKey_alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		bool isKey_control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

		// ~~~~ DEBUG ~~~~
		// ALT + ___
		if (isKey_alt) {
			// Skipping through worlds!
			if (Input.GetKeyDown(KeyCode.LeftBracket)) { Debug_StartGameAtWorld(currentWorldIndex-1); return; }
			else if (Input.GetKeyDown(KeyCode.RightBracket)) { Debug_StartGameAtWorld(currentWorldIndex+1); return; }
		}
		// CONTROL + ___
		if (isKey_control) {
			// CONTROL + L = Toggle slow-mo!
			if (Input.GetKeyDown(KeyCode.L)) { Debug_ToggleSlowMo (); }
		}
		// Printing!
		if (Input.GetKeyDown(KeyCode.I) && currentLevel!=null) { currentLevel.Debug_PrintBoardAttributes (); }
		// Skipping through levels!
		if (Input.GetKeyDown(KeyCode.LeftBracket)) { StartPreviousLevel(); return; }
		if (Input.GetKeyDown(KeyCode.RightBracket)) { StartNextLevel(); return; }



		// ENTER = Reset current level (by reloading this scene).
		if (Input.GetKeyDown (KeyCode.Return)) {
			ReloadScene ();
			return;
		}
		// SPACE
		else if (Input.GetKeyDown (KeyCode.Space)) {
			if (currentLevel!=null && currentLevel.IsLevelComplete) {
				StartNextLevel ();
			}
		}
		// P = Toggle pause
		else if (Input.GetKeyDown (KeyCode.P)) {
			TogglePause ();
		}
		// ESC = Open LevelSelect
		else if (Input.GetKeyDown (KeyCode.Escape)) {
			OpenScene_LevelSelect ();
		}

		// We DO have currentLevel!
		if (currentLevel != null) {
			// Player dead? Any input to undo last move.
			if (Input.anyKeyDown && currentLevel.IsEveryPlayerDead()) {
				currentLevel.UndoLastMove ();
				return;
			}

			// Player movement!
			if (inputController != null) { // Note: This is only here to prevent errors when recompiling code during runtime.
				currentLevel.UpdateSimulatedMove (inputController.SimulatedMoveDir, inputController.SimulatedMovePercent);
//				if (inputController.IsTouchDown) { currentLevel.OnVJTouchDown(); }
				if 		(inputController.IsPlayerMove_L ())  { currentLevel.MovePlayerInstances (-1, 0); }
				else if (inputController.IsPlayerMove_R ())  { currentLevel.MovePlayerInstances ( 1, 0); }
				else if (inputController.IsPlayerMove_D ())  { currentLevel.MovePlayerInstances ( 0, 1); }
				else if (inputController.IsPlayerMove_U ())  { currentLevel.MovePlayerInstances ( 0,-1); }
			}
		}
	}

	private void Debug_ToggleSlowMo () {
		debug_isSlowMo = !debug_isSlowMo;
		UpdateTimeScale ();
	}





}






