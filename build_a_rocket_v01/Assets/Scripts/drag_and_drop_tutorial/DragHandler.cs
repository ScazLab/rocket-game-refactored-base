using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	public static GameObject itemBeingDragged; 
	Vector3 startPosition;
	Transform startParent;

	#region IBeginDragHandler implementation

	public void OnBeginDrag (PointerEventData eventData)
	{
		itemBeingDragged = gameObject;
		startPosition = transform.position;
		startParent = transform.parent;
		// allows us to pass events from what's being dragged to the events behind it
		GetComponent<CanvasGroup> ().blocksRaycasts = false; 
        
	}

	#endregion

	#region IDragHandler implementation

	public void OnDrag (PointerEventData eventData)
	{
        var v3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
		transform.position = Camera.main.ScreenToWorldPoint(v3); 
	}

	#endregion

	#region IEndDragHandler implementation

	public void OnEndDrag (PointerEventData eventData)
	{
		itemBeingDragged = null;
		GetComponent<CanvasGroup> ().blocksRaycasts = true;
		if (transform.parent == startParent) {
			transform.position = startPosition;
		}
	}

	#endregion
}
