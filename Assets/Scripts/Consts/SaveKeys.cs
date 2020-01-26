using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveKeys {
	public const string LAST_PLAYED_LEVEL_INDEX = "lastPlayedLevelIndex"; // the advantage of using Index instead of Key is that we can change a level's name.
	public const string LAST_PLAYED_LEVEL_KEY = "lastPlayedLevelKey";
	public const string LAST_PLAYED_WORLD_INDEX = "lastPlayedWorldIndex";

	public static string BestNumMoves (int worldIndex, string levelKey) { return "bestNumMoves_" + worldIndex + "_" + levelKey; }
}
