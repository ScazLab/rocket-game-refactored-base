using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SavedPieceInfo {
	public Vector3 initialLockPosition;
	public int pieceType; // 0 - cone, 1 - body, 2 - booster, 3 - fin
	public int pos;
	public Vector3 vectorPos;

	public SavedPieceInfo(Vector3 initialLockPosition, int pieceType, int pos, Vector3 vectorPos)
	{
		this.initialLockPosition = initialLockPosition;
		this.pieceType = pieceType;
		this.pos = pos;
		this.vectorPos = vectorPos;
	}
}

public class SavedVariables : MonoBehaviour {

	public List<int> distanceVals = new List<int> () {-1, -1, -1, -1, -1, -1, -1};

	public int trialNumber = 1;

	// public bool gameStarted = true; // DEMO CHANGE
	public bool gameStarted = false;

	public List<SavedPieceInfo> previousTrialRocketPieces = new List<SavedPieceInfo> ();

	void Awake() {
		DontDestroyOnLoad (this);
	}
}
