using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameObjectUtil {

	private static Dictionary<RecycleGameObject, ObjectPool> pools = new Dictionary<RecycleGameObject, ObjectPool> ();

	public static GameObject Instantiate(GameObject prefab, Vector3 pos) {
		GameObject instance = null;

		// want to make sure that the prefab that we pass in is actually a recycled game object
		/*var recycledScript = prefab.GetComponent<RecycleGameObject> ();
		if (recycledScript != null) {
			var pool = GetObjectPool (recycledScript);
			instance = pool.NextObject (pos).gameObject;
			instance.GetComponent<RecycleGameObject>().Restart();
		} 
		// if it doesn't 
		else */{
			instance = GameObject.Instantiate (prefab);
			instance.transform.position = pos;
		}

		return instance;
	}

	public static void Destroy(GameObject gameObject){

		var recycleGameObject = gameObject.GetComponent<RecycleGameObject> ();
		var objectInfo = gameObject.GetComponent<ObjectInfo> ();

		if (recycleGameObject != null && !objectInfo.isLeftoverPiece) {
			recycleGameObject.ShutDown ();
		} else {
			GameObject.Destroy (gameObject);
		}
	}

	private static ObjectPool GetObjectPool(RecycleGameObject reference) {

		ObjectPool pool = null;

		if (pools.ContainsKey (reference)) {
			pool = pools [reference];
		}
		// if the pool doesn't exist, create it from scratch
		else {
			// way for us to store the refence of the pool script on it
			var poolContainer = new GameObject(reference.gameObject.name + "ObjectPool");
			pool = poolContainer.AddComponent<ObjectPool>();
			pool.prefab = reference;
			pools.Add (reference, pool);
		}
	
		return pool;
	}
	
}
