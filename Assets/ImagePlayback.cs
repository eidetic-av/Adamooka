using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Unity.Utility;

public class ImagePlayback : MonoBehaviour {

	public static ImagePlayback Instance;

	public Texture2D CaptureTexture;

	public bool Play = false;

	// Use this for initialization
	void Start () {
		Instance = this;
		// gameObject.InstanceMaterial();
	}
	
	// Update is called once per frame
	void Update () {
		if (Play) {
			// Load all textures into an array
			Object[] textures = Resources.LoadAll("Capture", typeof(Texture2D));

			GetComponent<Renderer>().material.mainTexture = textures[0] as Texture2D;

			Play = false;
		}
	}
}
