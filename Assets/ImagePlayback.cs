using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using Eidetic.Unity.Utility;
using Utility;
using B83.Image.BMP;

public class ImagePlayback : MonoBehaviour
{

    public static ImagePlayback Instance;

	public int FramesPerSecond = 30;

    public bool Play = false;
	private bool Playing = false;

	private int LastFrame = -1;
	private float StartTime;
	private float TotalTime;
	private float PlaybackTime;

	BMPLoader loader = new BMPLoader();

	List<Texture2D> Textures = new List<Texture2D>();

	UnityEngine.Material Material;

    // Use this for initialization
    void Start()
    {
        Instance = this;
        // gameObject.InstanceMaterial();
		Material = GetComponent<Renderer>().material;
    }

	public void ClearImages() {
		Textures.Clear();
	}

	public void LoadImage(string filePath) {
		var img = loader.LoadBMP(filePath);
		var texture = img.ToTexture2D();
		Textures.Add(texture);
	}

    // Update is called once per frame
    void Update()
    {
		if (Play) {
			StartTime = Time.time;
			TotalTime = (float) Textures.Count / (float) FramesPerSecond;
			Debug.Log("TotalTime: " + TotalTime);
			PlaybackTime = 0;
			Play = false;
			Playing = true;
		}

		if (Playing) {
			PlaybackTime += Time.deltaTime;
			if (PlaybackTime >= TotalTime) {
				PlaybackTime = PlaybackTime - TotalTime;
			}
			var currentPosition = PlaybackTime / TotalTime;
			var currentFrame = Mathf.FloorToInt(currentPosition.Map(0f, 1f, 0f, (float)Textures.Count));
			if (currentFrame != LastFrame) {
				Material.mainTexture = Textures[currentFrame];
				LastFrame = currentFrame;
			}
		}
    }
}
