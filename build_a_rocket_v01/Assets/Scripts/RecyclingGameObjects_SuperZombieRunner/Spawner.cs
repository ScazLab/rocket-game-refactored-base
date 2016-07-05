using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	public GameObject[] prefabs;
	public float delay = 2.0f;
	public bool active = true;
	public Vector2 delayrange = new Vector2(1, 2);

	// Use this for initialization
	void Start () {
		ResetDelay ();
		// co-routine (waits a certain amount of time before executing)
		StartCoroutine (EnemyGenerator ());
	}

	// the co-routine - returns interface of IEnumerator
	IEnumerator EnemyGenerator() {

		// makes the code after this statement executable only after the delay
		yield return new WaitForSeconds (delay);

		if (active) {
			var newTransform = transform;

			// Instantiate(prefabs[Random.Range(0, prefabs.Length)], newTransform.position, Quaternion.identity);
			GameObjectUtil.Instantiate(prefabs[Random.Range(0, prefabs.Length)], newTransform.position);
			ResetDelay();
		}

		// restart the coroutine
		StartCoroutine (EnemyGenerator ());
	}

	void ResetDelay() {
		delay = Random.Range (delayrange.x, delayrange.y);
	}

}
