using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class FreezeFramer : MonoBehaviour
{

	public GameObject OutputObject;
    public bool Capture = false;

	private int FrameWidth;
	private int FrameHeight;
    Camera Camera;
    Material Material;

    void Start() {
        Camera = GetComponent<Camera>();
        if (OutputObject != null)
		    Material = OutputObject.GetComponent<Renderer>().material;

		FrameWidth = Camera.targetTexture.width;
		FrameHeight = Camera.targetTexture.height;
    }

    public void SetOutput(GameObject outputObject) {
        OutputObject = outputObject;
		Material = OutputObject.GetComponent<Renderer>().material;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {

        if (Capture) {

			// if(source.width != FrameWidth || source.height != FrameHeight)
			// {
			// 	throw new UnityException("FreezeFramer render target size has changed!");
			// }

            var renderTexture = new RenderTexture(FrameWidth, FrameHeight, 0);

            Graphics.Blit(source, renderTexture);
            Material.mainTexture = renderTexture;

            Capture = false;
        }
        
		// Passthrough
		Graphics.Blit (source, destination);
    }

}