﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace BuildARocketGame {

	public class GameManager : MonoBehaviour {

		public Text countdownTimer;
		public float rocketBuildingPhaseDuration; // time in seconds
		public float pausePhaseDuration;

		// Values dependent on the number of trials
		public int totalTrialsNumber = 7;

		// Everything on the canvas that we'll be manipulating (lots of things) 
		// the rocket stats
		public GameObject weight;
		public GameObject airResistance;
		public GameObject power;
		public GameObject fuel;

		// and their corresponding integer values
		private int rocketWeightInt;
		private int rocketAirResistanceInt;
		private int rocketPowerInt;
		private int rocketFuelInt;


		[SerializeField] Text gameOverText;
		[SerializeField] Image overlayPanel;
		[SerializeField] GameObject resultsPanel;
		[SerializeField] GameObject statsPanel;
		[SerializeField] List<GameObject> trialResults;

		[SerializeField] Text milesNumText;
		[SerializeField] Text milesLabelText;
		[SerializeField] GameObject timer;
		[SerializeField] GameObject questionMark;

		[SerializeField] GameObject startButton;
		[SerializeField] GameObject toggleR;
		[SerializeField] GameObject toggleT;
		[SerializeField] GameObject toggleC;

		// rocket pieces 
		[SerializeField] List<GameObject> bodyRocketPieces = new List<GameObject> ();
		[SerializeField] List<GameObject> boosterRocketPieces = new List<GameObject> ();
		[SerializeField] List<GameObject> coneRocketPieces = new List<GameObject> ();
		[SerializeField] List<GameObject> finRocketPieces = new List<GameObject> ();

		// rocekt pieces actually on the rocket
		private List<GameObject> piecesOnRocket = new List<GameObject> ();

		// selected outline pieces
		[SerializeField] List<GameObject> selectedBodyOutlineSlots = new List<GameObject> (); 
		[SerializeField] List<GameObject> selectedBoosterOutlineSlots = new List<GameObject> (); 
		[SerializeField] List<GameObject> selectedConeOutlineSlots = new List<GameObject> ();
		[SerializeField] List<GameObject> selectedFinOutlineSlots = new List<GameObject> ();  

		// normal outline pieces
		[SerializeField] List<GameObject> dashedBodyOutlineSlots = new List<GameObject> ();
		[SerializeField] List<GameObject> dashedBoosterOutlineSlots = new List<GameObject> ();
		[SerializeField] List<GameObject> dashedConeOutlineSlots = new List<GameObject> ();
		[SerializeField] List<GameObject> dashedFinOutlineSlots = new List<GameObject> ();

		// the jets
		[SerializeField] ParticleSystem bottomJet1;
		[SerializeField] ParticleSystem bottomJet2;
		[SerializeField] ParticleSystem bottomJet3;
		[SerializeField] ParticleSystem bottomJet4;
		[SerializeField] ParticleSystem leftJet;
		[SerializeField] ParticleSystem rightJet;

		// corresponding emission modules
		private ParticleSystem.EmissionModule bottomJetEmission1;
		private ParticleSystem.EmissionModule bottomJetEmission2;
		private ParticleSystem.EmissionModule bottomJetEmission3;
		private ParticleSystem.EmissionModule bottomJetEmission4;
		private ParticleSystem.EmissionModule leftJetEmission;
		private ParticleSystem.EmissionModule rightJetEmission;

		// animator and related variables
		public Animator panelsAnimator;
		private bool firstStateChangeOccured = false;

		// animator for the foreground
		public Animator foregroundAnimator;

		// sounds being used
		public AudioClip countdownBeep;
		public AudioClip liftoffSound;

		// type of piece selected
		private int currentPieceTypeSelected;
		private int lastPieceTypeSelected;

		// timing private variables used in the update function
		private float launchDuration = 0f;
		private float timeElapsed = 0f;
		private float remainingTime = 0f;
		private int remainingBuildTimeSec = 0;

		private bool launched = false;
		private bool stoppedLaunch = false;
		private bool gameOver = false;

		private int currentDistance;
		private int calculatedDistance;

		private bool gameStarted;

		private List<int> distanceVals = new List<int> () {-1, -1, -1, -1, -1, -1, -1};

		private int trialNumber = 1;

		// audio source for sound effects
		private AudioSource audioSource1;
		private AudioSource audioSource2;

		void Awake () {
			// initialize all of the emission modules
			bottomJetEmission1 = bottomJet1.emission;
			bottomJetEmission2 = bottomJet2.emission;
			bottomJetEmission3 = bottomJet3.emission;
			bottomJetEmission4 = bottomJet4.emission;
			leftJetEmission = leftJet.emission;
			rightJetEmission = rightJet.emission;

			// disable all of the jets
			bottomJetEmission1.enabled = false;
			bottomJetEmission2.enabled = false;
			bottomJetEmission3.enabled = false;
			bottomJetEmission4.enabled = false;
			rightJetEmission.enabled = false;
			leftJetEmission.enabled = false;

			// initialize pieceTypeSelected
			currentPieceTypeSelected = Constants.NONE_SELECTED;
			lastPieceTypeSelected = Constants.NONE_SELECTED;

			// initialize the audio sources
			var audioSources = GetComponents<AudioSource> ();
			audioSource1 = audioSources [0];
			//audioSource2 = audioSources [1];

			// initialize all of the rocket stats
			rocketWeightInt = 0;
			rocketFuelInt = 0;
			rocketAirResistanceInt = 0;
			rocketPowerInt = 0;
		}
		
		void Start ()
		{
			// hide all outline pieces so that we don't click them
			HideAllOutlinePieces ();

			// hide the rocket stats panel
			statsPanel.SetActive (false);

			// hide the timer
			countdownTimer.enabled = false;

			// hide the question mark
			questionMark.SetActive (false);

			// hide the game over text
			gameOverText.enabled = false;

			// hide the distance stats
			milesNumText.enabled = false;
			milesLabelText.enabled = false;

			// hide the results panel
			resultsPanel.SetActive (false);
		}

		// Update is called once per frame
		void Update () {
			
			if (gameStarted) {
				// if the rocket building phase has not yet ended
				remainingTime = (rocketBuildingPhaseDuration + pausePhaseDuration) - timeElapsed;
				if ((remainingTime - pausePhaseDuration) > 0.0) {
					// only update when a second has elapsed
					if (GetSeconds (remainingTime - pausePhaseDuration) != remainingBuildTimeSec) {
						// get remaining seconds
						remainingBuildTimeSec = GetSeconds (remainingTime - pausePhaseDuration);
						// set the timer text
						SetCountdownTimerText (remainingBuildTimeSec);
						// play the beeping sound if it's 5,4,3,2,1 seconds left
						if (remainingBuildTimeSec == 1 || remainingBuildTimeSec == 2 || remainingBuildTimeSec == 3 ||
							remainingBuildTimeSec == 4 || remainingBuildTimeSec == 5) {
							audioSource1.PlayOneShot (countdownBeep, 1F);
						}
					}
				}
				// if we haven't launched and time has expired - launch!!
				else if (!launched) {
					// calculate the distance the rocket should go 
					calculatedDistance = calculateDistance ();

					// calculate the time that the rocket will launch in the game
					launchDuration = (float)calculatedDistance / 20.0f;

					// set the current rocket distance to 0
					currentDistance = 0;
					milesNumText.text = currentDistance.ToString ();

					// launch the rocket
					StartRocketLaunch ();
				}
				// stop the launch after the specified launchDuration has ended
				else if (((rocketBuildingPhaseDuration + launchDuration) - timeElapsed) < 0.0 && !stoppedLaunch) {
					// stop the launch
					StopRocketLaunch ();

					// update the results array
					distanceVals[trialNumber - 1] = calculatedDistance;

					// change the results text to match what's in the distance results array
					for (int i = 0; i < resultsPanel.transform.childCount; i++) {
						resultsPanel.transform.GetChild (i).GetChild (1).gameObject.GetComponent<Text> ().text = distanceVals [i].ToString ();
					}

					// show the results 
					resultsPanel.SetActive (true);
					for (int i = 0; i < resultsPanel.transform.childCount; i++) {
						if (i <= (trialNumber - 1)) {
							resultsPanel.transform.GetChild(i).gameObject.SetActive (true);
						} else {
							resultsPanel.transform.GetChild(i).gameObject.SetActive (false);
						}
					}

					// report the results
					string resultsString = "results*";
					for (int i = 0; i < distanceVals.Count; i++)
					{
						resultsString = resultsString + distanceVals[i].ToString();
						if (i != (distanceVals.Count - 1))
						{
							resultsString = resultsString + "*";
						}
					}
					Debug.Log (resultsString);
				}
				// if we have started the launch, but haven't finished it yet, update the miles
				else if (launched && !stoppedLaunch) {
					float launchTimeElapsedRatio = (timeElapsed - rocketBuildingPhaseDuration) / launchDuration;
					currentDistance = (int)(launchTimeElapsedRatio * (float)calculatedDistance);
					milesNumText.text = currentDistance.ToString ();
				}
				// if the trial has concluded (build + pause phases have ended)
				else if (remainingTime <= 0.0 && !gameOver) {
					// game over
					if (trialNumber == 7) {
						gameOver = true;
						gameOverText.enabled = true;
						overlayPanel.enabled = true;
						HidePieces (piecesOnRocket);
						milesLabelText.enabled = false;
						milesNumText.enabled = false;
					}
					// start the next round
					else {
						// increase the trial number
						trialNumber++;

						// hide the results
						resultsPanel.SetActive (false);
						milesNumText.enabled = false;
						milesLabelText.enabled = false;

						// re-start gameplay
						timeElapsed = 0f;
						firstStateChangeOccured = false;
						foregroundAnimator.SetTrigger ("RestartRound");
						lastPieceTypeSelected = Constants.NONE_SELECTED;
						currentPieceTypeSelected = Constants.NONE_SELECTED;
						launched = false;
						stoppedLaunch = false;
						StartDragAndDropGameplay ();
					}
				}

				// increment the time elapsed
				timeElapsed += Time.deltaTime;
			}
		}

		//calculates how far the rocket should go
		public int calculateDistance() {

			List<int> rocketPieceTypes = countNumBodyPieceTypes (piecesOnRocket);
			int numConePieces = rocketPieceTypes [0];
			int numBodyPieces = rocketPieceTypes [1];
			int numBoosterPieces = rocketPieceTypes [2];
			int numFinPieces = rocketPieceTypes [3];

			// when penalizing missing pieces, we'll penalize the cone and fin pieces 3 times as much
			float penalty = (float)(numConePieces * 3 + numBodyPieces + numBoosterPieces + numFinPieces * 3) / (float)(1 * 3 + 16 + 4 + 2 * 3);

			float distanceTemp = (float)((-1.0 * ((float)rocketAirResistanceInt + 1.5 * (float)rocketWeightInt) + (float)rocketFuelInt + ((float)rocketFuelInt * (float)rocketPowerInt) * 0.002 + 250.0) * (penalty));

			if (distanceTemp < 0 || rocketFuelInt == 0 || numBoosterPieces == 0) {
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

			foreach (GameObject piece in pieces) {
				if (piece.tag == "TopCone") conePieces++;
				else if (piece.tag == "Body") bodyPieces++;
				else if (piece.tag == "Engine") boosterPieces++;
				else if (piece.tag == "RightFin" || piece.tag == "LeftFin") finPieces++;
			}

			// add them to the output list
			List<int> output = new List<int> () {conePieces, bodyPieces, boosterPieces, finPieces};
			return output;
		}

		string FormatTime(float value) {
			TimeSpan t = TimeSpan.FromSeconds (value);
			return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
		}

		string FormatTime2(int value) 
		{
			int min, sec;
			min = value / 60;
			sec = value % 60;
			return string.Format("{0:D2}:{1:D2}", min, sec);
		}

		int GetSeconds(float value) {
			TimeSpan t = TimeSpan.FromSeconds (value);
			return (t.Minutes * 60 + t.Seconds);
		}

		void HideAllOutlinePieces() {
			HidePieces (selectedBodyOutlineSlots);
			HidePieces (selectedBoosterOutlineSlots);
			HidePieces (selectedConeOutlineSlots);
			HidePieces (selectedFinOutlineSlots);
			HidePieces (dashedBodyOutlineSlots);
			HidePieces (dashedBoosterOutlineSlots);
			HidePieces (dashedConeOutlineSlots);
			HidePieces (dashedFinOutlineSlots);
		}

		void HidePieces (List<GameObject> pieces) {
			foreach (GameObject piece in pieces) {
				piece.GetComponent<Image> ().enabled = false;
			}
		}

		void HideUIElements() {
			overlayPanel.enabled = false;
			startButton.SetActive (false);
			toggleR.SetActive (false);
			toggleT.SetActive (false);
			toggleC.SetActive (false);
			gameOverText.enabled = false;
		}

		// This is our cue to hide the old pieces on the panels and show the new ones
		void PanelIn () {

			// hide the rocket pieces of the old selected piece type
			if (lastPieceTypeSelected == Constants.BODY) {
				HidePieces (bodyRocketPieces);
			} else if (lastPieceTypeSelected == Constants.BOOSTER) {
				HidePieces (boosterRocketPieces);
			} else if (lastPieceTypeSelected == Constants.CONE) {
				HidePieces (coneRocketPieces);
			} else if (lastPieceTypeSelected == Constants.FIN) {
				HidePieces (finRocketPieces);
			}

			// show the rocket pieces of the new selected piece type
			if (currentPieceTypeSelected == Constants.BODY) {
				ShowPieces (bodyRocketPieces);

			} else if (currentPieceTypeSelected == Constants.BOOSTER) {
				ShowPieces (boosterRocketPieces);

			} else if (currentPieceTypeSelected == Constants.CONE) {
				ShowPieces (coneRocketPieces);
			} else if (currentPieceTypeSelected == Constants.FIN) {
				ShowPieces (finRocketPieces);

			}
		}

		void PieceAddedToPanel (GameObject pieceAdded) {
			if (pieceAdded.tag == "Body") {
				bodyRocketPieces.Add (pieceAdded);
			} else if (pieceAdded.tag == "Engine") {
				boosterRocketPieces.Add (pieceAdded);
			} else if (pieceAdded.tag == "LeftFin" || pieceAdded.tag == "RightFin") {
				finRocketPieces.Add (pieceAdded);
			} else if (pieceAdded.tag == "TopCone") {
				coneRocketPieces.Add (pieceAdded);
			}
		}

		void PieceAddedToRocket (GameObject pieceAdded) {
			// add the piece to the piecesOnRocket list if the piece was previously
			// on a panel (not moved around inside the rocket
			int removalIndex = -1;
			if (pieceAdded.tag == "Body") {
				for (var i = 0; i < bodyRocketPieces.Count; i++) {
					if (pieceAdded.name == bodyRocketPieces [i].name) {
						removalIndex = i;
						break;
					}
				}
				if (removalIndex != -1) {
					bodyRocketPieces.RemoveAt (removalIndex);
					piecesOnRocket.Add (pieceAdded);
				}
			} else if (pieceAdded.tag == "RightFin" || pieceAdded.tag == "LeftFin") {
				for (var i = 0; i < finRocketPieces.Count; i++) {
					if (pieceAdded.name == finRocketPieces [i].name) {
						removalIndex = i;
						break;
					}
				}
				if (removalIndex != -1) {
					finRocketPieces.RemoveAt (removalIndex);
					piecesOnRocket.Add (pieceAdded);
				}
			} else if (pieceAdded.tag == "TopCone") {
				for (var i = 0; i < coneRocketPieces.Count; i++) {
					if (pieceAdded.name == coneRocketPieces [i].name) {
						removalIndex = i;
						break;
					}
				}
				if (removalIndex != -1) {
					coneRocketPieces.RemoveAt (removalIndex);
					piecesOnRocket.Add (pieceAdded);
				}
			} else if (pieceAdded.tag == "Engine") {
				for (var i = 0; i < boosterRocketPieces.Count; i++) {
					if (pieceAdded.name == boosterRocketPieces [i].name) {
						removalIndex = i;
						break;
					}
				}
				if (removalIndex != -1) {
					boosterRocketPieces.RemoveAt (removalIndex);
					piecesOnRocket.Add (pieceAdded);
				}
			}

			// update the rocket stats
			rocketWeightInt += pieceAdded.GetComponent<RocketPieceInfo> ().weight;
			rocketFuelInt += pieceAdded.GetComponent<RocketPieceInfo> ().fuel;
			rocketAirResistanceInt += pieceAdded.GetComponent<RocketPieceInfo> ().airResistance;
			rocketPowerInt += pieceAdded.GetComponent<RocketPieceInfo> ().power;

			// update the rocket stats on the display
			UpdateRocketStats ();
		}

		void PieceDroppedOnQuestionMark (GameObject pieceDropped) {
			Debug.Log (pieceDropped.name + " dropped on question mark");
		}

		void PieceRemoved (GameObject pieceToRemove) {
			int removalIndex = -1;
			for (var i = 0; i < piecesOnRocket.Count; i++) {
				if (piecesOnRocket[i].name == pieceToRemove.name) {
					removalIndex = i;
					break;
				}
			}
			if (removalIndex != -1) {
				piecesOnRocket.RemoveAt (removalIndex);
				rocketWeightInt -= pieceToRemove.GetComponent<RocketPieceInfo> ().weight;
				rocketFuelInt -= pieceToRemove.GetComponent<RocketPieceInfo> ().fuel;
				rocketAirResistanceInt -= pieceToRemove.GetComponent<RocketPieceInfo> ().airResistance;
				rocketPowerInt -= pieceToRemove.GetComponent<RocketPieceInfo> ().power;
				UpdateRocketStats ();
			}
		}

		// used for debugging purposes only 
		void printPieceList (List<GameObject> listToPrint) {
			foreach (GameObject listItem in listToPrint) {
				Debug.Log (listItem.name);
			}
		}

		// restarts the game for each trial
		void RestartGame() {
			// reset the timer
			timeElapsed = 0;
		}

		void SetCountdownTimerText(int timerSec)
		{
			string timerText = FormatTime2 (timerSec);
			countdownTimer.text = timerText;
		}

		void ShowPieces (List<GameObject> pieces) {
			foreach (GameObject piece in pieces) {
				piece.GetComponent<Image> ().enabled = true;
			}
		}

		public void StartGame() {
			
			gameStarted = true;

			// report the selected condition
			if (toggleR.GetComponent<Toggle> ().isOn) {
				Debug.Log ("Relational condition");
			} 
			else if (toggleT.GetComponent<Toggle> ().isOn) {
				Debug.Log ("Task condition");
			} 
			else if (toggleC.GetComponent<Toggle> ().isOn) {
				Debug.Log ("Control condition");
			}

			// hide the UI elements 
			HideUIElements ();

			// subscribe to the events that indicate clicks on outline pieces
			Slot.OnClickForPanelChangeOutlinePiece += TriggerPanelChange;
			DragHandler.OnClickForPanelChangeRocketPiece += TriggerPanelChange;

			// subscribe to the event that alerts the game manager of pieces added to the rocket
			Slot.OnPieceAddedToRocket += PieceAddedToRocket;

			// subscribe to the event that alerts the game manager of cloned pieces added to the panel
			DragHandler.OnPieceClonedToPanel += PieceAddedToPanel;

			// subscribe to the event that alerts the game manager of a deleted piece (via trash)
			DragHandler.OnPieceRemovedByTrash += PieceRemoved;

			// subscribe to the event that alerts the game manager of the panel going in
			PanelAnimationEventHandler.OnTriggerPanelIn += PanelIn;

			// subscribe to the event that alerts the game manager of a piece dropped on a question mark
			DragHandler.OnPieceDroppedOnQuestionMark += PieceDroppedOnQuestionMark;

			StartDragAndDropGameplay ();
		}

		void StartDragAndDropGameplay () {

			// show the timer
			countdownTimer.enabled = true;

			// show the outline pieces 
			UpdateOutlineAndRocketPanelPieces ();

			// show the stats panel
			statsPanel.SetActive (true);

			// show the question mark 
			questionMark.SetActive (true);
		}

		void StartRocketLaunch () {
			// indicate that we've launched
			launched = true;

			// hide all of the UI elements that we don't want
			statsPanel.SetActive (false);
			questionMark.SetActive (false);
			countdownTimer.enabled = false;

			// hide all of the outline pieces
			HidePieces(dashedBodyOutlineSlots);
			HidePieces (dashedBoosterOutlineSlots);
			HidePieces (dashedConeOutlineSlots);
			HidePieces (dashedFinOutlineSlots);
			HidePieces (selectedBodyOutlineSlots);
			HidePieces (selectedBoosterOutlineSlots);
			HidePieces (selectedConeOutlineSlots);
			HidePieces (selectedFinOutlineSlots);

			// trigger the animations to hide the side panels and the ground
			panelsAnimator.SetTrigger("movePanelsOut");
			foregroundAnimator.SetTrigger ("TriggerLaunch");

			// show the distance stats
			milesNumText.enabled = true;
			milesLabelText.enabled = true;

			// set the clip of audioSource2 to the liftoff sound
			audioSource1.clip = liftoffSound;

			// start the liftoff sound
			audioSource1.Play ();

			// enable the jets for the relevant fin/booster pieces
			bottomJetEmission1.enabled = true;
			bottomJetEmission2.enabled = true;
			bottomJetEmission3.enabled = true;
			bottomJetEmission4.enabled = true;

			// enable the jet liftoff animation if we have the engine or propeller fins
			foreach (GameObject piece in piecesOnRocket) {
				if (piece.name.Contains ("fin_Engine") || piece.name.Contains ("fin_Propeller")) {
					// if it's on the left side
					if (piece.transform.position.x < 0) {
						leftJetEmission.enabled = true;
					} 
					// or if it's on the right side 
					else {
						rightJetEmission.enabled = true;
					}
				}
			}
		}

		void StopDragAndDropGameplay () {

			// unsubscribe from all events
			Slot.OnClickForPanelChangeOutlinePiece -= TriggerPanelChange;
			Slot.OnPieceAddedToRocket -= PieceAddedToRocket;
			DragHandler.OnClickForPanelChangeRocketPiece -= TriggerPanelChange;
			DragHandler.OnPieceClonedToPanel -= PieceAddedToPanel;
			DragHandler.OnPieceRemovedByTrash -= PieceRemoved;
			PanelAnimationEventHandler.OnTriggerPanelIn -= PanelIn;
			DragHandler.OnPieceDroppedOnQuestionMark -= PieceDroppedOnQuestionMark;
		}

		void StopRocketLaunch () {
			stoppedLaunch = true;

			// stop the liftoff sound
			audioSource1.Stop ();

			// disable the jets for the relevant fin/booster pieces
			bottomJetEmission1.enabled = false;
			bottomJetEmission2.enabled = false;
			bottomJetEmission3.enabled = false;
			bottomJetEmission4.enabled = false;
			leftJetEmission.enabled = false;
			rightJetEmission.enabled = false;
		}

		void TriggerPanelChange (int selectedOutlineType) {
			// only switch the panels if we're wanting to put on a different type of piece
			if (selectedOutlineType != currentPieceTypeSelected) {
				// play the animations to hide the sidebars
				if (firstStateChangeOccured == true) {
					panelsAnimator.SetTrigger ("movePanelsInOut");
				} else {
					panelsAnimator.SetTrigger ("movePanelsIn");
					firstStateChangeOccured = true;
				}

				// update the current and last piece type selected varaibles
				lastPieceTypeSelected = currentPieceTypeSelected;
				currentPieceTypeSelected = selectedOutlineType;

				// show/hide the appropriate ouline and rocket panel pieces
				UpdateOutlineAndRocketPanelPieces();
			}
		}

		void UpdateOutlineAndRocketPanelPieces() {

			// hide the selected outlines and show the dashed outlines of the old selected piece type
			if (lastPieceTypeSelected == Constants.BODY) {
				HidePieces (selectedBodyOutlineSlots);
				ShowPieces (dashedBodyOutlineSlots);
			} else if (lastPieceTypeSelected == Constants.BOOSTER) {
				HidePieces (selectedBoosterOutlineSlots);
				ShowPieces (dashedBoosterOutlineSlots);
			} else if (lastPieceTypeSelected == Constants.CONE) {
				HidePieces (selectedConeOutlineSlots);
				ShowPieces (dashedConeOutlineSlots);
			} else if (lastPieceTypeSelected == Constants.FIN) {
				HidePieces (selectedFinOutlineSlots);
				ShowPieces (dashedFinOutlineSlots);
			}

			// hide the dashed outlines and show the selected outlines of the new selected piece type
			if (currentPieceTypeSelected == Constants.NONE_SELECTED) {
				
				// show all dashed pieces 
				ShowPieces (dashedBodyOutlineSlots);
				ShowPieces (dashedBoosterOutlineSlots);
				ShowPieces (dashedConeOutlineSlots);
				ShowPieces (dashedFinOutlineSlots);

				// hide all selected pieces
				HidePieces (selectedBodyOutlineSlots);
				HidePieces (selectedBoosterOutlineSlots);
				HidePieces (selectedConeOutlineSlots);
				HidePieces (selectedFinOutlineSlots);

				// hide all the body pieces
				HidePieces (bodyRocketPieces);
				HidePieces (boosterRocketPieces);
				HidePieces (coneRocketPieces);
				HidePieces (finRocketPieces);

			} else if (currentPieceTypeSelected == Constants.BODY) {
				HidePieces (dashedBodyOutlineSlots);
				ShowPieces (selectedBodyOutlineSlots);

			} else if (currentPieceTypeSelected == Constants.BOOSTER) {
				HidePieces (dashedBoosterOutlineSlots);
				ShowPieces (selectedBoosterOutlineSlots);

			} else if (currentPieceTypeSelected == Constants.CONE) {
				HidePieces (dashedConeOutlineSlots);
				ShowPieces (selectedConeOutlineSlots);
			} else if (currentPieceTypeSelected == Constants.FIN) {
				HidePieces (dashedFinOutlineSlots);
				ShowPieces (selectedFinOutlineSlots);

			}

		}

		void UpdateRocketStats () {
			weight.GetComponent<Text> ().text = rocketWeightInt.ToString ();
			fuel.GetComponent<Text> ().text = rocketFuelInt.ToString ();
			airResistance.GetComponent<Text> ().text = rocketAirResistanceInt.ToString ();
			power.GetComponent<Text> ().text = rocketPowerInt.ToString ();
		}
			
	}

}