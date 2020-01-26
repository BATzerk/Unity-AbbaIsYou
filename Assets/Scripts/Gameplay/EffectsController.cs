using UnityEngine;
using System.Collections;

public class EffectsController : MonoBehaviour {
	// References
	[SerializeField] private ParticleSystem particles_collectFlower;
	[SerializeField] private ParticleSystem particles_pickUpSeed;
	[SerializeField] private ParticleSystem particles_putSeedInHole;
	[SerializeField] private ParticleSystem particles_refillWater;
	[SerializeField] private ParticleSystem particles_waterFlower;



	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	public void OnCollectFlower (Vector3 pos) {
		particles_collectFlower.transform.localPosition = pos;
		particles_collectFlower.Emit (20);
	}
	public void OnPickUpSeed (Vector3 pos) {
		particles_pickUpSeed.transform.localPosition = pos;
		particles_pickUpSeed.Emit (4);
	}
	public void OnPutSeedInSeedHole (Vector3 pos) {
		particles_putSeedInHole.transform.localPosition = pos;
		particles_putSeedInHole.Emit (4);
	}
	public void OnRefillWater (Vector3 pos) {
		particles_refillWater.transform.localPosition = pos;
		particles_refillWater.Emit (12);
	}
	public void OnWaterFlower (Vector3 pos) {
		particles_waterFlower.transform.localPosition = pos;
		particles_waterFlower.Emit (5);
	}


}


