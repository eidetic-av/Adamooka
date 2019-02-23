using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodController : MonoBehaviour
{

    ParticleSystem LeftHand;
    ParticleSystem RightHand;
    ParticleSystem Ribbon;
	void Start()
    {
        LeftHand = GameObject.Find("LeftHand").GetComponent<ParticleSystem>();
        RightHand = GameObject.Find("RightHand").GetComponent<ParticleSystem>();
        Ribbon = GameObject.Find("Ribbon").GetComponent<ParticleSystem>();
    }

    // Control properties at runtime
    [SerializeField]
    float _test;
    public float Test
    {
        get { return _test; }
        set
        {
			LeftHand.gameObject.SetActive(value > 5);
			_test = value;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckKeyboardInput(LeftHand, KeyCode.LeftArrow, true);
        CheckKeyboardInput(RightHand, KeyCode.RightArrow, true);
    }

    void CheckKeyboardInput(ParticleSystem particleSystem, KeyCode key, bool requiresShift = false)
    {
        if (Input.GetKeyDown(key))
        {
            if (Input.GetKey(KeyCode.LeftShift) || !requiresShift)
            {
                particleSystem.Clear();
                particleSystem.Play();
            }
        }
    }
}
