using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum MovementTypes {
	None = 0,
	Any = 1,
	PullBehind = 2,
	PullSide = 4
}
