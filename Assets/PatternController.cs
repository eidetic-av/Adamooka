using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility;

public class PatternController : MonoBehaviour
{

    public static PatternController Instance;

	public GameObject CameraObject;
	public float CameraRotationDamping = 10f;
	float NewRotation = 0f;

    public ParticleSystem CircleSystem;
    public ParticleSystem LineSystem;

    ParticleSystemRenderer CircleSystemRenderer;
    ParticleSystemRenderer LineSystemRenderer;

    public bool SpeedBang = false;
    public AnimationCurve SpeedBangCurve = AnimationCurve.EaseInOut(0, 5, 1, 1);
    public float SpeedBangLength = 1f;
    float SpeedBangStartTime;
    bool ChangingSpeed;

    public Vector2 CircleScaleMinMax = new Vector2(0.001f, 2);
    public float CircleScalePower = 1;
    public Vector2Int CircleCountMinMax = new Vector2Int(500, 50);
    public float CircleCountPower = 1;
    public Vector2 CircleSpeedMinMax = new Vector2(0.2f, 2f);
    public float CircleSpeedPower = 1;
    public Vector2 CircleLifetimeMinMax = new Vector2(0.2f, 2f);
    public float CircleLifetimePower = 1;
	
    public Vector2 LineScaleMinMax = new Vector2(0.001f, 2);
    public float LineScalePower = 1;
    public Vector2Int LineCountMinMax = new Vector2Int(500, 50);
    public float LineCountPower = 1;
    public Vector2 LineSpeedMinMax = new Vector2(0.2f, 2f);
    public float LineSpeedPower = 1;
    public Vector2 LineLifetimeMinMax = new Vector2(0.2f, 2f);
    public float LineLifetimePower = 1;

    void Start()
    {
        Instance = this;
        CircleSystemRenderer = CircleSystem.GetComponent<ParticleSystemRenderer>();
        LineSystemRenderer = LineSystem.GetComponent<ParticleSystemRenderer>();
    }

    void Update()
    {

        if (SpeedBang)
        {
            SpeedBangStartTime = Time.time;
            ChangingSpeed = true;
            SpeedBang = false;
        }
        if (ChangingSpeed)
        {
            float position = (Time.time - SpeedBangStartTime) / SpeedBangLength;
            if (position > 1)
            {
                position = 1;
                ChangingSpeed = false;
            }
            float value = SpeedBangCurve.Evaluate(position);
            var mainModule = CircleSystem.main;
            mainModule.simulationSpeed = value;
        }

		var euler = CameraObject.transform.eulerAngles;
		if (Mathf.Abs(NewRotation - euler.z) > 1f) {
			euler.z = euler.z + (NewRotation - euler.z) / CameraRotationDamping;
			CameraObject.transform.eulerAngles = euler;
		}

    }

    public void ActivateCircleSystem()
    {
        var emissionModule = CircleSystem.emission;
		emissionModule.enabled = true;
    }

    public void DeactivateCircleSystem()
    {
        var emissionModule = CircleSystem.emission;
		emissionModule.enabled = false;
    }

	public void ActivateLineSystem() {
        var emissionModule = LineSystem.emission;
		emissionModule.enabled = true;
	}

	public void DeactivateLineSystem() {
        var emissionModule = LineSystem.emission;
		emissionModule.enabled = false;
	}

    public void TriggerSpeedBang()
    {
        if (Osc.Instance.Breakdown == 0)
            SpeedBang = true;
    }

    public void SetScale(float scale)
    {
		// circle
        scale = Mathf.Pow(scale, CircleScalePower);
        var particleScale = scale.Map(0, 1, CircleScaleMinMax.x, CircleScaleMinMax.y);
        CircleSystemRenderer.maxParticleSize = particleScale;
        var mainModule = CircleSystem.main;
        var startSize = mainModule.startSize;
        startSize.constantMax = particleScale;
        mainModule.startSize = startSize;

		// line
        scale = Mathf.Pow(scale, LineScalePower);
        particleScale = scale.Map(0, 1, LineScaleMinMax.x, LineScaleMinMax.y);
        // LineSystemRenderer.maxParticleSize = particleScale;
        mainModule = LineSystem.main;
        var startSizeY = mainModule.startSizeY;
        startSizeY.constantMax = particleScale;
        mainModule.startSizeY = startSizeY;
    }

    public void SetCount(float count)
    {
		// circle
        count = Mathf.Pow(count, CircleCountPower);
        var particleCount = Mathf.RoundToInt(count.Map(0, 1, CircleCountMinMax.x, CircleCountMinMax.y));
        var emissionModule = CircleSystem.emission;
        emissionModule.rateOverTime = particleCount;

		// line
        count = Mathf.Pow(count, LineCountPower);
        particleCount = Mathf.RoundToInt(count.Map(0, 1, LineCountMinMax.x, LineCountMinMax.y));
        emissionModule = LineSystem.emission;
        emissionModule.rateOverTime = particleCount;
    }

    public void SetSpeed(float speed)
    {
		// circle
        speed = Mathf.Pow(speed, CircleSpeedPower);
        var particleSpeed = speed.Map(0, 1, CircleSpeedMinMax.x, CircleSpeedMinMax.y);
        var mainModule = CircleSystem.main;
        mainModule.startSpeedMultiplier = speed;

        var lifetime = Mathf.Pow(speed, CircleLifetimePower);
        lifetime = lifetime.Map(0, 1, CircleLifetimeMinMax.x, CircleLifetimeMinMax.y);
        mainModule.startLifetimeMultiplier = lifetime;

		//line
        speed = Mathf.Pow(speed, LineSpeedPower);
        particleSpeed = speed.Map(0, 1, LineSpeedMinMax.x, LineSpeedMinMax.y);
        mainModule = LineSystem.main;
        mainModule.startSpeedMultiplier = speed;

        lifetime = Mathf.Pow(speed, LineLifetimePower);
        lifetime = lifetime.Map(0, 1, LineLifetimeMinMax.x, LineLifetimeMinMax.y);
        mainModule.startLifetimeMultiplier = lifetime;
    }

	public void SetRotation(float rotation) {
		var angle = rotation.Map(0, 1, 0, 359);
		NewRotation = angle;
	}
}
