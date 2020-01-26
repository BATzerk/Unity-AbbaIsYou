using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpaceView : MonoBehaviour {
	// Components
	[SerializeField] private SpriteRenderer sr_backing;
	[SerializeField] private SpriteRenderer sr_border;
	// Properties
	private Color fillColor;

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize (BoardView _boardView, BoardSpace _mySpace) {
		int col = _mySpace.Col;
		int row = _mySpace.Row;

		// Parent me to my boooard!
		this.transform.SetParent (_boardView.tf_BoardSpaces);
		this.gameObject.name = "BoardSpace_" + col + "," + row;

		// Size/position me right!
		this.transform.localPosition = new Vector3 (_boardView.BoardToX(col), _boardView.BoardToY(row), 0);
		this.transform.localScale = Vector3.one;
		this.transform.localEulerAngles = Vector3.zero;

		GameUtils.SizeSpriteRenderer (sr_border, _boardView.UnitSize+16,_boardView.UnitSize+16);
		GameUtils.SizeSpriteRenderer (sr_backing, _boardView.UnitSize-1f,_boardView.UnitSize-1f); // -1 so we can see the borders between spaces!

		bool isSpaceEven = BoardUtils.IsSpaceEven (col,row);
		fillColor = Colors.GetBoardSpaceColor (_boardView.WorldIndex, isSpaceEven);
		sr_backing.color = fillColor;

		sr_backing.enabled = _mySpace.IsPlayable;
		sr_border.enabled = _mySpace.IsPlayable;
	}

}
