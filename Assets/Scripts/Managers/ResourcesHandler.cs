using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesHandler : MonoBehaviour {
	// References!
	[SerializeField] public GameObject prefabGO_beamSegmentRenderer;
	[SerializeField] public GameObject prefabGO_boardSpaceView;
	[SerializeField] public GameObject prefabGO_boardView;

	[SerializeField] public GameObject prefabGO_beamGoalView;
	[SerializeField] public GameObject prefabGO_beamSourceView;
	[SerializeField] public GameObject prefabGO_bucketView;
	[SerializeField] public GameObject prefabGO_bucketGoalView;
	[SerializeField] public GameObject prefabGO_crateView;
	[SerializeField] public GameObject prefabGO_crateGoalView;
	[SerializeField] public GameObject prefabGO_exitSpotView;
	[SerializeField] public GameObject prefabGO_floorBeamGoalView;
	[SerializeField] public GameObject prefabGO_mirrorView;
	[SerializeField] public GameObject prefabGO_obstacleView;
	[SerializeField] public GameObject prefabGO_playerView;
	[SerializeField] public GameObject prefabGO_portalView;
	[SerializeField] public GameObject prefabGO_pusherView;
	[SerializeField] public GameObject prefabGO_wallView;

	// Instance
	static private ResourcesHandler instance;
	static public ResourcesHandler Instance { get { return instance; } }

	// Awake
	private void Awake () {
		// There can only be one (instance)!
		if (instance == null) {
			instance = this;
		}
		else {
			GameObject.Destroy (this);
		}
	}
}
