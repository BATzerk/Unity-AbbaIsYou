using UnityEngine;

/** This class is just to scale this GameObject. */
public class GameWorld : MonoBehaviour {
	private void Awake () {
		// Apply that scale, sir!
		this.transform.localScale = Vector3.one * GameVisualProperties.WORLD_SCALE;
	}
}
