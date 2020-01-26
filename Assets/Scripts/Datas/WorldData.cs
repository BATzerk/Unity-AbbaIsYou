using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

//[System.Serializable]
public class WorldData {
	// LevelDatas
	private Dictionary<string, LevelData> levelDatas_dict; // by levelKey. ALL level datas in this world! Loaded up when WE'RE loaded up.
	private List<LevelData> levelDatas_list; // by levelIndex. ALL level datas in this world! Loaded up when WE'RE loaded up.
	private LevelOrder levelOrder; // this one class keeps track of how my levels are arranged within this world.
	// Properties
	private bool isWorldUnlocked; // if false, we won't be selectable in WorldSelect.
	private int worldIndex; // starts at 0.
	private int numPlayableLevels; // how many levels we have until we hit "EmptyLevel"!!


	// ----------------------------------------------------------------
	//  Getters
	// ----------------------------------------------------------------
	public bool IsWorldUnlocked { get { return isWorldUnlocked; } }
	public int NumLevels { get { return levelDatas_list.Count; } }
	public int NumPlayableLevels { get { return numPlayableLevels; } }
	public int WorldIndex { get { return worldIndex; } }
	public LevelOrder LevelOrder { get { return levelOrder; } }
//	public Dictionary<string, LevelData> LevelDatas { get { return levelDatas; } }

	public LevelData GetLevelData (string key) {
		if (levelDatas_dict.ContainsKey(key)) { return levelDatas_dict [key]; }
		else { return null; }
	}
	// TEMPORARY during the level system transition!
	public LevelData GetLevelData (int index) {
		if (index<0 || index>levelDatas_list.Count-1) { return null; } // Outta bounds.
		return levelDatas_list[index];
	}


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public WorldData (int _worldIndex) {
		worldIndex = _worldIndex;

		isWorldUnlocked = true;//SaveStorage.GetInt (SaveKeys.IsWorldUnlocked (worldIndex)) == 1;

		LoadAllLevelDatas ();
		levelOrder = new LevelOrder(worldIndex);
		CalculateNumPlayableLevels ();
		UpdateLevelDatasIsLocked ();
	}

	private void CalculateNumPlayableLevels () {
		numPlayableLevels = 0;
		for (int i=0; i<levelDatas_list.Count; i++) {
			if (levelDatas_list[i].levelKey == "EmptyLevel") { break; } // DEBUG temporary (maybe?). If this is the "EmptyLevel", then stop adding LevelTiles for this world.
			numPlayableLevels ++;
		}
	}
	public void UpdateLevelDatasIsLocked () {
		int unlockedIncompletedLevelsLeft = 3; // make the first X lvls that we HAVEN'T beaten unlocked!
		for (int i=0; i<levelDatas_list.Count; i++) {
			if (levelDatas_list[i].DidCompleteLevel) { // If we BEAT the level, then of course it's unlocked.
				levelDatas_list[i].isLocked = false;
			}
			else if (unlockedIncompletedLevelsLeft > 0) { // If we still have unlocked-ness-es to give out, give this level unlocked-ness!
				unlockedIncompletedLevelsLeft --;
				levelDatas_list[i].isLocked = false;
				// SPECIAL CASE: If this is the FIRST level, then FORCE us to start only with this level.
				if (i==0) {
					unlockedIncompletedLevelsLeft = 0;
				}
			}
			else { // We haven't beaten this level AND we're out of unlocked-ness to give out. It's locked!
				levelDatas_list[i].isLocked = true;
			}
		}
	}

	// ----------------------------------------------------------------
	//  LevelDatas
	// ----------------------------------------------------------------
	/** Makes a LevelData for every level file in our world's levels folder!! */
	private void LoadAllLevelDatas () {
		string filePath = FilePaths.LevelsFileXML(worldIndex);
//		string levelsFileString = Resources.Load<TextAsset> (filePath).text;

		XmlSerializer serializer = new XmlSerializer(typeof(WorldDataXML));
//		System.IO.FileStream stream = new System.IO.FileStream (filePath, System.IO.FileMode.Open);
		System.IO.FileStream stream = System.IO.File.OpenRead (filePath);
		WorldDataXML worldDataXML = serializer.Deserialize(stream) as WorldDataXML;
		stream.Close();

		// Convert the XML to LevelDatas!
		levelDatas_dict = new Dictionary<string, LevelData>();
		levelDatas_list = new List<LevelData>();
		for (int i=0; i<worldDataXML.levelDataXMLs.Count; i++) {
			LevelData newLD = new LevelData (worldIndex, i, worldDataXML.levelDataXMLs[i]);
			AddLevelData (newLD);
		}
	}
	private void AddLevelData (LevelData newLD) {
		if (!levelDatas_dict.ContainsKey(newLD.levelKey)) {
			levelDatas_dict.Add (newLD.levelKey, newLD);
			levelDatas_list.Add (newLD);
		}
		// If w already have a lvl with this key, add it with an added suffix!
		else {
			Debug.LogError ("Oops! We've already added a level with this key in this world. World: " + worldIndex + ", " + newLD.levelKey);
			newLD.levelKey = newLD.levelKey + "1";
			AddLevelData (newLD);
		}
	}

	// Events
	public void OnCompleteLevel (string levelKey, int numMovesMade) {
		LevelData ld = GetLevelData(levelKey);
		bool isFirstTimeCompleted = !ld.DidCompleteLevel;
		// Save stats!
		ld.UpdateBestNumMoves (numMovesMade);
		// Did we just beat this level for the first time?? Update all levels' isLocked!
		if (isFirstTimeCompleted) {
			UpdateLevelDatasIsLocked ();
		}
	}




}





