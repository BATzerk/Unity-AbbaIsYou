using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectController : MonoBehaviour {
	// Constants
	private readonly Vector2 tileSize = new Vector2(120,120);
	private readonly Vector2 tileGap = new Vector2(0, 0);
//	private const float worldGapY = 100f; // from the bottom of the bottommost tile in w0 to the top of the topmost tile in w1.
	private const float levelClusterGapY = 16f; // between clusters of levels
	// Components
	[SerializeField] private Button b_prevWorld;
	[SerializeField] private Button b_nextWorld;
	[SerializeField] private TextMeshProUGUI t_worldName;
	[SerializeField] private Transform tf_levelTiles;
	private GameObject[] gos_tileWorldContainers; // all LevelTiles from w0 go in the 0th element, etc.
//	private Dictionary<string,LevelTile> allLevelTiles; // EVERY level tile by levelTileDictKey (aka worldIndex+"_"+levelKey)!
	private List<List<LevelTile>> allLevelTiles; // first index is World; second is levelIndex within world
	// References
	[SerializeField] private GameObject prefabGO_levelTile;
	// Properties
	private int selectedWorldIndex; // we view one world's set of tiles at a time.

	// Getters / Setters
	private DataManager dataManager { get { return GameManagers.Instance.DataManager; } }
	private int numWorlds { get { return GameProperties.NUM_WORLDS; } }

//	private string levelTileDictKey (int worldIndex, string levelKey) { return worldIndex + "_" + levelKey; }
//	private void SetLevelTile (int worldIndex, string levelKey, LevelTile lt) {
//		allLevelTiles[levelTileDictKey(worldIndex,levelKey)] = lt;
//	}
//	private LevelTile GetLevelTile (int worldIndex, string levelKey) {
//		string dictKey = levelTileDictKey(worldIndex,levelKey);
//		if (allLevelTiles.ContainsKey (dictKey)) { return allLevelTiles[dictKey]; }
//		Debug.LogError ("Can't find LevelTile with this key: " + dictKey + ". Make sure the name in LevelOrder.txt matches the name within the levels XML.");
//		return null;
//	}


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	private void Start () {
		MakeLevelTiles ();
		SetWorld (SaveStorage.GetInt (SaveKeys.LAST_PLAYED_WORLD_INDEX));

		// Add event listeners!
		GameManagers.Instance.EventManager.ScreenSizeChangedEvent += OnScreenSizeChanged;
	}
	private void OnDestroy () {
		// Remove event listeners!
		GameManagers.Instance.EventManager.ScreenSizeChangedEvent -= OnScreenSizeChanged;
	}
	private void MakeLevelTiles () {
		// Make the container GOs first.
		gos_tileWorldContainers = new GameObject[numWorlds];

		allLevelTiles = new List<List<LevelTile>>();// new Dictionary<string, LevelTile>();
		for (int wi=0; wi<numWorlds; wi++) {
			// Make the container.
			MakeTileWorldContainer (wi);
			// Make the tiles.
			allLevelTiles.Add (new List<LevelTile>());
			WorldData wd = dataManager.GetWorldData(wi);
			for (int li=0; li<wd.NumPlayableLevels; li++) {
				LevelData ld = wd.GetLevelData (li);
				AddLevelTile (ld);
			}
		}
		PositionLevelTiles ();
	}
	private void MakeTileWorldContainer (int worldIndex) {
		GameObject go = new GameObject();
		go.transform.SetParent (tf_levelTiles);
		go.name = "WorldTiles_" + worldIndex;
		RectTransform rt = go.AddComponent<RectTransform>();
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = rt.offsetMax = Vector2.zero;
		rt.transform.localScale = Vector3.one;
		rt.transform.localEulerAngles = Vector3.zero;
		gos_tileWorldContainers[worldIndex] = go;
	}
	private void AddLevelTile (LevelData ld) {
		LevelTile newTile = Instantiate(prefabGO_levelTile).GetComponent<LevelTile>();
		GameObject go_container = gos_tileWorldContainers[ld.worldIndex];
		newTile.Initialize (this, go_container.transform, ld);
//		SetLevelTile (ld.worldIndex,ld.levelKey, newTile);
		allLevelTiles[ld.worldIndex].Add (newTile);
	}

	private void PositionLevelTiles () {
		Vector2 levelTilesContainerSize = tf_levelTiles.GetComponent<RectTransform>().rect.size;
//		float containerWidth = ScreenHandler.RelativeScreenSize.x + levelTilesContainerSize.x; // let's use this value to determine how much horz. space I've got for the tiles.
		float containerWidth = levelTilesContainerSize.x; // let's use this value to determine how much horz. space I've got for the tiles.

		for (int wi=0; wi<numWorlds; wi++) {
			float tempX = tileGap.x; // where we're putting things! Added to as we go along.
			float tempY = -tileGap.y; // where we're putting things! Added to as we go along.

			int numLevels = allLevelTiles[wi].Count;
			bool temp_didBonusLevelsRowBreak = false; // this is temporary, as I'm still undecided how bonus levels will be arranged (so I'm not gonna do this code elaborately and "properly").
			for (int li=0; li<numLevels; li++) {
				LevelTile lt = allLevelTiles[wi][li];

				// Temporary code to do a row break for the bonus levels.
				if (!temp_didBonusLevelsRowBreak && lt.IsBonus) {
					tempX = tileGap.x;
					tempY -= tileSize.y+tileGap.y + 30f; // extra gap for bonus lvls.
					temp_didBonusLevelsRowBreak = true; // yep, we've done the break; only do it this once per world!
				}

				lt.SetPosSize (tempX,tempY, tileSize);
				tempX += tileSize.x+tileGap.x;
				if (tempX+tileSize.x > containerWidth) { // wrap to the next row
					tempX = tileGap.x;
					tempY -= tileSize.y+tileGap.y;
				}
			}
			tempX = tileGap.x;
			tempY -= tileSize.y+levelClusterGapY;
		}
	}
	/*
	private void PositionLevelTiles () {
		for (int wi=0; wi<numWorlds; wi++) {
			float tempX = tileGap.x; // where we're putting things! Added to as we go along.
			float tempY = -tileGap.y; // where we're putting things! Added to as we go along.

			LevelOrder levelOrder = dataManager.GetWorldData(wi).LevelOrder;
			for (int ci=0; ci<levelOrder.NumClusters; ci++) {
				int numLevelsInCluster = levelOrder.NumLevelsInCluster (ci);
				for (int indexInCluster=0; indexInCluster<numLevelsInCluster; indexInCluster++) {
					string levelKey = levelOrder.GetLevelKey (ci, indexInCluster);
					LevelTile lt = GetLevelTile (wi, levelKey);
					lt.SetPosSize (tempX,tempY, tileSize);
					tempX += tileSize.x+tileGap.x;
					if (tempX+tileSize.x > ScreenHandler.RelativeScreenSize.x) { // wrap to the next row
						tempX = tileGap.x;
						tempY -= tileSize.y+tileGap.y;
					}
				}
				tempX = tileGap.x;
				tempY -= tileSize.y+levelClusterGapY;
			}
//			tempX = tileGap.x;
//			tempY -= worldGapY-levelClusterGapY;
		}
	}
	*/


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void LoadLevel (int worldIndex, int levelIndex) {
		SaveStorage.SetInt (SaveKeys.LAST_PLAYED_WORLD_INDEX, worldIndex);
		SaveStorage.SetInt (SaveKeys.LAST_PLAYED_LEVEL_INDEX, levelIndex);
		UnityEngine.SceneManagement.SceneManager.LoadScene (SceneNames.Gameplay);
	}
	public void SetWorld_Prev () { SetWorld (selectedWorldIndex-1); }
	public void SetWorld_Next () { SetWorld (selectedWorldIndex+1); }
	private void SetWorld (int _worldIndex) {
		// Set the value!
		selectedWorldIndex = _worldIndex;
		// Update containers' visibilities!
		for (int i=0; i<gos_tileWorldContainers.Length; i++) {
			bool isVisible = i==selectedWorldIndex;
			gos_tileWorldContainers[i].SetActive (isVisible);
		}
		// Update header text!
		t_worldName.text = "World " + selectedWorldIndex;
		// Update world-selecting buttons!
		UpdateWorldSelectButtons ();
	}
	private void UpdateWorldSelectButtons () {
		b_prevWorld.interactable = selectedWorldIndex>GameProperties.FIRST_WORLD_INDEX;
		b_nextWorld.interactable = selectedWorldIndex<numWorlds-1;
	}

	private void OpenScene_LevelSelect () {
		UnityEngine.SceneManagement.SceneManager.LoadScene (SceneNames.LevelSelect);
	}
	private void ClearAllSaveDataAndReloadScene () {
		GameManagers.Instance.DataManager.ClearAllSaveData ();
		OpenScene_LevelSelect ();
	}

	// Events
	private void OnScreenSizeChanged () {
		PositionLevelTiles ();
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		bool isKey_control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		bool isKey_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

		// DEBUG
		if (Input.GetKeyDown(KeyCode.U)) {
			Debug_UnlockAllLevelTiles ();
		}
		if (Input.GetKeyDown(KeyCode.Return)) {
			OpenScene_LevelSelect ();
			return;
		}
		if (isKey_control && isKey_shift && Input.GetKeyDown(KeyCode.Delete)) {
			ClearAllSaveDataAndReloadScene ();
			return;
		}
	}
	public void Debug_UnlockAllLevelTiles () {
		foreach (List<LevelTile> list in allLevelTiles) {
			foreach (LevelTile lt in list) {
				lt.Debug_UnlockMe ();
			}
		}
	}


}
