using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {

	public RecycleGameObject prefab;

	private List<RecycleGameObject> poolInstances = new List<RecycleGameObject>();

	private RecycleGameObject CreateInstance(Vector3 pos) {
	
		var clone = GameObject.Instantiate (prefab);
		clone.transform.position = pos;
		// make sure any prefab instance we create is nested into the object pool game object
		// in our heirarcy view
		clone.transform.parent = transform;
	
		poolInstances.Add (clone);

		return clone;
	}

	public RecycleGameObject NextObject(Vector3 pos) {
	
		RecycleGameObject instance = null;

		// recycle inactive game objects
		foreach (var go in poolInstances) {
			if (go.gameObject.activeSelf != true) {
				instance = go;
				instance.transform.position = pos;
			}
		}

		// if no instances are found to be inactive, create a new one
		if (instance == null)
			instance = CreateInstance (pos);

		instance.Restart ();

		return instance;
	
	}

}
