using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strobe : MonoBehaviour {

    public float StrobeLength = 0.2f;
    public bool Bang = false;

    bool Active = false;
    float BangTime;

    MeshRenderer MeshRenderer;

	void Start () {
        MeshRenderer = gameObject.GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Bang)
        {
            MeshRenderer.enabled = true;
            Active = true;
            BangTime = Time.time;
            Bang = false;
        }
        if (Active)
        {
            if ((Time.time - BangTime) > StrobeLength)
            {
                Active = false;
                MeshRenderer.enabled = false;
            }
        }
	}
}
