using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesHandler : MonoBehaviour {
    // References!
    [Header ("Common")]
    [SerializeField] public GameObject ImageLine;
    [SerializeField] public GameObject ImageLinesJoint;
    
    [Header ("AbbaIsYou")]
    // Level, Board
    [SerializeField] public GameObject Level;
    [SerializeField] public GameObject BoardView;
    [SerializeField] public GameObject BoardSpaceView;
    // TileViews
    [SerializeField] private GameObject AbbaView=null;
    [SerializeField] private GameObject CrateView=null;
    [SerializeField] private GameObject CrateGoalView=null;
    [SerializeField] private GameObject ExitSpotView=null;
    [SerializeField] private GameObject TextBlockView=null;
    
    // Getters
    public GameObject GetTileView(Tile sourceObject) {
        if (sourceObject is Abba) { return AbbaView; }
        if (sourceObject is Crate) { return CrateView; }
        if (sourceObject is CrateGoal) { return CrateGoalView; }
        if (sourceObject is ExitSpot) { return ExitSpotView; }
        if (sourceObject is TextBlock) { return TextBlockView; }
        Debug.LogError ("Trying to add TileView from Tile, but no clause to handle this type! " + sourceObject.GetType());
        return null;
    }
    
    
    
    // Instance
    static public ResourcesHandler Instance { get; private set; }


    // ----------------------------------------------------------------
    //  Awake
    // ----------------------------------------------------------------
    private void Awake () {
        // There can only be one (instance)!
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy (this);
        }
	}
    
}
