using UnityEngine;
using System.Collections;

namespace BuildARocketGame {

	public class ObjectInfo : MonoBehaviour {
		public Vector3 lockPosition;
		public Vector3 initialLockPosition;
		public bool firstTouch = false;
		private float jumpSize;

		// indicating whether it is a leftover piece
		public bool isLeftoverPiece = false;


		//changing the sprite
		private Sprite sideSprite;
		public Sprite rocketSprite;
		public Sprite finSpriteRight;

		// indicates the location in the [Cone]PiecesArray in the TouchInputHandler, so the
		// correct piece will be cloned to replace it
		public int pos;

		//characteristic values
		public int weight = 10;
		public int fuel = 10;
		public int power = 10;
		public int airResistance = 10;

		public bool jump = true;

		//use this for keeping objects visible
		public bool seeMe = false;

		public bool added = false;
		// Use this for initialization
		public void Start () {
			Vector3 piecePosition = gameObject.transform.position;
			initialLockPosition = lockPosition = piecePosition;
			GameObject objLeft = GameObject.Find ("LeftPanel");
			jumpSize = objLeft.GetComponent<MeshRenderer> ().bounds.size.x;
			if (jump) {
				if (piecePosition.x < 0) { 
					lockPosition.x = piecePosition.x + jumpSize; 
				} else {
					lockPosition.x = piecePosition.x - jumpSize;
				}
			}

			sideSprite = gameObject.GetComponent<SpriteRenderer> ().sprite;
		}

		public void Update() {
			if (gameObject.transform.position == lockPosition && lockPosition != initialLockPosition) {
				GameObject.Find ("GameManager").GetComponent<TouchInputHandler> ().addToRocketPieces (gameObject);
			} 

			foreach (GameObject outline in GameObject.Find("GameManager").GetComponent<TouchInputHandler>().selectedOutlinePieces) {
				if (outline.transform.position == lockPosition) {
					outline.GetComponent<OutlineInfo> ().objectLocked = true;
				} 
			}
			if (rocketSprite != null) {
				if ((gameObject.transform.position != initialLockPosition || isLeftoverPiece) && 
				    gameObject.transform.position == lockPosition && 
				    lockPosition.x != 86 && 
				    lockPosition.x != -86) {
					if (finSpriteRight != null ) {
						if (lockPosition.x <0) {
							if (gameObject.GetComponent<SpriteRenderer>().sprite != rocketSprite) {
								gameObject.GetComponent<SpriteRenderer>().sprite = rocketSprite;
							}
						} else {
							if (gameObject.GetComponent<SpriteRenderer>().sprite != finSpriteRight) {
								gameObject.GetComponent<SpriteRenderer>().sprite = finSpriteRight;
							}
						}
					} else if (gameObject.GetComponent<SpriteRenderer> ().sprite != rocketSprite) {
							gameObject.GetComponent<SpriteRenderer> ().sprite = rocketSprite;
					}
				} else if (gameObject.GetComponent<SpriteRenderer> ().sprite != sideSprite) {
					gameObject.GetComponent<SpriteRenderer> ().sprite = sideSprite;
				}

			}
		}

		public int newLock (Vector3 newLock) {
			foreach (GameObject outline in GameObject.Find("GameManager").GetComponent<TouchInputHandler>().selectedOutlinePieces) {
				if (outline.transform.position == lockPosition) {
					outline.GetComponent<OutlineInfo> ().objectLocked = false;
				} 
			}
			lockPosition = newLock;
			if (lockPosition.x == initialLockPosition.x) {
				initialLockPosition = lockPosition;
			} //fixes jumbling error

			seeMe = true;
			gameObject.transform.parent = null;


			return 0;
		}

		public int reLock () {
			if (firstTouch) {
				gameObject.transform.position = lockPosition;
			}
			return 0;
		}
	}

}