using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BuildARocketGame {
	// Interface being used so that we can look for other scripts that the recycle script
	// will need to reset whenever it gets reset itself (aka the Obstacle script)
	public interface IRecycle {

		void Restart();
		void ShutDown();

	}

	public class RecycleGameObject : MonoBehaviour {

		private List<IRecycle> recycleComponents;

		void Awake() {
			var components = GetComponents<MonoBehaviour> ();
			recycleComponents = new List<IRecycle> ();
			foreach (var component in components) {
				if (component is IRecycle) {
					recycleComponents.Add (component as IRecycle);
				}
			}
		}

		public void Restart() {
			gameObject.SetActive (true);

			foreach (var component in recycleComponents) {
				component.Restart();
			}
		}

		// use instead of destroy call
		public void ShutDown() {
			gameObject.SetActive (false);
			
			foreach (var component in recycleComponents) {
				component.ShutDown();
			}
		}

	}

}