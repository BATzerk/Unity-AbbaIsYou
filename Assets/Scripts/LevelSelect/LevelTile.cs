using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelTile : MonoBehaviour {
	// Components
	[SerializeField] private Button button; // I'm like selectable, mm!
	[SerializeField] private CanvasGroup myCanvasGroup;
	[SerializeField] private Image i_backingFill;
	[SerializeField] private Image i_backingStroke;
	[SerializeField] private Image i_parIcon; // we'll destroy this if we didn't actually hit par.
	[SerializeField] private LevelThumbnail thumbnail;
	[SerializeField] private RectTransform myRectTransform;
	[SerializeField] private TextMeshProUGUI t_levelNumber;
	// References
	[SerializeField] private Sprite s_backingFill_beatLevel;
	[SerializeField] private Sprite s_backingFill_didNotBeatLevel;
	[SerializeField] private Sprite s_parMovesIcon_filled;
	[SerializeField] private Sprite s_parMovesIcon_empty;
	private LevelSelectController levelSelectControllerRef;
	// Properties
	private bool isBonus;
	private bool isLocked;
	private int worldIndex;
	private int levelIndex;
	private string levelKey;

	// Getters
	public bool IsBonus { get { return isBonus; } }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize (LevelSelectController _levelSelectControllerRef, Transform _parentTransform, LevelData ld) {
		levelSelectControllerRef = _levelSelectControllerRef;
		worldIndex = ld.worldIndex;
		levelIndex = ld.levelIndex;
		levelKey = ld.levelKey;
		isBonus = ld.isBonus;
		bool didCompleteLevel = GameManagers.Instance.DataManager.DidCompleteLevel (worldIndex, levelKey);
		bool didAchieveParMoves = didCompleteLevel && GameManagers.Instance.DataManager.DidAchieveParMoves (worldIndex, levelKey);

		// Parent jazz!
		this.transform.SetParent (_parentTransform);
		this.transform.localScale = Vector3.one;
		this.transform.localPosition = Vector3.zero;
		this.transform.localEulerAngles = Vector3.zero;
		this.gameObject.name = "LevelTile " + worldIndex + "-" + levelIndex + " " + levelKey;

		thumbnail.Initialize (ld);

		// Update visuals/interactivity!
		SetIsLocked (ld.isLocked);
		t_levelNumber.text = worldIndex + "-" + levelIndex;

		i_backingFill.sprite = didCompleteLevel ? s_backingFill_beatLevel : s_backingFill_didNotBeatLevel;
		i_backingStroke.color = isBonus ? new Color(1,0.7f,0f, 0.6f) : new Color(1,1,1, 0.3f);
		i_parIcon.sprite = didAchieveParMoves ? s_parMovesIcon_filled : s_parMovesIcon_empty;

	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void SetPosSize (float posX,float posY, Vector2 _size) {
		myRectTransform.anchoredPosition = new Vector2 (posX, posY);
		myRectTransform.sizeDelta = _size;
		// Scale the thumbnail appropriately!
		thumbnail.UpdateScale (_size);
	}
	private void SetIsLocked (bool _isLocked) {
		isLocked = _isLocked;
		// Update visuals!
		button.interactable = !isLocked;
		myCanvasGroup.alpha = isLocked ? 0.04f : 1f;
	}
	public void Debug_UnlockMe () {
		SetIsLocked (false);
	}


	public void OnClicked () {
		levelSelectControllerRef.LoadLevel (worldIndex, levelIndex);
	}


}
