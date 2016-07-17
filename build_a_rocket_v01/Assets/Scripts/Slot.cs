using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


namespace BuildARocketGame {
	
	public class Slot : MonoBehaviour, IDropHandler, IPointerClickHandler {

		public delegate void ClickForPanelChange(int pieceType);
		public static event ClickForPanelChange OnClickForPanelChange; 

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
	//			ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x,y) => x.HasChanged());
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