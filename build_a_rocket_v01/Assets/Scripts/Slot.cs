using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler, IPointerClickHandler {

	public bool isDashedOutlinePiece;

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
			DragHandler.itemBeingDragged.transform.SetParent (transform);
			// calls it on everything above this one on the heirarchy until it is handled
			ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x,y) => x.HasChanged());
		}
	}
	#endregion

	#region IPointerClickHandler implementation

	public void OnPointerClick (PointerEventData eventData)
	{
		// if there's a click on a piece that's a dashed outline piece then we want to trigger
		// the animation that brings in the panels with that kind of piece
		if (isDashedOutlinePiece) {
			Debug.Log ("Trigger panels for bringing in new pieces");
		}
	}

	#endregion
}
