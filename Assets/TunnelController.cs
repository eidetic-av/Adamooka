using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelController : MonoBehaviour {

	public static TunnelController Instance;

	public float DampRate = 10f;

	public List<MeshRenderer> TunnelRenderers;
	List<Vector2> Alphas = new List<Vector2>();
	void Start () {
		Instance = this;
		TunnelRenderers.ForEach(tunnel => {
			Alphas.Add(new Vector2(0, 0));
		});
	}
	
	// Update is called once per frame
	void Update () {
		TunnelRenderers.ForEach(tunnel => {
			var i = TunnelRenderers.IndexOf(tunnel);
			if (Mathf.Abs(Alphas[i].x - Alphas[i].y) > 0.001f) {
				Alphas[i] = new Vector2(Alphas[i].x + (Alphas[i].y - Alphas[i].x) / DampRate, Alphas[i].y);
			}
			tunnel.material.SetColor("_Color", new Color(1, 1, 1, Alphas[i].x));
		});
	}

	public void Bang(int index) {
		if (index >= Alphas.Count) return;
		Alphas[index] = new Vector2(1, 0);
	}
}
