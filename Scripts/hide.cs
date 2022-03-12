using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hide : MonoBehaviour
{
    //public float num;
    //public AudioSource source;
    //public AudioClip clip;
    private MeshRenderer render;

    void Start()
    {
        render = GetComponent<MeshRenderer>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "GameController")
        {
            //Debug.Log("entered collision");
            //Debug.Log("Playing?" + source.isPlaying);
            //source.Play();
            render.enabled = true;
            //Debug.Log("Playing?" + source.isPlaying);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        //Debug.Log("collision exit");
        if (other.tag == "GameController")
        {
            render.enabled = false;
            //if (source.isPlaying)
            //{
            // source.Stop();
            // }
        }
    }
}
