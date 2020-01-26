using UnityEngine;
using System.Collections;

public class GameProperties : MonoBehaviour {
	public const string VERSION_NUMBER = "0.2";

	public const int NUM_WORLDS = 7;
//	public const int[] PLAYABLE_WORLD_INDEXES = new int[] { 1,2,3,4,5,6 };
	public const int FIRST_WORLD_INDEX = 1; // the first world is 1. World 0 is just for testing/development.


	static public bool IsSwipeInstructionsLevel (Level _level) {
		return  _level.WorldIndex==1 && _level.LevelKey=="introGame";
	}



}


