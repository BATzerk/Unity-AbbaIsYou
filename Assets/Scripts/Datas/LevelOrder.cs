using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOrder {
	// Constants
	private readonly string[] LEVEL_KEY_SEPARATORS = new string[] { ", " };
	// Properties
	private List<List<string>> levelClusters; // first index is cluster's index; second index is level's indexInCluster.

	// Getters
	public int NumClusters { get { return levelClusters.Count; } }
	public int NumLevelsInCluster (int clusterIndex) { return levelClusters[clusterIndex].Count; }
	public string GetLevelKey (int clusterIndex, int indexInCluster) { return levelClusters[clusterIndex][indexInCluster]; }

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public LevelOrder (int _worldIndex) {
		LoadOrderFromFile (_worldIndex); // Load me up from my file!
	}
	private void LoadOrderFromFile (int _worldIndex) {
		string textAssetFilePath = FilePaths.LevelOrder(_worldIndex);
//		TextAsset textAsset = Resources.Load<TextAsset> (textAssetFilePath);
//		if (textAsset == null) {
//			Debug.LogError ("No LevelOrder file found for world! " + textAssetFilePath);
//			return;
//		}
//		string[] stringArray = TextUtils.GetStringArrayFromTextAsset (textAsset);
		if (!System.IO.File.Exists(textAssetFilePath)) {
//			Debug.LogError ("No LevelOrder file found for world! " + textAssetFilePath); Note: Disabled any error message because I'm not using LevelOrders right now.
			return;
		}

		string fileString = System.IO.File.ReadAllText (textAssetFilePath);
		string[] stringArray = TextUtils.GetStringArrayFromStringWithLineBreaks (fileString);

		// Make them clusters!
		levelClusters = new List<List<string>>();
		for (int i=0; i<stringArray.Length; i++) {
			string lineString = stringArray[i];//.Substring (2); // cut the first two characters ("* ").
			string[] levelKeys = lineString.Split (LEVEL_KEY_SEPARATORS, System.StringSplitOptions.RemoveEmptyEntries);
			if (levelKeys.Length != 0) {
				levelClusters.Add (new List<string>(levelKeys));
			}
//			else {
//				Debug.Log ("level keys length is 0. just for debugging");
//			}
		}
	}

}
