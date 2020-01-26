using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager {
	// Properties
	private List<WorldData> worldDatas; // ALL worldDatas! Loaded up when program opens.



	// ----------------------------------------------------------------
	//  Getters
	// ----------------------------------------------------------------
	public WorldData GetWorldData (int worldIndex) {
		if (worldIndex<0 || worldIndex>=worldDatas.Count) { return null; }
		return worldDatas [worldIndex];
	}
	public LevelData GetLevelData (int worldIndex, string levelKey) {
		WorldData wd = GetWorldData(worldIndex);
		if (wd==null) { return null; } // No world?? Return null for the LevelData, then.
		return wd.GetLevelData (levelKey);
	}
	public bool DidAchieveParMoves (int worldIndex, string levelKey) {
		return GetWorldData(worldIndex).GetLevelData(levelKey).DidAchieveParMoves;
	}
	public bool DidCompleteLevel (int worldIndex, string levelKey) {
		return GetWorldData(worldIndex).GetLevelData(levelKey).DidCompleteLevel;
	}
	// TEMPORARY during code transition
	public LevelData GetLevelData (int worldIndex, int levelIndex) {
		WorldData wd = GetWorldData(worldIndex);
		if (wd==null) { return null; } // No world?? Return null for the LevelData, then.
		return wd.GetLevelData (levelIndex);
	}



	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public DataManager() {
		Reset ();
	}
	private void Reset () {
		ReloadWorldDatas ();
	}


	public void ReloadWorldDatas () {
		worldDatas = new List<WorldData> ();
		for (int i=0; i<GameProperties.NUM_WORLDS; i++) {
			WorldData newWorldData = new WorldData(i);
			worldDatas.Add (newWorldData);
		}
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void ClearAllSaveData() {
//		// What data do we wanna retain??
//		int controllerType = GameManagers.Instance.InputManager.ControllerType;

		// NOOK IT
		SaveStorage.DeleteAll ();
		Reset ();
		Debug.Log ("All SaveStorage CLEARED!");

//		// Pump back the data we retained!
//		GameManagers.Instance.InputManager.SetControllerType (controllerType);
	}

}




















