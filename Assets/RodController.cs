using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodController : MonoBehaviour
{
	public float Test;
    ParticleSystem LeftHand;
    ParticleSystem RightHand;
    ParticleSystem Ribbon;
    // Use this for initialization
    void Start()
    {
        LeftHand = GameObject.Find("LeftHand").GetComponent<ParticleSystem>();
        RightHand = GameObject.Find("RightHand").GetComponent<ParticleSystem>();
        Ribbon = GameObject.Find("Ribbon").GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckKeyboardInput(LeftHand, KeyCode.LeftArrow);
        CheckKeyboardInput(RightHand, KeyCode.RightArrow);
    }

    void CheckKeyboardInput(ParticleSystem particleSystem, KeyCode key)
    {
        if (Input.GetKeyDown(key))
        {
            if (Input.GetKey(KeyCode.LeftShift))
			{
                particleSystem.Clear();
				particleSystem.Play();
				GameObject.Find("RuntimeInspector")
					.GetComponent<RuntimeInspectorNamespace.RuntimeInspector>()
					.Inspect(GameObject.Find("Rods"));
			}
        }
    }
}
