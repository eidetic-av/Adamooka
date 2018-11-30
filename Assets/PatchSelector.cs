using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatchSelector : MonoBehaviour {

	GameObject UserParticles;
	GameObject ParticleClearLayer;
	GameObject UserMesh;

	// Use this for initialization
	void Start () {
		UserParticles = GameObject.Find("UserParticles");
		ParticleClearLayer = GameObject.Find("ParticleClearLayer");
		UserMesh = GameObject.Find("UserMesh");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Z)){
			UserParticles.SetActive(true);
			SetMaterialColor(ParticleClearLayer.GetComponent<Renderer>(), new Color(0, 0, 0, .3f));
			UserMesh.GetComponent<Renderer>().material = Resources.Load<Material>("BlackOcclusion");
		} else if (Input.GetKeyDown(KeyCode.X)) {
			SetMaterialColor(ParticleClearLayer.GetComponent<Renderer>(), new Color(0, 0, 0, 1f));
			UserParticles.SetActive(false);
		}
	}

	
    void SetMaterialColor(Renderer renderer, Color color)
    {
        Material material = renderer.material;
        material.SetColor("_Color", color);
    }
}
