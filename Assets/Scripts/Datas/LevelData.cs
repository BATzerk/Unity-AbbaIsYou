using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LevelData {
	// Properties
	public BoardData boardData;
	public bool isBonus;
	public bool isLocked; // this is calculated by my WorldData.
	public int levelIndex;
	public int worldIndex;
	public int parMoves;
	public string levelKey;
	// Variable properties
	public int bestNumMoves;

	// Getters
	public bool DidCompleteLevel { get { return bestNumMoves > 0; } } // If I have ANY recorded moves on record, then yes, I've beaten this level! (I do this shortcut for optimization, so as to only store one value per lvl instead of two.)
	public bool DidAchieveParMoves { get { return DidCompleteLevel && bestNumMoves <= parMoves; } }

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public LevelData (int _worldIndex, int _levelIndex, LevelDataXML ldxml) {
		// Basic properties
		worldIndex = _worldIndex;
		levelIndex = _levelIndex;
		levelKey = ldxml.name;
		isBonus = ldxml.isBonus;
		parMoves = ldxml.parMoves;

		boardData = new BoardData (ldxml);

		// LOAD up stats!
		bestNumMoves = SaveStorage.GetInt (SaveKeys.BestNumMoves(worldIndex,levelKey));
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void UpdateBestNumMoves (int _numMoves) {
		if (bestNumMoves<=0 || bestNumMoves > _numMoves) {
			bestNumMoves = _numMoves;
			SaveStorage.SetInt (SaveKeys.BestNumMoves(worldIndex,levelKey), bestNumMoves);
		}
	}

}

