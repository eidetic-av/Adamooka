using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateWatcher : MonoBehaviour {

	public string Name;
	public int State = 0;

	static Dictionary<string, StateWatcher> Instances = new Dictionary<string, StateWatcher>();
	public static StateWatcher Get(string name) {
		return Instances[name];
	}

	void Start () {
		if (Name != null) {
			Instances.Add(Name, this);
		}
	}

	public void SetState(int newState) {
		State = newState;
	}
	
}
