using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	float timeRemaining = 5.0f; // bullets disappear after this many seconds
	public bool playerHandled = false;
	public bool hitFish = false;
	public bool hitRabbit = false;
	public bool hitMonster = false;

	void Update () {
		// keep track of time remaining before bullet disappears
		timeRemaining -= Time.deltaTime;
		if (timeRemaining <= 0) {
			Destroy (gameObject);
		} else if (playerHandled == true) {
			Destroy (gameObject);
		}
	}

	/*void UpdateHits (GameObject col) {
		//try {
			col.hits++;
		//} catch () {}
	}*/

	void OnCollisionEnter (Collision col) {
		// bullets also disappear if they hit a shootable gameObject
		if (col.gameObject.CompareTag ("fish")) {
			hitFish = true;
			Destroy (col.gameObject);
		} else if (col.gameObject.CompareTag ("monster")) {
			Destroy (col.gameObject);
			hitMonster = true;
		} else if (col.gameObject.CompareTag ("rabbit")) {
			hitRabbit = true;
			Destroy (col.gameObject);
		}
	}
}
