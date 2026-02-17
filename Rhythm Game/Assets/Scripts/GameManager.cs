using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AudioSource theMusic;

    public bool startPlaying;

    public BeatScroller theBS;

    //Public static instance reference of game manager for NoteObject
    public static GameManager instance;

    public int currentScore;
    public int scorePerNote = 100;

    public int currentMultiplier;
    public int multiplierTracker;
    public int[] multiplierThreshoulds;

    //For canvas
    public Text scoreText;
    public Text multiText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Game manager instance to make changes
        instance = this;

        //Set beginning score to 0
        scoreText.text = "Score: 0";

        //Multiplier starts at 1
        currentMultiplier = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //If game has not started
        if(!startPlaying)
        {
            //If user presses any button
            if(Input.anyKeyDown)
            {
                //Start game
                startPlaying = true;
                theBS.hasStarted = true;

                //play music
                theMusic.Play();
            }
        }
    }

    //When we hit a note
    public void NoteHit()
    {
        Debug.Log("Hit On Time");

        //If current multiplier has not exceed the multiplierThreshould
        if (currentMultiplier - 1 < multiplierThreshoulds.Length)
        {

            multiplierTracker++;

            if (multiplierThreshoulds[currentMultiplier - 1] <= multiplierTracker)
            {
                multiplierTracker = 0;
                currentMultiplier++;
            }
        }

        //Modify canvas multiplier
        multiText.text = "Multiplier: x" + currentMultiplier;

        //Add scoring points to score
        currentScore += scorePerNote * currentMultiplier;

        //Modify canvas ScoreText
        scoreText.text = "Score: " + currentScore;
    }

    //When we miss a note
    public void NoteMissed()
    {
        Debug.Log("Missed Note");

        //Reset multiplier
        currentMultiplier = 1;
        multiplierTracker = 0;

        //Update multiplier ScoreText
        multiText.text = "Multiplier: x" + currentMultiplier;
    }



}
