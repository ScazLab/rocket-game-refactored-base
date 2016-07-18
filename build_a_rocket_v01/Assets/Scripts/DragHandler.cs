using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


namespace BuildARocketGame {

	public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

		public delegate void PieceClonedToPanel(GameObject pieceAdded);
		public static event PieceClonedToPanel OnPieceClonedToPanel;

		public delegate void PieceRemovedByTrash (GameObject pieceRemoved);
		public static event PieceRemovedByTrash OnPieceRemovedByTrash;

		public static GameObject itemBeingDragged; 
		Vector3 startPosition;
		Transform startParent;
		Vector3 startScale;

		#region IBeginDragHandler implementation

		public void OnBeginDrag (PointerEventData eventData)
		{
			itemBeingDragged = gameObject;
			startPosition = transform.position;
			startParent = transform.parent;
			startScale = transform.localScale;
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

			// if the piece is a fin and we need to flip it, we follow the following procedure
	        if (transform.childCount > 0 && transform.GetChild(0).tag == transform.parent.tag) // flip fins if necessary
	        {
	            Vector3 newScale = transform.localScale;
	            newScale.x *= -1;
	            transform.localScale = newScale; // Note: Tranform.Rotate() works, but doesn't let you drag object out again
	            var temp = transform.tag;
	            transform.tag = transform.GetChild(0).tag; // Update tags
	            transform.GetChild(0).tag = temp;
			}

			// if a piece was dragged from the rocket to the trash
			if (transform.parent.tag == "Trash" && startParent.tag != "PieceGrop") {
				// remove the gameobject from any lists it's a part of in the game manager
				OnPieceRemovedByTrash (gameObject);
				// destroy it
				Destroy (gameObject);
			}
			// if the piece's parent is not of the same type, if it's parent is the panel
			// we send the piece back to where it came from
			else if (transform.parent == startParent || transform.tag != transform.parent.tag) {
				transform.position = startPosition;
				transform.SetParent (startParent);
			}
			// otherwise, we clone the piece and put a new one in the panel in place of the old one
			// as long as the parent is the panel
			else if (startParent.tag == "PieceGroup") {
				GameObject clone = Instantiate (gameObject);
				clone.transform.position = startPosition;
				clone.transform.SetParent (startParent);
				clone.transform.localScale = startScale;
				clone.transform.tag = gameObject.transform.tag;

				// let the game manager know that we've cloned a new piece
				OnPieceClonedToPanel (clone);
			}
		}

		#endregion
	}

}