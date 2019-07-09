using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	public Light light;

	// player stats to keep track of throughout the game
	int food = 0;
	int gold = 0;
	int hitPoints = 100;

	// variables updated or used by the gameManager
	public int perimeterHits = 0;
	public int bulletsFired = 0;
	public int numberOfDeaths = 0;
	public Vector3 startingPosition;

	void Start () {
		light = GetComponent<Light> ();
	}

	void Update () {
		if (Input.GetButtonDown ("Fire1")) {
			Fire ();
		}

		if (hitPoints <= 0) {
			hitPoints = 0;
			HandleDeath ();
		}
	}

	void HandleDeath () {
		// on player death, the player is returned to the starting position
		// and loses half of their gold and food
		gold = gold / 2;
		food = food / 2;
		transform.position = startingPosition;
		numberOfDeaths++;
	}

	void Fire () {
		var bullet = (GameObject)Instantiate (
			bulletPrefab,
			bulletSpawn.transform.position,
			bulletSpawn.transform.rotation);
		bullet.GetComponent<Rigidbody> ().velocity = bullet.transform.forward * 10;
		bulletsFired++;
	}

	void OnCollisionEnter (Collision col) {
		if (col.gameObject.CompareTag ("Food")) {
			col.gameObject.SetActive (false);
			food++;
		} else if (col.gameObject.CompareTag ("Perimeter")) {
			perimeterHits++;
		}
	}
}
