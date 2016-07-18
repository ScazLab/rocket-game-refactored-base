using UnityEngine;
using System.Collections;

namespace BuildARocketGame {

	/* we only have this script on the left panel, because the panels always mirror each other */

	public class PanelAnimationEventHandler : MonoBehaviour {
		
		public delegate void TriggerPanelIn();
		public static event TriggerPanelIn OnTriggerPanelIn;

		void OnPanelIn () {
			OnTriggerPanelIn ();
		}
	}
}