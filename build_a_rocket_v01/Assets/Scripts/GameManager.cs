using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;


namespace BuildARocketGame {

	public class GameManager : MonoBehaviour {

		public bool sendThalamusMsgs = false;

		public Text countdownTimer;
		public float initialTimerValue = 120f; // time in seconds
		public float secondInitialValue = 30f;

		// Values dependent on the number of trials
		public int totalTrialsNumber = 7;
		private List<int> distanceVals = new List<int> () {-1, -1, -1, -1, -1, -1, -1};
		private List<int> oldDistanceVals = new List<int> () {-1, -1, -1, -1, -1, -1, -1};


		// Everything on the canvas that we'll be manipulating (lots of things) 
		public GameObject weight;
		public GameObject airResistance;
		public GameObject power;
		public GameObject fuel;

		[SerializeField] Text gameOverText;
		[SerializeField] Image overlayPanel;
		[SerializeField] GameObject resultsPanel;
		[SerializeField] List<GameObject> trialResults;

		[SerializeField] Text distanceText;
		[SerializeField] Text milesText;
		[SerializeField] GameObject timer;

		[SerializeField] GameObject startButton;
		[SerializeField] GameObject toggleR;
		[SerializeField] GameObject toggleT;
		[SerializeField] GameObject toggleC;

		// rocket pieces 
		[SerializeField] GameObject[] bodyRocketPieces;
		[SerializeField] GameObject[] boosterRocketPieces;
		[SerializeField] GameObject[] coneRocketPieces;
		[SerializeField] GameObject[] finRocketPieces;

		// selected outline pieces
		[SerializeField] GameObject[] selectedBodyOutlineSlots; 
		[SerializeField] GameObject[] selectedBoosterOutlineSlots; 
		[SerializeField] GameObject[] selectedConeOutlineSlots;
		[SerializeField] GameObject[] selectedFinOutlineSlots;  

		// normal outline pieces
		[SerializeField] GameObject[] dashedBodyOutlineSlots;
		[SerializeField] GameObject[] dashedBoosterOutlineSlots;
		[SerializeField] GameObject[] dashedConeOutlineSlots;
		[SerializeField] GameObject[] dashedFinOutlineSlots;

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
		public Animator leftPanelAnimator;
		public Animator rightPanelAnimator;
		private bool firstStateChangeOccured = false;

		// sounds being used
		public AudioClip countdownBeep;
		public AudioClip liftoffSound;

		// if the game state bools
		public bool gameStarted = false;
		public bool paused = false;

		// type of piece selected
		private int currentPieceTypeSelected;
		private int lastPieceTypeSelected;

		private float timeElapsed = 0f;
		private float timeElapsed2 = 0f;
		private float remainingTime = 0f;
		private int remainingTimeSec = 0;
		private float remainingTime2 = 0f;
		private int remainingTimeSec2 = 0;
		private TimeSpan t_timer_start;

		private bool launched = false;
		private float alphaSet = 0f;

		private int distance = 0;
		private int maxDistance;

		private float fov;

		public bool canRestart = false;

		private bool doOnce;

		private Vector3 timerSavePosition;

		// audio source for sound effects
		private AudioSource audioSource1;
		private AudioSource audioSource2;

		// animator for the results panel
		private Animator resultsAnimator;

		// the object used to send all the messages to Thalamus
		private ThalamusUnity thalamusUnity;

		void Awake () {
			var saved = GameObject.Find ("SavedVariables").GetComponent<SavedVariables> ();

			for (int i = 0; i < oldDistanceVals.Count; i++) {
				distanceVals[i] = saved.distanceVals[i];
			}

			gameStarted = saved.gameStarted;
			// gameStarted = true; // DEMO CHANGE

			if (gameStarted) {
				HideUIElements ();
			} else {
				gameOverText.enabled = false;
			}

			// show the question mark
			GameObject questionMark = GameObject.Find("QuestionArea");
			questionMark.GetComponent<SpriteRenderer> ().enabled = true;

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

			// show the distance stats
			distanceText.enabled = false;
			milesText.enabled = false;

			fov =  Camera.main.orthographicSize;
			doOnce = false;
			
			// initialize the thalamusUnity object
			if (sendThalamusMsgs) {
				thalamusUnity = new ThalamusUnity ();
			}

			// initialize pieceTypeSelected
			currentPieceTypeSelected = Constants.NONE_SELECTED;
			lastPieceTypeSelected = Constants.NONE_SELECTED;

			// initialize the audio sources
			var audioSources = GetComponents<AudioSource> ();
			audioSource1 = audioSources [0];
			//audioSource2 = audioSources [1];
		}
		
		void Start ()
		{
			resultsAnimator = resultsPanel.GetComponent<Animator> ();

			// hide all outline pieces so that we don't click them
			HideAllOutlinePieces ();
		}
		
		// Update is called once per frame
		void Update () {

			if (gameStarted) {
				// update the timer
				remainingTime = initialTimerValue - timeElapsed;
				if (remainingTime > 0.0) {
					if (GetSeconds (remainingTime) != remainingTimeSec) {
						// get remaining seconds
						remainingTimeSec = GetSeconds (remainingTime);
						// set the timer text
						SetCountdownTimerText (remainingTimeSec);
						// play the beeping sound if it's 5,4,3,2,1 seconds left
						if (remainingTimeSec == 1 || remainingTimeSec == 2 || remainingTimeSec == 3 || 
							remainingTimeSec == 4 || remainingTimeSec == 5) {
							audioSource1.PlayOneShot (countdownBeep, 1F);
						}
					}
				} else {
					// we can trigger the launch here


					GameObject foreground = GameObject.Find ("Foreground");
					foreground.transform.Translate (Vector3.down);
					GameObject bottomPanel = GameObject.Find ("BottomPanel");
					bottomPanel.transform.Translate (Vector3.down);
					GameObject leftPanel = GameObject.Find ("LeftPiecePanel");
					leftPanel.transform.Translate (Vector3.down);
					GameObject rightPanel = GameObject.Find ("RightPiecePanel");
					rightPanel.transform.Translate (Vector3.down);

					var script = gameObject.GetComponent<TouchInputHandler> ();

					script.endSequence ();

					GameObject black = GameObject.Find ("black");

					if (launched == false) {
						launched = true;

						// show the distance stats
						distanceText.enabled = true;
						
						// hide the question mark
						GameObject questionMark = GameObject.Find ("QuestionArea");
						questionMark.GetComponent<SpriteRenderer> ().enabled = false;

						// show the miles
						milesText.enabled = true;

						// hide the timer
						timerSavePosition = timer.transform.position;
						timer.GetComponent<Text> ().enabled = false;
						//timer.transform.position = new Vector3(-1000, -1000, 0);


						weight.transform.position = new Vector3 (-1000, -1000, 0);
						airResistance.transform.position = new Vector3 (-1000, -1000, 0);
						fuel.transform.position = new Vector3 (-1000, -1000, 0);
						power.transform.position = new Vector3 (-1000, -1000, 0);

						// enable the jet liftoff animation if we have the engine or propeller fins
						foreach (GameObject piece in GameObject.Find("GameManager").GetComponent<TouchInputHandler>().rocketPieces) {
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

						// enable the jet liftoff animation for the bottom jets
						bottomJetEmission1.enabled = true;
						bottomJetEmission2.enabled = true;
						bottomJetEmission3.enabled = true;
						bottomJetEmission4.enabled = true;

						var pos = black.transform.position;
						pos = new Vector3 (0, 0, 0);
						black.transform.position = pos;
						
						
						// set the clip of audioSource2 to the liftoff sound
						audioSource1.clip = liftoffSound;
						
						// start the liftoff sound
						audioSource1.Play ();
					}

					var a = black.GetComponent<SpriteRenderer> ().color;
					a = new Color (1f, 1f, 1f, alphaSet);
					black.GetComponent<SpriteRenderer> ().color = a;

					// update the distance text to show how far the rocket has gone
					distanceText.text = distance.ToString ();

					maxDistance = script.calculateDistance ();
					if (distance < maxDistance) {
						distance += 1;
						if (launched == true) {
							alphaSet += .001f;
							
						}

						if (GetSeconds (remainingTime2) != remainingTimeSec2) {
							remainingTimeSec2 = GetSeconds (remainingTime2);
							SetCountdownTimerText (0);
						}
					} else {
						distance = maxDistance;

						// stop the liftoff sound
						audioSource1.Stop ();

						// set the distance value for this trial
						if (doOnce == false) {
							for (int i = 0; i < distanceVals.Count; i++)
							{
								if (distanceVals[i] == -1)
								{
									distanceVals[i] = maxDistance;
									break;
								}
							}
							doOnce = true;
						}
					
						resultsPanel.GetComponent<Animator> ().SetTrigger ("go");

						// hide all of the jet emissions
						bottomJetEmission1.enabled = false;
						bottomJetEmission2.enabled = false;
						bottomJetEmission3.enabled = false;
						bottomJetEmission4.enabled = false;
						rightJetEmission.enabled = false;
						leftJetEmission.enabled = false;

						int y_val;
						for (int i = 0; i < distanceVals.Count; i++)
						{
							if (distanceVals[i] != -1)
							{
								y_val = -16 + 7 * i;
								trialResults[i].transform.position = resultsPanel.transform.position - new Vector3 (-36, y_val, 0);
							}
						}


						int trialChanged = -1; // note trials in this case will start at 0
						for (int i = 0; i < distanceVals.Count; i++)
						{
							if (distanceVals[i] != oldDistanceVals[i])
							{
								trialChanged = i;
								break;
							}
						}
						
						// Only update the text displays and saved variables (and send a Thalamus message) when the results values change
						if (trialChanged != -1)
						{
							trialResults[trialChanged].GetComponent<Text>().text = "Trial " + (trialChanged + 1).ToString() + ":   " + distanceVals[trialChanged].ToString();
							GameObject.Find ("SavedVariables").GetComponent<SavedVariables> ().distanceVals[trialChanged] = distanceVals[trialChanged];
							oldDistanceVals[trialChanged] = distanceVals[trialChanged];
							GameObject.Find ("SavedVariables").GetComponent<SavedVariables> ().gameStarted = gameStarted;

							// send the results to Thalamus
							string resultsString = "results*";
							for (int i = 0; i < distanceVals.Count; i++)
							{
								resultsString = resultsString + distanceVals[i].ToString();
								if (i != (distanceVals.Count - 1))
								{
									resultsString = resultsString + "*";
								}
							}

							if (sendThalamusMsgs) {
								thalamusUnity.Publisher.SentFromUnityToThalamus (resultsString);
							}

						}


						// Put the game over panel once the results panel has come out
						AnimatorStateInfo currentResultsPanelState = resultsAnimator.GetCurrentAnimatorStateInfo (0);
						int trialNum = GameObject.Find ("SavedVariables").GetComponent<SavedVariables> ().trialNumber;

						if (currentResultsPanelState.IsName ("Base Layer.stay"))
						{
							paused = true;

							if (trialNum >= totalTrialsNumber)
							{
								overlayPanel.enabled = true;
								gameOverText.enabled = true;
							}
						}

						timer.transform.position = timerSavePosition;
						remainingTime2 = secondInitialValue - timeElapsed2;
						if (remainingTime2 > 0.0) {
							if (GetSeconds (remainingTime2) != remainingTimeSec2) {
								remainingTimeSec2 = GetSeconds (remainingTime2);
								SetCountdownTimerText (remainingTimeSec2);
							}
						} else {
							if (GetSeconds (remainingTime2) != remainingTimeSec2) {
								remainingTimeSec2 = GetSeconds (remainingTime2);
								SetCountdownTimerText (0);

								// hide the distance stats
								distanceText.enabled = false;
								milesText.enabled = false;
							}

							// increase the trail number & enable/disable restart
							if (trialNum < totalTrialsNumber)
							{
								GameObject.Find ("SavedVariables").GetComponent<SavedVariables> ().trialNumber++;
								Debug.Log ("Trial number: " + GameObject.Find ("SavedVariables").GetComponent<SavedVariables> ().trialNumber.ToString ());

								canRestart = true;

								paused = false;
							}
						
						}
						timeElapsed2 += Time.deltaTime;

					}


					Camera.main.orthographicSize = fov;
					//fov += .05f;

				}

				// increment the time elapsed
				timeElapsed += Time.deltaTime;

			}
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

		void HidePieces (GameObject[] pieces) {
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

		// restarts the game for each trial
		void RestartGame() {
			// reset the timer
			timeElapsed = 0;
		}

		void SetCountdownTimerText(int timerSec)
		{
			string timerText = FormatTime2 (timerSec);
			countdownTimer.text = timerText;

			if (timerSec % 5 == 0) {
				// send the timer value to Thalamus
				if (sendThalamusMsgs) {
					thalamusUnity.Publisher.SentFromUnityToThalamus ("timer*" + timerText);
				}
			}
		}

		void ShowPieces (GameObject[] pieces) {
			foreach (GameObject piece in pieces) {
				piece.GetComponent<Image> ().enabled = true;
			}
		}

		public void StartGame() {
			gameStarted = true;

			// send the condition selected to Thalamus 
			if (toggleR.GetComponent<Toggle> ().isOn) {
				if (sendThalamusMsgs) {
					thalamusUnity.Publisher.SentFromUnityToThalamus ("relational");
				}
			} 
			else if (toggleT.GetComponent<Toggle> ().isOn) {
				if (sendThalamusMsgs) {
					thalamusUnity.Publisher.SentFromUnityToThalamus ("task");
				}
			} 
			else if (toggleC.GetComponent<Toggle> ().isOn) {
				if (sendThalamusMsgs) {
					thalamusUnity.Publisher.SentFromUnityToThalamus ("control");
				}
			}

			// hide the UI elements 
			HideUIElements ();

			// show the outline pieces 
			UpdateOutlineAndRocketPanelPieces ();

			// subscribe to the event that indicates clicks on outline pieces
			Slot.OnClickForPanelChange += TriggerPanelChange;
		}

		void TriggerPanelChange (int selectedOutlineType) {
			// play the animations to hide the sidebars
			if (firstStateChangeOccured == true) {
				leftPanelAnimator.SetTrigger ("stateChangeTriggerLeft");
				rightPanelAnimator.SetTrigger ("stateChangeTriggerRight");
			} else {
				leftPanelAnimator.SetBool ("firstStateSelectedLeft", true);
				rightPanelAnimator.SetBool ("firstStateSelectedRight", true);
				firstStateChangeOccured = true;
			}

			// update the current and last piece type selected varaibles
			lastPieceTypeSelected = currentPieceTypeSelected;
			currentPieceTypeSelected = selectedOutlineType;

			// show/hide the appropriate ouline and rocket panel pieces
			UpdateOutlineAndRocketPanelPieces();
		}

		void UpdateOutlineAndRocketPanelPieces() {

			// hide the selected outlines and show the dashed outlines of the old selected piece type
			// hide the rocket pieces of the old selected piece type
			if (lastPieceTypeSelected == Constants.BODY) {
				HidePieces (selectedBodyOutlineSlots);
				ShowPieces (dashedBodyOutlineSlots);
				HidePieces (bodyRocketPieces);
			} else if (lastPieceTypeSelected == Constants.BOOSTER) {
				HidePieces (selectedBoosterOutlineSlots);
				ShowPieces (dashedBoosterOutlineSlots);
				HidePieces (boosterRocketPieces);
			} else if (lastPieceTypeSelected == Constants.CONE) {
				HidePieces (selectedConeOutlineSlots);
				ShowPieces (dashedConeOutlineSlots);
				HidePieces (coneRocketPieces);
			} else if (lastPieceTypeSelected == Constants.FIN) {
				HidePieces (selectedFinOutlineSlots);
				ShowPieces (dashedFinOutlineSlots);
				HidePieces (finRocketPieces);
			}

			// hide the dashed outlines and show the selected outlines of the new selected piece type
			// show the rocket pieces of the new selected piece type
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
				ShowPieces (bodyRocketPieces);

			} else if (currentPieceTypeSelected == Constants.BOOSTER) {
				HidePieces (dashedBoosterOutlineSlots);
				ShowPieces (selectedBoosterOutlineSlots);
				ShowPieces (boosterRocketPieces);

			} else if (currentPieceTypeSelected == Constants.CONE) {
				HidePieces (dashedConeOutlineSlots);
				ShowPieces (selectedConeOutlineSlots);
				ShowPieces (coneRocketPieces);
			} else if (currentPieceTypeSelected == Constants.FIN) {
				HidePieces (dashedFinOutlineSlots);
				ShowPieces (selectedFinOutlineSlots);
				ShowPieces (finRocketPieces);

			}

		}
			
	}

}