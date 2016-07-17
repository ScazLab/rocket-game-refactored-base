using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace BuildARocketGame {

	public class TouchInputHandler : MonoBehaviour {

		private int containPos = 0;

		// all of the prefabs that we can touch/drag
		public GameObject[] conePieces;
		public GameObject[] finPieces;
		public GameObject[] bodyPieces;
		public GameObject[] boosterPieces;
		public GameObject[] outlinePieces;
		public GameObject[] selectedOutlinePieces;
		
		public GameObject[] trashcans;

		//lists for prefabs, not sure if arrays are still necessary but will leave them for now
		public List<GameObject> bodyPieceList = new List<GameObject> ();
		public List<GameObject> finPieceList = new List<GameObject> ();
		public List<GameObject> boosterPieceList = new List<GameObject> ();
		public List<GameObject> conePieceList = new List<GameObject> ();
		public List<GameObject> rocketPieces = new List<GameObject> ();

		public GameObject savedBodyPiece = null;

		// keep track of the last piece that was over the question mark
		private GameObject savedQuestionPiece = null;
		// and the question mark game object itself
		private GameObject questionMark = null;

		public int resistance = 0;
		public int power = 0;
		public int fuel = 0;
		public int weight = 0;
		
		// flags to be able to use with either mouse or touch or both
		public bool usingMouse = true;
		public bool usingTouch = true;

		public int pixelsPerUnit = 10;
		
		public GameObject[] savedPiece = new GameObject[10];
		
		// states of the build phase 
		private int currentState;
		private int nextState;
		private bool switching = false;
		private int switchDelay = 0;
		private const int nothingSelected = 0;
		private const int coneSelected = 1;
		private const int finSelected = 2;
		private const int bodySelected = 3;
		private const int boosterSelected = 4;
		
		// variables indicating whether a piece is a rocketPiece, pieceOutline, pieceSelectedOutline, or trashcan
		private const int rocketPiece = 0;
		private const int outlinePiece = 1;
		private const int selectedOutlinePiece = 2;
		private const int trashcan = 3;

		// values storing the old values of the 4 quantities
		private int resistance_old = 0;
		private int power_old = 0;
		private int fuel_old = 0;
		private int weight_old = 0;

		// variables indicating outline peiece identity
		private const int coneOutlinePiece = 0;
		private const int finOutlinePiece = 1;
		private const int bodyOutlinePiece = 2;
		private const int boosterOutlinePiece = 3;
		
		// camera settings
		private float cameraHeight;
		private float cameraWidth;
		
		// animator and related variables
		private Animator leftPanelAnimator;
		private Animator rightPanelAnimator;
		private bool firstStateChangeOccured = false;

		// game manager script
		private GameManager gameManagerScript;

		private bool ending = false;

		// List containing the positions of all of the touches
		private List<Vector3> touchPositions = new List<Vector3>();

		// the object used to send all the messages to Thalamus
		// ETHAN
		private ThalamusUnity thalamusUnity;

		// indicate whether we've started the game
		private bool startGame = false;
		
		// Use this for initialization
		void Start () {
			// set the state to nothingSelected
			currentState = nothingSelected;

			// get the Game Manager script 
			gameManagerScript = GameObject.Find ("GameManager").GetComponent<GameManager> ();

			// get the animators
			GameObject leftPiecePanel = GameObject.Find ("LeftPiecePanel");
			if (leftPiecePanel != null) {
				leftPanelAnimator = leftPiecePanel.GetComponent<Animator> ();
			}
			GameObject rightPiecePanel = GameObject.Find ("RightPiecePanel");
			if (rightPiecePanel != null) {
				rightPanelAnimator = rightPiecePanel.GetComponent<Animator>(); 
			}
			
			// obtain the width and height of the camera
			Camera cam = Camera.main;
			cameraHeight = 2f * cam.orthographicSize;
			cameraWidth = Mathf.Round (cameraHeight * cam.aspect);
			
			//Initialize our lists:
			foreach (GameObject piece in conePieces) {
				conePieceList.Add (piece);
			}
			foreach (GameObject piece in finPieces) {
				finPieceList.Add (piece);
			}
			foreach (GameObject piece in boosterPieces) {
				boosterPieceList.Add (piece);
			}
			foreach (GameObject piece in bodyPieces) {
				bodyPieceList.Add (piece);
			}

			// hide all of the pieces to start out
			hidePieceList (conePieceList);
			hidePieceList (finPieceList);
			hidePieceList (bodyPieceList);
			hidePieceList (boosterPieceList);
			
			// hide all of the selected outlines to start out
			hidePieces (selectedOutlinePieces);

			// initialize the thalamusUnity object
			// ETHAN
			if (gameManagerScript.sendThalamusMsgs) {
				thalamusUnity = new ThalamusUnity ();
			}
			
			// set the question mark object
			questionMark = GameObject.Find ("QuestionArea");

			// sets up the pieces that were there before 
			var savedVariablesScript = GameObject.Find ("SavedVariables").GetComponent<SavedVariables> ();
			foreach (SavedPieceInfo savedPiece in savedVariablesScript.previousTrialRocketPieces) {

				GameObject newObject = null;
				// 0 - cone, 1 - body, 2 - booster, 3 - fin
				if (savedPiece.pieceType == 0) {
					newObject = GameObjectUtil.Instantiate(conePieces[savedPiece.pos], savedPiece.vectorPos);
					conePieceList.Add (newObject);
					newObject.GetComponent<SpriteRenderer>().sprite = conePieces[savedPiece.pos].GetComponent<SpriteRenderer>().sprite;
					newObject.GetComponent<SpriteRenderer>().enabled = true;
				} else if (savedPiece.pieceType == 1) {
					newObject = GameObjectUtil.Instantiate(bodyPieces[savedPiece.pos], savedPiece.vectorPos);
					bodyPieceList.Add (newObject);
					newObject.GetComponent<SpriteRenderer>().sprite = bodyPieces[savedPiece.pos].GetComponent<SpriteRenderer>().sprite;
					newObject.GetComponent<SpriteRenderer>().enabled = true;
				} else if (savedPiece.pieceType == 2) {
					newObject = GameObjectUtil.Instantiate(boosterPieces[savedPiece.pos], savedPiece.vectorPos);
					boosterPieceList.Add (newObject);
					newObject.GetComponent<SpriteRenderer>().sprite = boosterPieces[savedPiece.pos].GetComponent<SpriteRenderer>().sprite;
					newObject.GetComponent<SpriteRenderer>().enabled = true;
				} else if (savedPiece.pieceType == 3) {
					newObject = GameObjectUtil.Instantiate(finPieces[savedPiece.pos], savedPiece.vectorPos);
					finPieceList.Add (newObject);
					newObject.GetComponent<SpriteRenderer>().sprite = finPieces[savedPiece.pos].GetComponent<SpriteRenderer>().sprite;
					newObject.GetComponent<SpriteRenderer>().enabled = true;
				} 
				// object info of the object we just created
				var newscript = newObject.GetComponent<ObjectInfo>();
				newscript.seeMe = true;
				newscript.jump = false;
				newscript.isLeftoverPiece = true;
				
				// sets the parent
				newObject.transform.parent = GameObject.Find("LeftoverPieces").transform;

				// add it to rocketPieces
				rocketPieces.Add(newObject);
			}
		}
		
		// Update is called once per frame
		void Update () {
			// if the game is not at the end or paused
			if (!gameManagerScript.paused && startGame) {
				// ending happens once the gameplay pause timer runs out 
				if (ending) {  
					int trialNum = GameObject.Find ("SavedVariables").GetComponent<SavedVariables> ().trialNumber;
					if (gameManagerScript.canRestart && 
						trialNum <= gameManagerScript.totalTrialsNumber) {
						Application.LoadLevel (Application.loadedLevel);
					} 
				}

				GameObject selectedBodyPiece = null;
				//count down the delay
				// can't re-select another outline piece until the timer has elapsed
				if (switchDelay > 0) {
					switchDelay -= 1;
				} else {
					switchDelay = 0;
				} 
				// for the mouse inputs
				if (usingMouse) {
					// if the left mouse button is clicking on our object
					if (Input.GetMouseButton (0)) {
						//if you click on the results object, reload the level

						// switching = panels going in
						if (switching == false) {
							selectedBodyPiece = MouseOverPiece (Input.mousePosition, rocketPiece, -1);
						} else {
							selectedBodyPiece = null;
						}

						// set and unset the layering order
						if (selectedBodyPiece != null)
						{
							selectedBodyPiece.GetComponent<SpriteRenderer> ().sortingLayerName = "Selected Piece";
						}

						//fixes bug where pieces jump around when you drag a piece over them
						if (selectedBodyPiece != savedBodyPiece && savedBodyPiece != null) {
							selectedBodyPiece = null;
						}
						//find the closest new lock position
						newLock (selectedBodyPiece);


						if (selectedBodyPiece != null) {
							// check if the selectedBodyPiece is over the question mark, if so, send message to Thalamus
							if (Contains (questionMark, Input.mousePosition) && 
							    selectedBodyPiece != savedQuestionPiece) {			
								// ETHAN
								// send the selected pieces to Thalamus
								if (gameManagerScript.sendThalamusMsgs) {
									thalamusUnity.Publisher.SentFromUnityToThalamus ("pieceQuestion*" + selectedBodyPiece.name);
								}
								
								// Debug.Log("pieceQuestion*" + selectedBodyPiece.name);

								// save the piece we just asked a question about, we can't ask a question about the same piece twice
								savedQuestionPiece = selectedBodyPiece;
							}
							// check if the selectedBodyPiece != savedBodyPiece, if so, send message to Thalamus
							// about the new selected piece
							/*else if (selectedBodyPiece != savedBodyPiece) {			
								// send the selected pieces to Thalamus
								// thalamusUnity.Publisher.SentFromUnityToThalamus ("pieceSelected*" + selectedBodyPiece.name);

								//Debug.Log("pieceSelected*" + selectedBodyPiece.name);
							}*/
						}

						//save it globally so we can operate on it in the next step if we want
						savedBodyPiece = selectedBodyPiece;
						//only select outline under certain conditions
						GameObject selectedOutlinePiece;
						if (selectedBodyPiece == null && switching == false && switchDelay == 0) {
							selectedOutlinePiece = MouseOverPiece (Input.mousePosition, outlinePiece, -1);
						} else {
							selectedOutlinePiece = null;
						}
					
						if (selectedBodyPiece != null) {
							Vector3 mousePos = Input.mousePosition;
						
							if (selectedBodyPiece.GetComponent<ObjectInfo> ().firstTouch == false) {
								selectedBodyPiece.GetComponent<ObjectInfo> ().firstTouch = true;
							}
						
							Vector3 piecePosition = new Vector3 ((mousePos.x * cameraWidth / Screen.width) - (cameraWidth / 2), 
						                                    (mousePos.y * cameraHeight / Screen.height) - (cameraHeight / 2), 
						                                    0);
						
							selectedBodyPiece.transform.position = piecePosition;
						
						} else if (selectedOutlinePiece != null) {
							string pieceName = selectedOutlinePiece.name;
							nextState = -1;
							if (pieceName.Contains ("top")) {
								nextState = coneSelected;
							} else if (pieceName.Contains ("right") || pieceName.Contains ("left")) {
								nextState = finSelected;
							} else if (pieceName.Contains ("body")) {
								nextState = bodySelected;
							} else if (pieceName.Contains ("engine")) {
								nextState = boosterSelected;
							}

							// locks after the state has changed 
							lockAll ();

							// set the next state if not the current state
							if (nextState >= 0 && nextState != currentState) {
								switching = true;
								if (firstStateChangeOccured == true) {
									// play the animations to hide the sidebars
									leftPanelAnimator.SetTrigger ("stateChangeTriggerLeft");
									rightPanelAnimator.SetTrigger ("stateChangeTriggerRight");
								} else {
									leftPanelAnimator.SetBool ("firstStateSelectedLeft", true);
									rightPanelAnimator.SetBool ("firstStateSelectedRight", true);
								}
							}
						}
					
					} else {//mouse isnt clicking
						if (!switching) {
							lockAll ();
							if (savedBodyPiece!= null)
							{
								savedBodyPiece.GetComponent<SpriteRenderer> ().sortingLayerName = "Unselected Piece";
							}
							//the step after we stop selecting a piece, add that piece to the rocketPieces list
	//						Debug.Log("When the mouse isn't clicking and usingMouse is on, call addToRocketPieces()");
							addToRocketPieces (savedBodyPiece);
							checkTrash (savedBodyPiece);
							savedBodyPiece = null;
						}
					}
				}

				// for use of touch input only
				else if (usingTouch) {
					// if the left mouse button is clicking on our object
					if (Input.touchCount > 0) {
						//get our touch positions
						//Vector3[] touchPosition;
						//touchPosition = new Vector3[Input.touchCount];
						touchPositions.Clear ();
					
						int i = 0;
						foreach (Touch touch in Input.touches) {
							//touchPosition [i] = new Vector3 (0, 0, 0);
							//touchPosition [i] = touch.position;
							touchPositions.Add(touch.position);
							i++;
						}
					
						GameObject[] selectedPiece;
						selectedPiece = new GameObject[Input.touchCount];
					
						if (switching == false) {
							i = 0;
							foreach (Vector3 touch in touchPositions) {
								containPos = i;
								selectedPiece [i] = MouseOverPiece (touch, rocketPiece, Input.touchCount);
								//find the closest new lock position
								newLock (selectedPiece [i]);

								if (selectedPiece[i] != null)
								{
									selectedPiece[i].GetComponent<SpriteRenderer> ().sortingLayerName = "Selected Piece";
								}
								
								bool isSaved = false;
								foreach (GameObject piece in savedPiece) {
									if (piece == selectedPiece [i]) {
										isSaved = true;
									}
								}

								if (selectedPiece[i] != null) {
									if (Contains (questionMark, touch) && 
									    selectedPiece[i] != savedQuestionPiece) {			
										// ETHAN
										// send the selected pieces to Thalamus
										if (gameManagerScript.sendThalamusMsgs) {
											thalamusUnity.Publisher.SentFromUnityToThalamus ("pieceQuestion*" + selectedPiece [i].name);
										}
										
										// Debug.Log("pieceQuestion*" + selectedPiece[i].name);
										
										// save the piece we just asked a question about, we can't ask a question about the same piece twice
										savedQuestionPiece = selectedPiece[i];
									} /*else if (isSaved == false) {
										// send the selected pieces to Thalamus
										// thalamusUnity.Publisher.SentFromUnityToThalamus ("pieceSelected*" + selectedPiece[i].name);
										
										// Debug.Log("pieceSelected*" + selectedPiece[i].name);
									}*/
								}

								i++;
							}
							containPos = 0;
						} 
						// if we are switching
						else {
							for (i = 0; i<Input.touchCount; i++) {
								selectedPiece [i] = null; //if we are switching then we should be able to select pieces

							}
						}
					
					
					
						//only select outline under certain conditions
						GameObject selectedOutlinePiece = null;
						if (touchPositions.Count > 0)
						{
							if (selectedPiece [0] == null && switching == false && switchDelay == 0) {
								selectedOutlinePiece = MouseOverPiece (touchPositions [0], outlinePiece, Input.touchCount); //FIX THIS, IT ONLY RECORDS THE FIRST TOUCH
							} else {
								selectedOutlinePiece = null;
							}
						}
					
						// allnull is true when all the selected pieces are null
						bool allnull = true;

						foreach (GameObject selected in selectedPiece) {
							if (selected != null) {
								allnull = false;
							}
						}

						//if we have at least one selected rocket piece
						if (!allnull) { 
							i = 0;
							foreach (GameObject selected in selectedPiece) { //do the code from the mouse section for each piece.
								if (selected != null && i < touchPositions.Count) {
									Vector3 mousePos = touchPositions [i];
								
									if (selected.GetComponent<ObjectInfo> ().firstTouch == false) {
										selected.GetComponent<ObjectInfo> ().firstTouch = true;
									}
								
									Vector3 piecePosition = new Vector3 ((mousePos.x * cameraWidth / Screen.width) - (cameraWidth / 2), 
								                                    (mousePos.y * cameraHeight / Screen.height) - (cameraHeight / 2), 
								                                    0);
								
									selected.transform.position = piecePosition;
								}
								i++;
							}
						
						}
						// if we have 1 touch and it's an outline piece
						else if (selectedOutlinePiece != null) { 
							string pieceName = selectedOutlinePiece.name;
							nextState = -1;
							if (pieceName.Contains ("top")) {
								nextState = coneSelected;
							} else if (pieceName.Contains ("right") || pieceName.Contains ("left")) {
								nextState = finSelected;
							} else if (pieceName.Contains ("body")) {
								nextState = bodySelected;
							} else if (pieceName.Contains ("engine")) {
								nextState = boosterSelected;
							}
							lockAll ();
							// set the next state if not the current state
							if (nextState >= 0 && nextState != currentState) {
								switching = true;
								if (firstStateChangeOccured == true) {
									// play the animations to hide the sidebars
									leftPanelAnimator.SetTrigger ("stateChangeTriggerLeft");
									rightPanelAnimator.SetTrigger ("stateChangeTriggerRight");
								} else {
									leftPanelAnimator.SetBool ("firstStateSelectedLeft", true);
									rightPanelAnimator.SetBool ("firstStateSelectedRight", true);
								}
							}
						}

						// we want to trash all the saved pieces that are not currently selected 
						int k = 0;
						for (k = 0; k<10; k++) {
							bool isselected = false;
							foreach (GameObject selected in selectedPiece) { 
								if (selected == savedPiece [k]) { // (the saved piece array is larger than the number of touches)
									isselected = true;
								}
							}
							if (!isselected) {
								checkTrash (savedPiece [k]); //check trash deletes pieces if they are close to the trash
							}
						}

						if (!switching) {
						
							for (int q = 0; q < 10; q++) {
								bool savednotselected = true;
								foreach (GameObject selected in selectedPiece) {
									if (savedPiece [q] == selected) {
										savednotselected = false;
									}
								}

								// we add pieces that were selected the step before but are no longer selected, ie they have been locked in
								if (savednotselected && savedPiece[q] != null) {
									Debug.Log("Savednotselected call to addToRocketPieces()");
									addToRocketPieces (savedPiece [q]); 
									if (savedPiece[q] != null)
									{
										savedPiece[q].GetComponent<SpriteRenderer> ().sortingLayerName = "Unselected Piece";
									}
									//the function itself makes sure that they are actually locked into a rocket piece slot and not the initial location or a trashcan
								}
								
								//save it globally so we can operate on it in the next step if we want. We need to have access to pieces that stop being selected
								for (int m = 0; m<Input.touchCount; m++) {
									savedPiece [m] = selectedPiece [m];
								}

								lockListExceptionArray (selectedPiece, conePieceList); //this could be simpler, so fix if we have lag
								lockListExceptionArray (selectedPiece, boosterPieceList); 
								lockListExceptionArray (selectedPiece, bodyPieceList);
								lockListExceptionArray (selectedPiece, finPieceList);
						
							}
						}
					
					
					}
					// If there are no touches
					else {
						if (!switching) {
							lockAll ();
							//the step after we stop selecting a piece, add that piece to the rocketPieces list
							int i = 0;
							for (i = 0; i<10; i++) {
								if (savedPiece[i] != null)
								{
									savedPiece[i].GetComponent<SpriteRenderer> ().sortingLayerName = "Unselected Piece";
								}
	//							Debug.Log("When there are no touches, calling addToRocketPieces()");
								addToRocketPieces (savedPiece [i]); //once we add, we can check the trash and then clear the array because its sole purpose is to be used in addToRocketPieces
								checkTrash (savedPiece [i]);
								savedPiece [i] = null;
							}
						}
					}

				}
				//get the current state
				AnimatorStateInfo currentRightPanelBaseState = rightPanelAnimator.GetCurrentAnimatorStateInfo (0);
				AnimatorStateInfo currentLeftPanelBaseState = leftPanelAnimator.GetCurrentAnimatorStateInfo (0);


				if (switching && currentRightPanelBaseState.IsName ("Base Layer.RightPanelIn") && currentLeftPanelBaseState.IsName ("Base Layer.LeftPanelIn")) {
					if (firstStateChangeOccured == true) {
						// hide the old pieces + the old selected outline
						if (currentState == coneSelected) {
							hideOutlinePieces (coneOutlinePiece, true);
							showOutlinePieces (coneOutlinePiece, false);
							hidePieceList (conePieceList);
						} else if (currentState == finSelected) {
							hideOutlinePieces (finOutlinePiece, true);
							showOutlinePieces (finOutlinePiece, false);
							hidePieceList (finPieceList);
						} else if (currentState == bodySelected) {
							hideOutlinePieces (bodyOutlinePiece, true);
							showOutlinePieces (bodyOutlinePiece, false);
							hidePieceList (bodyPieceList);
						} else if (currentState == boosterSelected) {
							hideOutlinePieces (boosterOutlinePiece, true);
							showOutlinePieces (boosterOutlinePiece, false);
							hidePieceList (boosterPieceList);
						}
					} else {
						firstStateChangeOccured = true;
					}
				
					// show the new pieces
					if (nextState == coneSelected) {
						hideOutlinePieces (coneOutlinePiece, false);
						showOutlinePieces (coneOutlinePiece, true);
						showPieceList (conePieceList);
					} else if (nextState == finSelected) {
						hideOutlinePieces (finOutlinePiece, false);
						showOutlinePieces (finOutlinePiece, true);
						showPieceList (finPieceList);
					} else if (nextState == bodySelected) {
						hideOutlinePieces (bodyOutlinePiece, false);
						showOutlinePieces (bodyOutlinePiece, true);
						showPieceList (bodyPieceList);
					} else if (nextState == boosterSelected) {
						hideOutlinePieces (boosterOutlinePiece, false);
						showOutlinePieces (boosterOutlinePiece, true);
						showPieceList (boosterPieceList);
					}
				
					// set the global variable to indicate the currentState
					currentState = nextState;
					switching = false;
					//I fixed a bunch of bugs with this... its hacky but it works
					switchDelay = 60; //should be 62

					// when we know we're going to switch, reset all the 'first touch' values so that
					// they won't be locked to a particular position and will be moved with the panels
					// for the animation
					firstTouchReset ();
				}

				// reset all values to 0 (and then compute them based on the pieces on the rocket)
				resistance = 0;
				power = 0;
				fuel = 0;
				weight = 0;

				//update all of the values of the rocket
				foreach (GameObject piece in rocketPieces) {
					weight += piece.GetComponent<ObjectInfo> ().weight;
					resistance += piece.GetComponent<ObjectInfo> ().airResistance;
					fuel += piece.GetComponent<ObjectInfo> ().fuel;
					power += piece.GetComponent<ObjectInfo> ().power;
				}
				if (resistance < 0) {
					resistance = 0;
				}

				// only if there was a change in the stats, do we update the diplay 
				if (weight != weight_old || resistance != resistance_old || fuel != fuel_old || power != power_old) {

					// update the display
					gameManagerScript.weight.GetComponent<Text>().text = weight.ToString ();
					gameManagerScript.airResistance.GetComponent<Text>().text = resistance.ToString ();
					gameManagerScript.fuel.GetComponent<Text>().text = fuel.ToString ();
					gameManagerScript.power.GetComponent<Text>().text = power.ToString ();

					// update the 'old' values
					weight_old = weight;
					resistance_old = resistance;
					fuel_old = fuel;
					power_old = power;
				
					// ETHAN
					// send the stats to Thalamus
					if (gameManagerScript.sendThalamusMsgs) {
						thalamusUnity.Publisher.SentFromUnityToThalamus ("stats*" + weight.ToString () + "*" + fuel.ToString () + "*" + resistance.ToString () + "*" + power.ToString ());
					}

					//Debug.Log ("stats*" + weight.ToString() + "*" + fuel.ToString() + "*" + resistance.ToString() + "*" + power.ToString());
				}
			} else if (gameManagerScript.paused)
			{
				hideOutlinePieces (coneOutlinePiece, false);
				hideOutlinePieces (finOutlinePiece, false);
				hideOutlinePieces (bodyOutlinePiece, false);
				hideOutlinePieces (boosterOutlinePiece, false);

				foreach (GameObject piece in rocketPieces)
				{
					piece.GetComponent<SpriteRenderer> ().enabled = false;
				}

			} else {
				if (gameManagerScript.gameStarted) {
					startGame = true;
				}
			}
		}
		void hideOutlinePieces (int pieceType, bool isHighlightedOutline) {
			
			GameObject[] pieces;
			if (isHighlightedOutline) {
				pieces = selectedOutlinePieces;
			} else {
				pieces = outlinePieces;
			}
			
			foreach (GameObject piece in pieces) {
				string pieceName = piece.name;
				if (pieceName.Contains("top") && pieceType == coneOutlinePiece) {
					piece.GetComponent<SpriteRenderer> ().enabled = false; 
				} else if (pieceName.Contains("fin") && pieceType == finOutlinePiece) {
					piece.GetComponent<SpriteRenderer> ().enabled = false; 
				} else if (pieceName.Contains("body") && pieceType == bodyOutlinePiece) {
					piece.GetComponent<SpriteRenderer> ().enabled = false; 
				} else if (pieceName.Contains("engine") && pieceType == boosterOutlinePiece) {
					piece.GetComponent<SpriteRenderer> ().enabled = false; 
				}
			}
		}


		
		//hide the pieces, but not if they are locked into a rocket
		void hidePieces (GameObject[] pieces) {
			if (pieces != boosterPieces && pieces != conePieces && pieces != bodyPieces && pieces != finPieces) {
				foreach (GameObject piece in pieces) {
					piece.GetComponent<SpriteRenderer> ().enabled = false; 
				} 
			}else {
				foreach (GameObject piece in pieces) {
					if (piece.GetComponent<ObjectInfo>().seeMe == false) {
						piece.GetComponent<SpriteRenderer> ().enabled = false; 
					}
				}
			}
		}
		//hide each piece in a pieceList
		void hidePieceList (List<GameObject> pieces) {
			foreach (GameObject piece in pieces) {
				if (piece.GetComponent<ObjectInfo>().seeMe == false) {
					piece.GetComponent<SpriteRenderer> ().enabled = false; 
				}
			}
		}
		//manually hide a piece whether or not it is locked into rocket
		void hidePiece (GameObject piece) {
			piece.GetComponent<SpriteRenderer>().enabled = false; 
		}
		
		void showOutlinePieces (int pieceType, bool isHighlightedOutline) {
			
			GameObject[] pieces;
			if (isHighlightedOutline) {
				pieces = selectedOutlinePieces;
			} else {
				pieces = outlinePieces;
			}
			
			foreach (GameObject piece in pieces) {
				string pieceName = piece.name;
				if (pieceName.Contains("top") && pieceType == coneOutlinePiece) {
					piece.GetComponent<SpriteRenderer> ().enabled = true; 
				} else if ((pieceName.Contains("right") || pieceName.Contains("left")) && pieceType == finOutlinePiece) {
					piece.GetComponent<SpriteRenderer> ().enabled = true; 
				} else if (pieceName.Contains("body") && pieceType == bodyOutlinePiece) {
					piece.GetComponent<SpriteRenderer> ().enabled = true; 
				} else if (pieceName.Contains("engine") && pieceType == boosterOutlinePiece) {
					piece.GetComponent<SpriteRenderer> ().enabled = true; 
				}
			}
		}

		void showPieces (GameObject[] pieces) {
			foreach (GameObject piece in pieces) {

				piece.GetComponent<SpriteRenderer>().enabled = true; 
				
			}
		}
		void showPieceList(List<GameObject> pieces) {
			foreach (GameObject piece in pieces) {
				piece.GetComponent<SpriteRenderer>().enabled = true; 
			}
		}
		
		void showPiece (GameObject piece) {
			piece.GetComponent<SpriteRenderer>().enabled = true; 
		}
		
		//should set all non outline objects back to their lock positions. call this before setting dragged object positions
		void lockSet(List<GameObject> pieces) {
			foreach (GameObject piece in pieces) {
				//Debug.Log (piece);
				if (piece != null) {
				piece.GetComponent<ObjectInfo> ().reLock (); 
				}
			}
		}
		void lockListException (List<GameObject> pieces, GameObject exception) {
			foreach (GameObject piece in pieces) {
				if (piece != exception) {
					piece.GetComponent<ObjectInfo> ().reLock ();
				}
			}
		}
		void lockListExceptionArray(GameObject[] array, List<GameObject> list) {
			foreach (GameObject piece in list) {
				bool notequal = true;
				foreach (GameObject except in array) {
					if (piece == except) {
						notequal = false;
					}
				}
				if (notequal)
					piece.GetComponent<ObjectInfo> ().reLock ();
			}
		}



		void lockAll() {
			lockSet (boosterPieceList);
			lockSet (conePieceList);
			lockSet (bodyPieceList);
			lockSet (finPieceList);
		}
		void firstTouch (List<GameObject> pieces) {
			foreach (GameObject piece in pieces) {
				piece.GetComponent<ObjectInfo> ().firstTouch = false;
			}
		}
		
		void firstTouchReset() {
			firstTouch (boosterPieceList);
			firstTouch (finPieceList);
			firstTouch (bodyPieceList);
			firstTouch (conePieceList);
		}
		
		void newLock(GameObject piece) { //finds a new lock position for a game object by looping through selected outline pieces
			if (piece != null) {
				float distance;
				float minDistance = Vector3.Distance (piece.transform.position, piece.GetComponent<ObjectInfo> ().lockPosition) / 3;
				float saveMinDistance = minDistance;
				Vector3 newLock = piece.GetComponent<ObjectInfo> ().lockPosition;
				bool trashpossible = false;
				if (rocketPieces.Contains(piece)) {
					trashpossible = true;
				}
			
				foreach (GameObject outline in selectedOutlinePieces) {
					// checks for the yellow outlines
					if (outline.GetComponent<SpriteRenderer> ().enabled == true   ) {
						// Debug.Log (outline.GetComponent<OutlineInfo>().objectLocked);
						if (true)//outline.GetComponent<OutlineInfo>().objectLocked == false) 
					{
						distance = Vector3.Distance (outline.transform.position, piece.transform.position);
						if (minDistance > distance && !collisionLoop (outline.transform.position)) {
							minDistance = distance;
							newLock = outline.transform.position;
						}
						
					}
					
					
					}
				}
				if (trashpossible) {
					foreach (GameObject trash in trashcans) {
						distance = Vector3.Distance (trash.transform.position, piece.transform.position);
						if (4*minDistance > distance) {
							minDistance = distance;
							newLock = trash.transform.position;
						}
					}
				}

				if (minDistance != saveMinDistance) {
					piece.GetComponent<ObjectInfo> ().newLock (newLock);
				}


			}
		}

		public void addToRocketPieces(GameObject selectedBodyPiece) { //if a piece is locked into the rocket and has not yet been added to the 
			//rocket pieces list, this function will add it. 
			//this function is also responsible for isntantiating new clones of old objects at the moment they are added to the rocket piece list


			//if the piece is locked into the body add it to the list of rocket pieces
			if (selectedBodyPiece != null) {

				var script = selectedBodyPiece.GetComponent<ObjectInfo> ();



	//			Debug.Log("Calling 'addToRocketPieces' on: " + selectedBodyPiece.name);
	//			Debug.Log("See me: " + script.seeMe.ToString());
	//			Debug.Log("Current position: " + selectedBodyPiece.transform.position.ToString());
	//			Debug.Log("Lock position: " + script.lockPosition.ToString());
	//			Debug.Log ("Initial Lock position: " + script.initialLockPosition.ToString());
	//			Debug.Log("Script added: " + script.added.ToString());
				
				// script.added indicates whether it's already been added to the rocketPieces list
				if (script.seeMe == true && selectedBodyPiece.transform.position == script.lockPosition &&
					script.lockPosition != script.initialLockPosition && script.added == false) {

	//				Debug.Log("Adding " + selectedBodyPiece.name + " to rocket pieces");

					rocketPieces.Add (selectedBodyPiece);
					script.added = true;
					

					Vector3 lockP = script.initialLockPosition; //we fix the lock position because the movement of the panels makes things screwy.
					//furthermore they jump to the left/right when initialized
					if (selectedBodyPiece.GetComponent<ObjectInfo>().isLeftoverPiece) {
						lockP.x = 250;
						lockP.y = 50;
					} else if (lockP.x <= 0) {
						lockP.x = -86;
						}else {
						lockP.x= 86;
					}
					int pos = selectedBodyPiece.GetComponent<ObjectInfo>().pos;


					GameObject newObject = null;


					if (currentState == coneSelected) {
						newObject = GameObjectUtil.Instantiate(conePieces[pos], lockP);
						conePieceList.Add (newObject);
						newObject.GetComponent<SpriteRenderer>().sprite = selectedBodyPiece.GetComponent<SpriteRenderer>().sprite;
					} else
					if (currentState == finSelected) {
						newObject = GameObjectUtil.Instantiate(finPieces[pos], lockP);
						finPieceList.Add (newObject);
						newObject.GetComponent<SpriteRenderer>().sprite = selectedBodyPiece.GetComponent<SpriteRenderer>().sprite;
					} else 
					if (currentState == bodySelected) {
						newObject = GameObjectUtil.Instantiate(bodyPieces[pos], lockP);
						bodyPieceList.Add (newObject);
						newObject.GetComponent<SpriteRenderer>().sprite = selectedBodyPiece.GetComponent<SpriteRenderer>().sprite;
					} else
					if (currentState == boosterSelected) {
						newObject = GameObjectUtil.Instantiate(boosterPieces[pos], lockP);
						boosterPieceList.Add (newObject);
						newObject.GetComponent<SpriteRenderer>().sprite = selectedBodyPiece.GetComponent<SpriteRenderer>().sprite;
					} 

					// object info of the object we just created
					var newscript = newObject.GetComponent<ObjectInfo>();
					newscript.seeMe = false;
					newscript.initialLockPosition = newObject.transform.position = lockP; //also questionable line of code
					newscript.jump = false;

					// sets the parent
					if (newObject.transform.position.x <0) {
						newObject.transform.parent = GameObject.Find("LeftPiecePanel").transform;
					} else {
						newObject.transform.parent = GameObject.Find("RightPiecePanel").transform;
					}

				}
			}
		}

		/*void searchToAdd() { //in theory this should add pieces which are locked onto the rocket but have somehow not been added.
			//it currently isnt being used
			search (conePieceList);
			search (boosterPieceList);
			search (bodyPieceList);
			search (finPieceList);
		}

		void search (List<GameObject> list) {
			foreach (GameObject piece in list) {
				if (piece.GetComponent<ObjectInfo> ().lockPosition == piece.transform.position &&
				    piece.GetComponent<ObjectInfo>().lockPosition == piece.GetComponent<ObjectInfo>().initialLockPosition) {
					addToRocketPieces (piece);
				}
			}
		}*/

		int determinePieceType(string pieceName) {
			// 0 - cone, 1 - body, 2 - booster, 3 - fin
			if (pieceName.Contains ("body_")) {
				return 1;
			} else if (pieceName.Contains ("cone_")) {
				return 0;
			} else if (pieceName.Contains ("engine_")) {
				return 2;
			} else if (pieceName.Contains ("fin_")) {
				return 3;
			}
			return -1;
		}

		//this is called to destroy objects which are close to the trashcans or far from the ship
		void checkTrash(GameObject selected) { 
			for(var i = rocketPieces.Count - 1; i > -1; i--) {
				GameObject selectedBodyPiece = rocketPieces[i];
				if (selectedBodyPiece != null) {
					foreach (GameObject trashcan in trashcans) {
						// Abs value stuff is about being too far away from the center line - trashing it
						if (Vector3.Distance (selectedBodyPiece.transform.position, trashcan.transform.position) < 10 ||
						    (Mathf.Abs(selectedBodyPiece.transform.position.x) > 70 && selectedBodyPiece.GetComponent<ObjectInfo>().lockPosition !=
						 selectedBodyPiece.GetComponent<ObjectInfo>().initialLockPosition) ) {
							removeFromRocketPieces (selectedBodyPiece);
							break;
						}
					}
				}
			}
		}

		void removeFromRocketPieces(GameObject selectedBodyPiece) {
			if (currentState == coneSelected) {
				conePieceList.Remove (selectedBodyPiece);
			} else
			if (currentState == finSelected) {
				finPieceList.Remove (selectedBodyPiece);
			} else 
			if (currentState == bodySelected) {
				bodyPieceList.Remove (selectedBodyPiece);
			} else
			if (currentState == boosterSelected) {
				boosterPieceList.Remove (selectedBodyPiece);
			} 
			rocketPieces.Remove (selectedBodyPiece);
			GameObjectUtil.Destroy (selectedBodyPiece);
		}

		
		/* Returns the GameObject of the piece that the mouse is over or null if the mouse isn't over a piece */
		GameObject MouseOverPiece(Vector3 mousePos, int pieceType, int touchCount) {
			// initialize outputs
			GameObject output = null;

			// check if the last selected piece is pieceType
			// if it is then see if my mouse is over that piece
			// if so return it
			if (usingMouse) {
				if (savedBodyPiece != null) {
					if (Contains (savedBodyPiece, mousePos) || savedBodyPiece.transform.position != savedBodyPiece.GetComponent<ObjectInfo>().lockPosition) {
						output = savedBodyPiece;
						return output;
					}
				}
			} else if (usingTouch) { //if we are still over a saved piece, that piece should have priority
				// getting the # of saved pieces
				int numSavedPieces = 0;
				foreach (GameObject s in savedPiece) {
					if (s != null)
					{
						numSavedPieces++;
					}
				}

				foreach (GameObject saved in savedPiece) {
					// if our touch isn't directly over the object, we assume that it is still being touched if it's
					// in the saved pieces, as long as the number of touches hasn't changed

					if (saved != null) {
						if (Contains (saved, mousePos) || ((saved.transform.position != saved.GetComponent<ObjectInfo>().lockPosition) 
						                                   && saved == savedPiece[containPos]) && touchCount == numSavedPieces)  {
							output = saved;
							return output;
						}
					}
				}
			}

			GameObject[] pieces = null;
			if (pieceType == rocketPiece) {
				if (currentState == coneSelected) {
					pieces = conePieces;
				} else if (currentState == finSelected) {
					pieces = finPieces;
				} else if (currentState == bodySelected) {
					pieces = bodyPieces;
				} else if (currentState == boosterSelected) {
					pieces = boosterPieces;
				} else {
					return output;
				}
			} else if (pieceType == outlinePiece) {
				pieces = outlinePieces;
			} else if (pieceType == selectedOutlinePiece) {
				pieces = selectedOutlinePieces;
			} else if (pieceType == trashcan) {
				pieces = trashcans;
			}

			if (pieces != conePieces && pieces != finPieces && pieces != bodyPieces && pieces != boosterPieces) {
				// Detect whether the mouse position is within one of the pieces
				foreach (GameObject piece in pieces) {
					if (Contains (piece, mousePos)) {
						output = piece;
						break;
					}
				}
			} else {
				List<GameObject> pieceList = null;
				if (pieces == conePieces) {
					pieceList = conePieceList;
				}
				if (pieces == finPieces) {
					pieceList = finPieceList;
				}
				if (pieces == boosterPieces) {
					pieceList = boosterPieceList;
				}
				if (pieces == bodyPieces) {
					pieceList = bodyPieceList;
				}
				foreach (GameObject piece in pieceList) {
					if (Contains (piece, mousePos)) {
						output = piece;
						break;
					}
				}
				/*foreach (GameObject piece in rocketPieces) {
					if (Contains (piece, mousePos)) {
						output = piece;
						break;
					}
				}*/ //add in if we want to be able to move pieces that arent selected
			}

			return output;
		}

		
		/* Returns true if the touchItem location within the container, which must have a SpriteRenderer */
		public bool Contains (GameObject container, Vector3 touchItem) {
			// get the position, width, and height of the container GameObject
			Vector3 containerPosition = container.transform.position;
			float containerWidth = container.GetComponent<SpriteRenderer> ().bounds.size.x;
			float containerHeight = container.GetComponent<SpriteRenderer> ().bounds.size.y;

			// if it's one of the flatter pieces that we manually added a pivot to
			if (container.name.Contains ("cone_Flat") || container.name.Contains ("cone_Equilateral")) {
				string spriteName = container.GetComponent<SpriteRenderer> ().sprite.name;
				// if they are on the rocket
				if (spriteName.Contains("cone-5001r")) {
					containerPosition.y = containerPosition.y - 5;
				} else if (spriteName.Contains("cone-3003-shortr")) {
					containerPosition.y = containerPosition.y - 5;
				}
			}
		
			// the mouse (0,0) is at the bottom left corner of the screen, not the middle like the container
			// so we need to have a scaled mouse position
			float touchItemScaledx = (touchItem.x * cameraWidth / Screen.width) - (cameraWidth / 2);
			float touchItemScaledy = (touchItem.y * cameraHeight / Screen.height) - (cameraHeight / 2);
		
			if (touchItemScaledx >= (containerPosition.x - containerWidth / 2) && touchItemScaledx <= (containerPosition.x + containerWidth / 2)
				&& touchItemScaledy >= (containerPosition.y - containerHeight / 2) && touchItemScaledy <= (containerPosition.y + containerHeight / 2)) {
				return true;
			} else {
				return false;
			}
		}


		
		//this checks for a collision at a point with all of the rocket objects
		bool collisionLoop(Vector3 pos) {
			foreach (GameObject obj in boosterPieceList) {
				if (obj.transform.position == pos) {
					return true;
				}
			}
			foreach (GameObject obj in conePieceList) {
				if (obj.transform.position == pos) {
					return true;
				}
			}
			foreach (GameObject obj in finPieceList) {
				if (obj.transform.position == pos) {
					return true;
				}
			}
			foreach (GameObject obj in bodyPieceList) {
				if (obj.transform.position == pos) {
					return true;
				}
			}
			return false;
		}

		//to be triggered by GameManager when the liftoff happens
		public void endSequence() {
			lockAll ();
			firstTouchReset ();
			hideOutlinePieces (coneOutlinePiece, true);
			hideOutlinePieces (bodyOutlinePiece, true);
			hideOutlinePieces (boosterOutlinePiece, true);
			hideOutlinePieces (finOutlinePiece, true);
			showOutlinePieces (coneOutlinePiece, false);
			showOutlinePieces (finOutlinePiece, false);
			showOutlinePieces (bodyOutlinePiece, false);
			showOutlinePieces (boosterOutlinePiece, false);
			foreach (GameObject piece in rocketPieces) {
				foreach (GameObject outline in outlinePieces) {
					if (outline.transform.position == piece.transform.position) {
						outline.GetComponent<SpriteRenderer> ().enabled = false;
					}
				}
			}
			leftPanelAnimator.SetTrigger ("stateChangeTriggerLeft");
			leftPanelAnimator.SetTrigger ("stateChangeTriggerTakeoff");
			rightPanelAnimator.SetTrigger ("stateChangeTriggerRight");
			rightPanelAnimator.SetTrigger ("stateChangeTriggerTakeoff");
			ending = true;

			// save the pieces and their positions that were in the rocket during this trial
			var savedVariablesScript = GameObject.Find ("SavedVariables").GetComponent<SavedVariables> ();
			savedVariablesScript.previousTrialRocketPieces.Clear ();
			foreach (GameObject piece in rocketPieces) {
				SavedPieceInfo newSavedPiece = new SavedPieceInfo(piece.GetComponent<ObjectInfo>().initialLockPosition, 
				                                                  determinePieceType(piece.name), 
				                                                  piece.GetComponent<ObjectInfo>().pos, 
				                                                  piece.transform.position);
				savedVariablesScript.previousTrialRocketPieces.Add (newSavedPiece);
			}
		}

		//calculates how far the rocket should go
		public int calculateDistance() {

			List<int> rocketPieceTypes = countNumBodyPieceTypes (rocketPieces);
			int numConePieces = rocketPieceTypes [0];
			int numBodyPieces = rocketPieceTypes [1];
			int numBoosterPieces = rocketPieceTypes [2];
			int numFinPieces = rocketPieceTypes [3];

			// when penalizing missing pieces, we'll penalize the cone and fin pieces 3 times as much
			float penalty = (float)(numConePieces * 3 + numBodyPieces + numBoosterPieces + numFinPieces * 3) / (float)(1 * 3 + 16 + 4 + 2 * 3);

			float distanceTemp = (float)((-1.0 * ((float)resistance + 1.5 * (float)weight) + (float)fuel + ((float)fuel * (float)power) * 0.002 + 250.0) * (penalty));

			if (distanceTemp < 0 || fuel == 0 || numBoosterPieces == 0) {
				return 0;
			} else {
				return (int)distanceTemp;
			}
		}

		List<int> countNumBodyPieceTypes (List<GameObject> pieces)
		{
			// piece types
			int conePieces = 0;
			int bodyPieces = 0;
			int boosterPieces = 0;
			int finPieces = 0;

			int pieceType; // 0 - cone, 1 - body, 2 - booster, 3 - fin
			foreach (GameObject piece in pieces) {
				pieceType = determinePieceType(piece.name);
				if (pieceType == 0) conePieces++;
				else if (pieceType == 1) bodyPieces++;
				else if (pieceType == 2) boosterPieces++;
				else if (pieceType == 3) finPieces++;
			}

			// add them to the output list
			List<int> output = new List<int> () {conePieces, bodyPieces, boosterPieces, finPieces};
			return output;
		}

		public void setPieces() { //commenting this out because it is buggy
			/*GameObject newobject; 
			foreach (GameObject piece in bodyPieceList) {
				Debug.Log ("Setting Pieces");
				piece.transform.position = piece.transform.position + new Vector3 (10000, 0, 0);
				piece.GetComponent<ObjectInfo>().newLock (piece.transform.position);
				newobject = GameObjectUtil.Instantiate(bodyPieces[piece.GetComponent<ObjectInfo>().pos], piece.GetComponent<ObjectInfo>().initialLockPosition);
				bodyPieceList.Add (newobject);
				newobject.transform.parent = GameObject.Find ("LeftPiecePanel").transform;
			}*/
		}


	}

}