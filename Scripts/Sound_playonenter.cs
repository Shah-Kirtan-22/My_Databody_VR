using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound_playonenter : MonoBehaviour {
    //public float num;
    public AudioSource source;
    public AudioClip clip;

    public void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            //Debug.Log("entered collision");
            //Debug.Log("Playing?" + source.isPlaying);
            source.Play();
            //Debug.Log("Playing?" + source.isPlaying);
        } 
    }
    public void OnTriggerExit(Collider other)
    {
        //Debug.Log("collision exit");
        if (other.tag == "Player")
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
    }
}
