using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strobe : MonoBehaviour {

    public float StrobeLength = 0.2f;
    public bool DoFlash = false;

    bool Active = false;
    float FlashTime;

    MeshRenderer MeshRenderer;

	void Start () {
        MeshRenderer = gameObject.GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (DoFlash)
        {
            MeshRenderer.enabled = true;
            Active = true;
            FlashTime = Time.time;
            DoFlash = false;
        }
        if (Active)
        {
            if ((Time.time - FlashTime) > StrobeLength)
            {
                Active = false;
                MeshRenderer.enabled = false;
            }
        }
	}

    public void Flash()
    {
        MeshRenderer.enabled = true;
        Active = true;
        FlashTime = Time.time;
    }
}
