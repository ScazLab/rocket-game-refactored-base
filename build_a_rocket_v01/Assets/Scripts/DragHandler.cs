using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


namespace BuildARocketGame {

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
			transform.position = Camera.main.ScreenToWorldPoint(v3); // Allows for camera space conversion
		}

		#endregion

		#region IEndDragHandler implementation

		public void OnEndDrag (PointerEventData eventData)
		{
			itemBeingDragged = null;
			GetComponent<CanvasGroup> ().blocksRaycasts = true;
	        if (transform.childCount > 0 && transform.GetChild(0).tag == transform.parent.tag) // flip fins if necessary
	        {

	            Vector3 newScale = transform.localScale;
	            newScale.x *= -1;
	            transform.localScale = newScale; // Note: Tranform.Rotate() works, but doesn't let you drag object out again
	            var temp = transform.tag;
	            transform.tag = transform.GetChild(0).tag; // Update tags
	            transform.GetChild(0).tag = temp;
	        }
	        if (transform.parent == startParent || transform.tag != transform.parent.tag)
	        {

	                transform.position = startPosition;
	                transform.parent = startParent;
			}
		}

		#endregion
	}

}