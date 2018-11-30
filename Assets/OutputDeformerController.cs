using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deform.Deformers;
using Deform;

public class OutputDeformerController : MonoBehaviour {

	public GameObject OutputObject;
	public bool StartNoiseDeformer;

	NoiseDeformer NoiseDeformer;
	DeformerComponentManager Manager;
	MeshRenderer MeshRenderer;

	void Start () {
		NoiseDeformer = OutputObject.GetComponent<NoiseDeformer>();
		Manager = OutputObject.GetComponent<DeformerComponentManager>();
		MeshRenderer = OutputObject.GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (StartNoiseDeformer) {
			Manager.enabled = true;
			NoiseDeformer.enabled = true;
			NoiseDeformer.update = true;
			StartNoiseDeformer = false;
		}
	}
}
