using System.Collections;
using UnityEngine;
//using UnityStandardAssets.ImageEffects;

public class GameCameraController : MonoBehaviour {
	// Camera
	[SerializeField] private Camera primaryCamera;
	// Properties
	private float orthoSizeNeutral;
	private float zoomAmount = 1; // UNUSED currently. Stays at 1. It's here for if/when we need it.
	private Rect viewRect;
//	// Scripts
//	[SerializeField] private BlurOptimized blurScript;
	// References
	[SerializeField] private FullScrim fullScrim;

	// Getters / Setters
	private float rotation {
		get { return this.transform.localEulerAngles.z; }
		set { this.transform.localEulerAngles = new Vector3 (0, 0, value); }
	}
	private Rect GetViewRect (Vector2 _rectCenter, float _zoomAmount) {
		Vector2 rectSize = GetViewRectSizeFromZoomAmount (_zoomAmount);
		return new Rect (_rectCenter-rectSize*0.5f, rectSize); // Note: Convert from center to bottom-left pos.
	}
	private Vector2 GetViewRectSizeFromZoomAmount (float zoomAmount) {
		return ScreenHandler.RelativeScreenSize / zoomAmount;
	}
	private float GetZoomAmountForViewRect (Rect rect) {
		return Mathf.Min (ScreenHandler.RelativeScreenSize.x/(float)rect.width, ScreenHandler.RelativeScreenSize.y/(float)rect.height);
	}
	private float ZoomAmount { get { return orthoSizeNeutral / primaryCamera.orthographicSize; } }



	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	private void Awake () {
		// Add event listeners!
		GameManagers.Instance.EventManager.ScreenSizeChangedEvent += OnScreenSizeChanged;
	}
	private void OnDestroy () {
		// Remove event listeners!
		GameManagers.Instance.EventManager.ScreenSizeChangedEvent -= OnScreenSizeChanged;
	}

	// Debug
	private void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube (viewRect.center*GameVisualProperties.WORLD_SCALE, new Vector3(viewRect.size.x,viewRect.size.y, 10)*GameVisualProperties.WORLD_SCALE);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube (viewRect.center*GameVisualProperties.WORLD_SCALE, new Vector3(ScreenHandler.RelativeScreenSize.x+11,ScreenHandler.RelativeScreenSize.y+11, 10)*GameVisualProperties.WORLD_SCALE);//+11 for bloat so we can still see it if there's overlap.
	}

	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnScreenSizeChanged () {
		// Go ahead and totally reset me completely when the screen size changes, just to be safe.
		Reset ();
	}

	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void Reset () {
		viewRect = new Rect ();
		viewRect.size = GetViewRectSizeFromZoomAmount (1);

		UpdateOrthoSizeNeutral ();
		ApplyViewRect ();
	}

	private void UpdateOrthoSizeNeutral () {
		orthoSizeNeutral = ScreenHandler.OriginalScreenSize.y / 2f * GameVisualProperties.WORLD_SCALE;
	}

	private void ApplyViewRect () {
		this.transform.localPosition = new Vector3 (viewRect.center.x, viewRect.center.y, -10); // lock z-pos to -10.
		ApplyZoomAmountToCameraOrthographicSize ();
	}
	private void ApplyZoomAmountToCameraOrthographicSize () {
		float zoomAmount = GetZoomAmountForViewRect (viewRect);
		float targetOrthoSize = orthoSizeNeutral / zoomAmount;
		primaryCamera.orthographicSize = targetOrthoSize;
	}

//	private void UpdateViewRectActual () {
//		viewRect_actual = GetViewRect (this.transform.localPosition, ZoomAmount);
//	}

	public void DarkenScreenForSceneTransition () {
//		blurScript.enabled = true;
//		blurScript.blurAmount = 3f;
		fullScrim.Show (0.5f);
	}



}



