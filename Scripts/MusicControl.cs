using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicControl : MonoBehaviour
{

    public bool indoors;
    public int colliderCount;
    public AudioSource Music;

    private void Start()
    {
        Music = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            colliderCount += 1;
            UpdateState();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            colliderCount -= 1;
            UpdateState();
        }
    }

    void UpdateState()
    {
        if (colliderCount == 0)
        {
            indoors = false;
            Music.Stop();
            Debug.Log("False");
        }


        else if (colliderCount > 0)
        {
            indoors = true;
            Music.Play();
            Debug.Log("True");
        }
    }
}
