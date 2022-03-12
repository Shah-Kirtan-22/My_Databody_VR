using UnityEngine;
using System.Collections;

public class ClipPlaneInteractor : MonoBehaviour {

    bool isClipPlane = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void clipPlaneOnOff()
    {
        if (isClipPlane)
        {
            isClipPlane = false;
            this.GetComponent<MeshRenderer>().enabled = false;
            this.GetComponent<MeshCollider>().enabled = false;
        }
        else
        {
            isClipPlane = true;
            this.GetComponent<MeshRenderer>().enabled = true;
            this.GetComponent<MeshCollider>().enabled = true;
        }
        //Debug.Log(isClipPlane);
    }
}
