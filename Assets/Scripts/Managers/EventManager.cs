using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventManager {
	// Actions and Event Variables
	public delegate void NoParamAction ();
//	public delegate void BoardAction (Board _board);
	public delegate void BoolAction (bool _bool);
	public delegate void IntAction (int _int);
	public delegate void StartGameAtLevelAction (Level _level);

	public event NoParamAction ScreenSizeChangedEvent;
//	public event BoardAction BoardFinishedMoveStepEvent;
//	public event BoardAction BoardMoveCompleteEvent;
	public event BoolAction SetIsLevelCompletedEvent;
	public event IntAction NumMovesMadeChangedEvent;
	public event StartGameAtLevelAction StartGameAtLevelEvent;

	// Events
//	public void OnBoardFinishedMoveStep (Board board) { if (BoardFinishedMoveStepEvent!=null) { BoardFinishedMoveStepEvent (board); } }
//	public void OnBoardMoveComplete (Board board) { if (BoardMoveCompleteEvent!=null) { BoardMoveCompleteEvent (board); } }
	public void OnSetIsLevelCompleted (bool isLevelComplete) { if (SetIsLevelCompletedEvent!=null) { SetIsLevelCompletedEvent (isLevelComplete); } }
	public void OnNumMovesMadeChanged (int numMovesMade) { if (NumMovesMadeChangedEvent!=null) { NumMovesMadeChangedEvent (numMovesMade); } }
	public void OnScreenSizeChanged () { if (ScreenSizeChangedEvent!=null) { ScreenSizeChangedEvent (); } }
	public void OnStartGameAtLevel (Level _level) { if (StartGameAtLevelEvent!=null) { StartGameAtLevelEvent(_level); } }



}




