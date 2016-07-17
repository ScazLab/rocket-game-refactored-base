using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace UnityEngine.EventSystems {
	public interface IHasChanged : IEventSystemHandler {
		void HasChanged ();
	}
}

namespace BuildARocketGame {
		
	public class Inventory : MonoBehaviour, IHasChanged {
		// always use serializeField unless you want to access it from other scripts
		[SerializeField] Transform slots; 
		[SerializeField] Text inventoryText;

		// Use this for initialization
		void Start () {
			HasChanged ();
		}

		#region IHasChanged implementation

		public void HasChanged ()
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder ();
			builder.Append (" - ");
			foreach (Transform slotTransform in slots) {
				GameObject item = slotTransform.GetComponent<Slot> ().item;
				if (item) {
					builder.Append (item.name);
					builder.Append (" - ");
				}
			}
			inventoryText.text = builder.ToString ();
		}

		#endregion
	}
}