using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Checks if arrow has entered key press area
public class NoteObject : MonoBehaviour
{
    public bool canBePressed;

    public KeyCode keyToPress;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Pressed key is the key to press
        if(Input.GetKeyDown(keyToPress))
        {
            //Arrow can be pressed its in the correct area
            if(canBePressed)
            {
                //Deactivate object (arrow)
                gameObject.SetActive(false);

                //Report we hit a note
                GameManager.instance.NoteHit();
            }
        }
    }

    //If collision with button (activator tag) area detected
    private void OnTriggerEnter2D(Collider2D other)
    {
        //If collision activator 
        if(other.tag == "Activator")
        {
            canBePressed = true;
        }
    }

    //If arrow leaves button area
    private void OnTriggerExit2D(Collider2D other)
    {
        //If no more collision with button area detected
        if (gameObject.activeInHierarchy)   //other.tag == "Activator"
        {
            if(other.tag == "Activator")
            {
                canBePressed = false;

                //Report missing a note
                GameManager.instance.NoteMissed();
            }
        }
    }

    /*
    //If arrow leaves button area
    private void OnTriggerExit2D(Collider2D other)
    {
        //If no more collision with button area detected
        if (other.tag == "Activator" && gameObject.activeSelf)   //other.tag == "Activator"
        {
            canBePressed = false;

            //Report missing a note
            GameManager.instance.NoteMissed();
        }
    }
    */
}
