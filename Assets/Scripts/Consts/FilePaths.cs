using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilePaths {
	public const string BOARD_OBJECTS = "Prefabs/Gameplay/BoardObjects/";

	static public string LevelsFileXML (int worldIndex) {
		return System.IO.Path.Combine (Application.streamingAssetsPath, "Levels/w" + worldIndex + "_Levels.xml");
//		return "Assets/Resources/" + "Data/
	}
	static public string LevelOrder (int worldIndex) {
		return System.IO.Path.Combine (Application.streamingAssetsPath, "Levels/w" + worldIndex + "_LevelOrder.txt");
//		return "Data/Levels/w" + worldIndex + "_LevelOrder";
	}

}
