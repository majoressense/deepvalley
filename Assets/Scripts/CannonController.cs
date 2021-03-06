﻿using UnityEngine;
using System.Collections;

public class CannonController : MonoBehaviour {

	public AudioClip sound_shoot;
	public AnimationCurve kick_back_curve;

	public float shift_radio_threshold = 0.3f;
	public float horizontal_rotate_speed = 5.0f;
	public float vertical_rotate_speed = 5.0f;
	public float vertical_angle_max = 45.0f;
	public float vertical_angle_min = -10.0f;
	public float sight_scope_var = 90f;

	private GameObject cannonShifter;
	private GameObject cannonBore;
	private float currentVertalRotate;
	private float moveRate;

	private RadarController radarController;
	private TerrainGenerator _terrainGenerator;

	private GameObject _cannonGuan;
	private GameObject _goCamera;




	private Camera mainCamera;

	// Use this for initialization
	void Start () {
		cannonShifter = GameObject.Find ("cannon-shifter");
		cannonBore = GameObject.Find ("cannon-bore");
		_goCamera = GameObject.Find ("main-camera");
		mainCamera = GameObject.Find ("main-camera").GetComponent<Camera> ();
		currentVertalRotate = 0;
		moveRate = 1f;

		radarController = GameObject.Find ("Radar").GetComponent<RadarController> ();
		_terrainGenerator = GameObject.Find ("Terrain").GetComponent<TerrainGenerator> ();

		_cannonGuan = GameObject.Find ("cannon-guan");

		Invoke ("setPositionToValley", 0.1f);
	}
	
	// Update is called once per frame
	void Update () {
//		shift (new Vector2 (1, 1));
	}


	// move the cannon in horizontal and the gun in vertical
	// radioXY is centered and uniformed in [-1,1]
	void shift(Vector2 radioXY)
	{

		if (radioXY.magnitude < shift_radio_threshold) {
			return;
		}
		cannonShifter.transform.Rotate (new Vector3(0,horizontal_rotate_speed*radioXY.x*moveRate,0));

		radarController.turnSightScope (cannonShifter.transform.eulerAngles.y+180+90);

		//!careful for euler stays in [0-360]
		if (currentVertalRotate > vertical_angle_max && radioXY.y > 0 ||
		    currentVertalRotate < vertical_angle_min && radioXY.y < 0) {
			// forbid to rotate out of vertical field
		} else {
			float delta = vertical_rotate_speed*radioXY.y*moveRate;
			currentVertalRotate += delta;
			cannonBore.transform.Rotate(new Vector3(0,0,-delta));
		}
	}


	public void move(int moveMode) 
	{
		 //0-1-2-3 left, right, top, bottom
		Vector2 radioXY = new Vector2 ();
		radioXY.x = moveMode == 0 ? -1 : moveMode == 1 ? 1 : 0;
		radioXY.y = moveMode == 2 ? 1 : moveMode == 3 ? -1 : 0;

		shift (radioXY);
	}

	public void swiftSightScope(float radio) 
	{
		moveRate = radio > 0.7f ? 0.2f :
			radio > 0.3f ? 0.6f : 1f;
		mainCamera.fieldOfView = 100 - radio * sight_scope_var;
	}

	public void setPositionToValley()
	{
		transform.position = _terrainGenerator.getValleyPosition();
		Debug.Log (transform.position.ToString ());
	}

	IEnumerator KickBack()
	{
		float t = 0f;
		Vector3 xBegin = _cannonGuan.transform.localPosition;
		Vector3 cameraBegin = _goCamera.transform.localPosition;

		while (t < 1f) {
			t += Time.deltaTime*(3f);
			float v = kick_back_curve.Evaluate(t);
			_cannonGuan.transform.localPosition = xBegin + new Vector3(v*3f,0f,0f);
			_goCamera.transform.localPosition = cameraBegin + new Vector3(v*0.05f,0f,0f);
			yield return null;
		}
	}

	public void shootEffects()
	{
		// kickback 
		StartCoroutine ("KickBack");

		// sound
		GetComponent<AudioSource> ().PlayOneShot (sound_shoot);
	}
}
