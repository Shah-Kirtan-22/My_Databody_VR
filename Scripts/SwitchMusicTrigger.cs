﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchMusicTrigger : MonoBehaviour {

    public AudioClip newTrack;
    //public AudioClip Original;

    private AudioManager theAM;

	// Use this for initialization
	void Start () {
        theAM = FindObjectOfType<AudioManager>();	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (newTrack != null)
                theAM.ChangeBGM(newTrack);
        }
    }
    //private void OnTriggerExit(Collider other)
    //{
        //if (other.tag == "Player")
        //{
            //theAM.ChangeBGM(Original);
        //}
    //}
}
