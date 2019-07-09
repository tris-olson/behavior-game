using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour {

    bool FPSinArea = false;
    Animation anim;

    Vector3 FPSdestination;


    //UPDATE THESE TO CHANGE MONSTER EFFICIENCY
    private float timer = 0.5f; // how fast monster updates person location to follow
    private float speed = 20f; // how fast monster move

    // Use this for initialization
    void Start ()
    {
        anim = this.GetComponent<Animation>();
        anim["rage"].wrapMode = WrapMode.Loop;
        anim.Play(anim["rage"].name);

        // "die", "hit" , "hit2" will be useful anim modes
    }

    void OnTriggerEnter(Collider other)
    {
        //when FPS enters circle
        if(other.name == "FPSController")
        {
            //Debug.Log("On Trigger Enter");
            //Debug.Log(other);
            FPSinArea = true;
        }

    }

    void UpdatePosition()
    {
        FPSdestination = GameObject.Find("FPSController").transform.position;
    }


    // Update is called once per frame
    void Update () {

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        if (timer <= 0)
        {
            //update to the player's position after timer is up
            UpdatePosition();
            timer = 0.1f;
        }

        if (FPSinArea)
        {
            transform.position = Vector3.MoveTowards(transform.position, FPSdestination, speed * Time.deltaTime);
            transform.LookAt(FPSdestination);
        }

    }


}

