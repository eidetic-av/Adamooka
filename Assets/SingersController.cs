using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class SingersController : MonoBehaviour {

	public static SingersController Instance;

	public bool Show = false;
	public bool Hide = false;
	public bool PlayAnimation = false;

	public MeshRenderer OutputQuad;
	Material OutputQuadMaterial;
	[Range(0,1)]
	public float Transparency;
	public bool FadeIn = false;
	public float FadeInLength = 3f;
	public bool FadeOut = false;
	public float FadeOutLength = 3f;
	bool FadingIn;
	float FadeInStartTime;
	bool FadingOut;
	float FadeOutStartTime;

	float ZPosition;
	float NewZPosition;
	public Vector2 ZPositionMinMax = new Vector2(100, 7);
	public bool FlyIn = false;
	public float FlyInDamping = 200f;

	public bool EnableKickParticles = false;
	public bool TriggerKick = false;

	List<ParticleSystem> KickParticles = new List<ParticleSystem>();


	SingerController[] SingerControllers;
	SkinnedMeshRenderer[] HeadRenderers;

	void Start () {
		Instance = this;
		SingerControllers = GetComponentsInChildren<SingerController>();
		HeadRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
		OutputQuadMaterial = OutputQuad.material;

		KickParticles.Add(GameObject.Find("SingerKickParticles0").GetComponent<ParticleSystem>());
		KickParticles.Add(GameObject.Find("SingerKickParticles1").GetComponent<ParticleSystem>());
		KickParticles.Add(GameObject.Find("SingerKickParticles2").GetComponent<ParticleSystem>());
		KickParticles.Add(GameObject.Find("SingerKickParticles3").GetComponent<ParticleSystem>());
	}

	void Update() {
		if (Hide) {
			foreach (var head in HeadRenderers) {
				head.enabled = false;
			}
			Hide = false;
		}
		if (Show) {
			foreach (var head in HeadRenderers) {
				head.enabled = true;
			}
			Show = false;
		}
		if (PlayAnimation) {
			Play();
			PlayAnimation = false;
		}

		if (FadeIn) {
			FadeInStartTime = Time.time;
			FadingIn = true;
			FadeIn = false;
		}
		if (FadingIn) {
			Transparency = (Time.time - FadeInStartTime) / FadeInLength;
			if (Transparency >= 1){
				Transparency = 1;
				FadingIn = false;
			}
		}

		if (FadeOut) {
			FadeOutStartTime = Time.time;
			FadingOut = true;
			FadeOut = false;
		}
		if (FadingOut) {
			Transparency = 1 - ((Time.time - FadeOutStartTime) / FadeOutLength);
			if (Transparency <= 0){
				Transparency = 0;
				FadingOut = false;
			}
		}

		OutputQuadMaterial.SetColor("_Color", new Color(1, 1, 1, Transparency.Map(0, 1, 0, 1f)));

		if (FlyIn) {
			ZPosition = ZPositionMinMax.x;
			NewZPosition = ZPositionMinMax.y;
			FlyIn = false;
		}
		if (Mathf.Abs(ZPosition - NewZPosition) >= 0.1f) {
			ZPosition = ZPosition + (NewZPosition - ZPosition) / FlyInDamping;
		}
		transform.position = new Vector3(transform.position.x, transform.position.y, ZPosition);

		if (EnableKickParticles && TriggerKick) {
			KickParticles.ForEach(p => {p.Clear(); p.Play();});
			TriggerKick = false;
		}
	}

	public void Play() {
		foreach (var singerController in SingerControllers) {
			singerController.Play = true;
		}
	}
}
