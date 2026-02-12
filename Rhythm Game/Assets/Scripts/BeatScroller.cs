using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatScroller : MonoBehaviour
{
    //For how fast arrows fall down
    public float beatTempo;

    //For checking if game start triggered 
    public bool hasStarted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //How fast arrows should move per second, beat tempo divided by 60 seconds
        beatTempo = beatTempo / 60f;
    }

    // Update is called once per frame
    void Update()
    {
        //Check if game has not started, check for key press
        if (!hasStarted)
        {
            /*
             * //If key is pressed, start game
                if (Input.anyKeyDown)
                {
                    hasStarted = true;
                }
             */
        }
        else //Game has started already
        {
            //Move arrows down y-axis based on beat tempo
            transform.position -= new Vector3(0f, beatTempo * Time.deltaTime, 0f);
        }
    }
}

//Standard beat 120
//Unity 120 beats per minute
//Unity 2 beats per second