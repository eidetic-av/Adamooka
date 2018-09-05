using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateEnabler : MonoBehaviour
{

    public string WatchName;
    public int WatchState;

    StateWatcher Watcher;

    bool Active = false;

    List<GameObject> Children = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
		if (Watcher == null) {
        	var watcher = StateWatcher.Get(WatchName);
			if (watcher != null)
        		Watcher = StateWatcher.Get(WatchName);
		}
        if (!Active && Watcher.State == WatchState)
        {
			foreach(GameObject child in Children) {
				child.SetActive(true);
			}
			Active = true;
			return;
        }
		if (Active && Watcher.State != WatchState) {
            Children.Clear();
            foreach (Transform child in transform)
            {
                Children.Add(child.gameObject);
				child.gameObject.SetActive(false);
            }
			Active = false;
			return;
		}
    }
}
