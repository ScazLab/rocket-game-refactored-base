using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


namespace BuildARocketGame {

	public class TrashSlot : MonoBehaviour, IDropHandler {

//		public delegate void ClickForPanelChange(int pieceType);
//		public static event ClickForPanelChange OnClickForPanelChange; 

		public GameObject item {
			get {
				if (transform.childCount > 0) {
					return transform.GetChild (0).gameObject;
				}
				return null;
			}
		}

		#region IDropHandler implementation
		public void OnDrop (PointerEventData eventData)
		{
			if (!item) {
				// set the parent in the heirarchy to be the selected outline pieces that contains the rocket piece
				DragHandler.itemBeingDragged.transform.SetParent (transform);

			}
		}
		#endregion
	}
}