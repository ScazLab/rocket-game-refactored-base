using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


namespace BuildARocketGame {
	
	public class Slot : MonoBehaviour, IDropHandler, IPointerClickHandler {

		public delegate void ClickForPanelChange(int pieceType);
		public static event ClickForPanelChange OnClickForPanelChange; 

		public delegate void PieceAddedToRocket(GameObject pieceAdded);
		public static event PieceAddedToRocket OnPieceAddedToRocket;

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
				// set the parent in the heirarchy to be the selected outline pieces that contains the rocket piece
				DragHandler.itemBeingDragged.transform.SetParent (transform);

				/* Note: pieces 'snap' to their slots as a result of having a horizontal layout group
			 	 *       on the selected outline piece that they're moving to	*/

				//Debug.Log ("End of drop: " + DragHandler.itemBeingDragged.name + " - to - " + gameObject.name);

				// If a piece has been dragged successfully, we'll let the game manager know
				OnPieceAddedToRocket (DragHandler.itemBeingDragged);
			}
		}
		#endregion

		#region IPointerClickHandler implementation

		public void OnPointerClick (PointerEventData eventData)
		{
			// if there's a click on a piece that's a dashed outline piece then we want to trigger
			// the animation that brings in the panels with that kind of piece
			if (isDashedOutlinePiece) {
				if (OnClickForPanelChange != null) {
					int selectedOutlineType = Constants.NONE_SELECTED;
					if (gameObject.tag == "Body") {
						selectedOutlineType = Constants.BODY;
					} else if (gameObject.tag == "LeftFin" || gameObject.tag == "RightFin") {
						selectedOutlineType = Constants.FIN;
					} else if (gameObject.tag == "TopCone") {
						selectedOutlineType = Constants.CONE;
					} else if (gameObject.tag == "Engine") {
						selectedOutlineType = Constants.BOOSTER;
					}
					OnClickForPanelChange (selectedOutlineType);
				}
			}
		}

		#endregion
	}
}