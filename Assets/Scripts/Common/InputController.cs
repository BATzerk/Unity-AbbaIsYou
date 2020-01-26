using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour {
	// Constants
//	private const float AXIS_MOVEMENT_THRESHOLD = 0.7f; // straightforward: how much (keyboard/joystick) axis input needed to register a move.
	// Instance
	static private InputController instance;
	// Components
	private TouchInputDetector touchInputDetector; // this guy will handle all the mobile stuff so I don't gotta.
	// Properties
	private Vector2 playerAxisInput;
	private Vector2 playerAxisInputRaw; // this ISN'T rotated to match the camera. It's raw, baby. Raw.

	// Getters
	static public InputController Instance {
		get {
//			if (instance==null) { return this; } // Note: This is only here to prevent errors when recompiling code during runtime.
			return instance;
		}
	}
//	static public bool IsPlayerMove_L () { return !isPlayerInputGapTimedOut && playerAxisInput.x< AXIS_MOVEMENT_THRESHOLD; }
//	static public bool IsPlayerMove_R () { return !isPlayerInputGapTimedOut && playerAxisInput.x<-AXIS_MOVEMENT_THRESHOLD; }
//	static public bool IsPlayerMove_D () { return !isPlayerInputGapTimedOut && playerAxisInput.y<-AXIS_MOVEMENT_THRESHOLD; }
//	static public bool IsPlayerMove_U () { return !isPlayerInputGapTimedOut && playerAxisInput.y> AXIS_MOVEMENT_THRESHOLD; }
//	public bool IsTouchDown { get { return touchInputDetector.IsTouchDown; } }
	public bool IsPlayerMove_L () { return Input.GetButtonDown ("MoveL") || touchInputDetector.PushRequestSide==3; }
	public bool IsPlayerMove_R () { return Input.GetButtonDown ("MoveR") || touchInputDetector.PushRequestSide==1; }
	public bool IsPlayerMove_D () { return Input.GetButtonDown ("MoveD") || touchInputDetector.PushRequestSide==2; }
	public bool IsPlayerMove_U () { return Input.GetButtonDown ("MoveU") || touchInputDetector.PushRequestSide==0; }
	public float SimulatedMovePercent { get { return touchInputDetector.SimulatedMovePercent; } }
	public Vector2Int SimulatedMoveDir { get { return touchInputDetector.SimulatedMoveDir; } }




	// ----------------------------------------------------------------
	//  Awake
	// ----------------------------------------------------------------
	private void Awake () {
		// There can only be one (instance)!!
		if (instance != null) {
			Destroy (this.gameObject);
			return;
		}
		instance = this;
	}
	private void Start () {
		touchInputDetector = new TouchInputDetector ();
	}

//	private void OnDrawGizmos () {
//		if (touchInputDetector != null) {
//			Gizmos.color = Color.black;
//			Gizmos.DrawLine (touchInputDetector.Debug_TouchDownPos*GameVisualProperties.WORLD_SCALE, touchInputDetector.Debug_TouchUpPos*GameVisualProperties.WORLD_SCALE);
//		}
//	}

	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		if (touchInputDetector!=null) {
			touchInputDetector.Update ();
		}
		RegisterButtonInputs ();
	}

	private void RegisterButtonInputs () {
//		isButtonDown_Grab = Input.GetButtonDown ("Grab");
		playerAxisInputRaw = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		playerAxisInput = playerAxisInputRaw;
//		isPlayerAxisInput = playerAxisInput.x!=0 || playerAxisInput.y!=0;

		// Scale playerAxisInput so it's normalized, and easier to control. :)
		if (playerAxisInput != Vector2.zero) {
			// Get the length of the directon vector and then normalize it
			// Dividing by the length is cheaper than normalizing when we already have the length anyway
			float directionLength = playerAxisInput.magnitude;
			playerAxisInput = playerAxisInput / directionLength;

			// Make sure the length is no bigger than 1
			directionLength = Mathf.Min (1, directionLength);

			// Make the input vector more sensitive towards the extremes and less sensitive in the middle
			// This makes it easier to control slow speeds when using analog sticks
			directionLength = directionLength * directionLength;

			// Multiply the normalized direction vector by the modified length
			playerAxisInput = playerAxisInput * directionLength;
		}

		// Rotate input to match any camera rotation
		float camAngle = Camera.main.transform.localEulerAngles.z * Mathf.Deg2Rad;
		playerAxisInput = new Vector2 (Mathf.Cos (camAngle)*playerAxisInput.x+Mathf.Sin (camAngle)*playerAxisInput.y, Mathf.Cos (camAngle)*playerAxisInput.y+Mathf.Sin (camAngle)*playerAxisInput.x);
	}


}


