using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelThumbnail : MonoBehaviour {
	// Constants
	static private readonly Vector2 defaultSize = new Vector2 (180,180); // I'll make myself at these proportions, and be scaled furthermore to fit my LevelTile's scale. So this value is just eyeballed.
	// Components
	[SerializeField] private RectTransform myRectTransform;
	// References
	[SerializeField] private Sprite s_whiteSquare;
	[SerializeField] private Sprite[] s_beamGoals;
	[SerializeField] private Sprite[] s_beamSources;
	[SerializeField] private Sprite[] s_buckets;
	[SerializeField] private Sprite   s_bucketGoal;
	[SerializeField] private Sprite   s_crate;
	[SerializeField] private Sprite   s_crateGoal;
	[SerializeField] private Sprite   s_exitSpot;
	[SerializeField] private Sprite[] s_mirrors;
	[SerializeField] private Sprite   s_obstacle;
	[SerializeField] private Sprite   s_pusher;
	[SerializeField] private Sprite[] s_portals;
	[SerializeField] private Sprite   s_wall;
	// Properties
	private float unitSize;
	private Vector2 boardSize; // independent of any scaling. Used so we can center-align everything.

	// Getters
	private Vector2 GetBoardPos (int col,int row) {
		return new Vector2 (col*unitSize-(boardSize.x-unitSize)*0.5f, -row*unitSize+(boardSize.y-unitSize)*0.5f);
	}

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize (LevelData ld) {
		// Set basic values.
		BoardData bd = ld.boardData;
		int numCols = bd.numCols;
		int numRows = bd.numRows;

		// What's the size I've got?
		unitSize = Mathf.Min(defaultSize.x/(float)(numCols), defaultSize.y/(float)(numRows));
		boardSize = new Vector2(numCols,numRows) * unitSize;

//		// Offset my content layer! So we're center-aligned within the tile.
//		rt_contentLayer.anchoredPosition = new Vector3 (-(boardSize.x-unitSize)*0.5f, (boardSize.y-unitSize)*0.5f, 0);

		// BoardSpaces!
		for (int col=0; col<numCols; col++) {
			for (int row=0; row<numRows; row++) {
				if (bd.spaceDatas[col,row].isPlayable) {
					Image i_space = new GameObject().AddComponent<Image>();
					i_space.sprite = s_whiteSquare;
					i_space.name = "i_boardSpace"+col+","+row;
					i_space.transform.SetParent (this.transform);
					i_space.transform.localScale = Vector3.one;
					i_space.rectTransform.sizeDelta = new Vector2(unitSize,unitSize);
					i_space.color = Colors.GetBoardSpaceColor (ld.worldIndex, BoardUtils.IsSpaceEven(col,row));
					i_space.transform.localPosition = GetBoardPos (col, row);
				}
			}
		}

		// All BoardObjects!
		foreach (ExitSpotData data in bd.exitSpotDatas) {
			AddBoardObjectImage (data, s_exitSpot, new Color (0,0,0, 0.4f));
		}
		foreach (BeamGoalData data in bd.beamGoalDatas) {
			AddBoardOccupantImage (data, s_beamGoals, Colors.GetBeamColor(ld.worldIndex, data.channelID));
		}
		foreach (BeamSourceData data in bd.beamSourceDatas) {
			AddBoardOccupantImage (data, s_beamSources, Colors.GetBeamColor(ld.worldIndex, data.channelID));
//			AddBeamThumbnail (bd, data);
		}
		foreach (BucketData data in bd.bucketDatas) {
			AddBoardOccupantImage (data, s_buckets);
		}
		foreach (BucketGoalData data in bd.bucketGoalDatas) {
			AddBoardObjectImage (data, s_bucketGoal, BucketGoalView.color_off);
		}
		foreach (CrateData data in bd.crateDatas) {
			AddBoardOccupantImage (data, s_crate);
		}
		foreach (CrateGoalData data in bd.crateGoalDatas) {
			AddBoardObjectImage (data, s_crateGoal, CrateGoalView.color_off);
		}
		foreach (MirrorData data in bd.mirrorDatas) {
			AddBoardOccupantImage (data, s_mirrors);
		}
		foreach (ObstacleData data in bd.obstacleDatas) {
			AddBoardOccupantImage (data, s_obstacle, new Color(0.8f,0.8f,0.8f));
		}
		foreach (PortalData data in bd.portalDatas) {
			AddBoardOccupantImage (data, s_portals);
		}
		foreach (PusherData data in bd.pusherDatas) {
			AddBoardObjectImage (data, s_pusher, Color.white);
		}
		foreach (WallData data in bd.wallDatas) {
			AddBoardObjectImage (data, s_wall, Color.white);
		}
	}

	private void AddBoardOccupantImage (BoardOccupantData data, Sprite sprite) {
		float occupantScale = BoardObjectView.GetScaleFromOccupantLayer(data.boardPos.layer);
		AddBoardObjectImage(data, sprite, Color.white, occupantScale);
	}
	private void AddBoardOccupantImage (BoardOccupantData data, Sprite[] spriteOptions, Color color) {
		if (data.isMovable) { AddBoardOccupantImage(data, spriteOptions[1], color); }
		else				{ AddBoardOccupantImage(data, spriteOptions[0], color); }
	}
	private void AddBoardOccupantImage (BoardOccupantData data, Sprite[] spriteOptions) { AddBoardOccupantImage (data, spriteOptions, Color.white); }
	private void AddBoardOccupantImage (BoardOccupantData data, Sprite sprite, Color color) {
		float occupantScale = BoardObjectView.GetScaleFromOccupantLayer(data.boardPos.layer);
		AddBoardObjectImage(data, sprite, color, occupantScale);
	}
	private void AddBoardObjectImage (BoardObjectData data, Sprite sprite, Color color) { AddBoardObjectImage(data, sprite, color, 1); }
	private void AddBoardObjectImage (BoardObjectData data, Sprite sprite, Color color, float scale) {
		Image image = new GameObject().AddComponent<Image>();
		image.sprite = sprite;
		image.name = data.GetType().ToString();
		image.rectTransform.SetParent (this.transform);
		image.rectTransform.localScale = Vector3.one;
		image.rectTransform.sizeDelta = new Vector2(unitSize,unitSize) * scale;
		image.rectTransform.localEulerAngles = new Vector3(0, 0, -data.boardPos.sideFacing*90);
		image.rectTransform.localPosition = GetBoardPos (data.boardPos.col, data.boardPos.row);
		image.color = color;
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void UpdateScale (Vector2 _levelTileSize) {
		// Shrink down size of thumbnail so it doesn't run up to our edge.
		myRectTransform.localScale = new Vector3 (_levelTileSize.x/defaultSize.x, _levelTileSize.y/defaultSize.y, 1) * 0.7f;
	}

}
