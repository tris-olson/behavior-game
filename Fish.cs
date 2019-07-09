using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour {

    public Transform[] waypointArray1;
    public Transform[] waypointArray2;

    float percentsPerSecond = 0.33f; // %2 of the path moved per second
    float currentPathPercent = 0.0f; //min 0, max 1

    bool path1 = true;

    // Use this for initialization
    void Start()
    {

        waypointArray1 = new Transform[] { GameObject.Find("waypoint").transform, GameObject.Find("waypoint1").transform, GameObject.Find("waypoint2").transform,
            GameObject.Find("waypoint3").transform , GameObject.Find("waypoint4").transform, GameObject.Find("waypoint5").transform, GameObject.Find("waypoint6").transform,
            GameObject.Find("waypoint7").transform};

        if (Random.value < .5) { 
            path1 = false;
            waypointArray2 = new Transform[] { GameObject.Find("2_waypoint").transform, GameObject.Find("2_waypoint1").transform, GameObject.Find("2_waypoint2").transform,
            GameObject.Find("2_waypoint3").transform , GameObject.Find("2_waypoint4").transform, GameObject.Find("2_waypoint5").transform, GameObject.Find("2_waypoint6").transform,
            GameObject.Find("2_waypoint7").transform};
        }


    }

    void Update()
    {
        currentPathPercent += percentsPerSecond * Time.deltaTime;
        if (path1)
            iTween.PutOnPath(gameObject, waypointArray1, currentPathPercent);
        else
            iTween.PutOnPath(gameObject, waypointArray2, currentPathPercent);

        if (currentPathPercent >= 1.0f)
        {
            GameObject.Destroy(gameObject);
        }
    }

    void OnDrawGizmos()
    {
        //Visual. Not used in movement
        //iTween.DrawPath(waypointArray1);
    }

}
