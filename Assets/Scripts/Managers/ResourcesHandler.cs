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
    [SerializeField] public GameObject WallView;
    // TileViews
    //[SerializeField] private GameObject AbbaView=null;
    //[SerializeField] private GameObject CrateView=null;
    //[SerializeField] private GameObject CrateGoalView=null;
    //[SerializeField] private GameObject ExitSpotView=null;
    [SerializeField] private GameObject GenericTileView=null;
    [SerializeField] private GameObject TextBlockView=null;
    // Sprites
    [SerializeField] private Sprite s_abba=null;
    [SerializeField] private Sprite s_brick=null;
    [SerializeField] private Sprite s_crate=null;
    [SerializeField] private Sprite s_exitSpot=null;
    [SerializeField] private Sprite s_is=null;
    [SerializeField] private Sprite s_push=null;
    [SerializeField] private Sprite s_stop=null;
    [SerializeField] private Sprite s_you=null;
    
    
    // Getters
    public GameObject GetTileView(Tile sourceObject) {
        //if (sourceObject is Abba) { return AbbaView; }// TODO: Remove these extra views, yo
        if (sourceObject is GenericTile) { return GenericTileView; }
        if (sourceObject is TextBlock) { return TextBlockView; }
        Debug.LogError ("Trying to add TileView from Tile, but no clause to handle this type! " + sourceObject.GetType());
        return null;
    }
    public Sprite GetTileSprite(TileType tileType) {
        switch (tileType) {
            case TileType.Abba: return s_abba;
            case TileType.Brick: return s_brick;
            case TileType.Crate: return s_crate;
            case TileType.ExitSpot: return s_exitSpot;
            
            case TileType.Is: return s_is;
            
            case TileType.Push: return s_push;
            case TileType.Stop: return s_stop;
            case TileType.You: return s_you;
            default: return null;
        }
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
