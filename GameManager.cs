using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public GameObject fish;

    // Use this for initialization
    void Start () {
        InvokeRepeating("LaunchFishes", 0.0f, .50f); //delay 0 seconds, every 3 seconds instatiate a fish
    }

    void LaunchFishes()
    {
        Instantiate(fish, new Vector3(-335.6777f, 0.1398773f, 176.0931f), Quaternion.Euler(0, 90, 0));
    }


}
