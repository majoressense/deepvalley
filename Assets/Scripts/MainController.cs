﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MainController : MonoBehaviour {
	public float delay_generate_terrain = 0.5f;
	public float init_left_time = 60f*2;
	public float time_add_per_score = 15f;
	private int _currentLevel;


	private TerrainGenerator _terrainGenerator;
	private GameObject _goFlyings;
	private Text _textLeftTime;
	private Text _textCurrentScore;
	private Text _textTargetScore;
	private int _currentScore;
	private int _targetScore;
	private float _leftTime;
	private bool _running = false;
	private Color _leftTextOldColor;

	// Use this for initialization
	void Start () {

		int[] targetScores = new int[]           {5,        13,       34,       89};
		float[] reduceRates = new float[]        {0.65f,    0.65f,    0.65f,   0.65f};

		float[] initHeightLB = new float[]       {0,        0,        0,       0};
		float[] initHeightLT = new float[]       {30,       30,       30,      30};
		float[] initHeightRT = new float[]       {30,       30,       30,      30};
		float[] initHeightRB = new float[]       {0,        0,        0,       0};

		float[] initRandomScope = new float[]    {15.44f,    20f,     20f,     20f};
		string[] randomSeedMap = new string[]    {"7a",     "b",      "c",     "d"};
		float[] centerDeepScales = new float[]   {2f,       2f,       2f,      2f};
		bool[] smoothNormals = new bool[]         {false,    false,    false,   false};



		_currentLevel = PlayerPrefs.HasKey ("current_level") ? PlayerPrefs.GetInt ("current_level") : 0;

		//debug
		_currentLevel = 0;
	
		_leftTime = init_left_time;
		_targetScore = targetScores [_currentLevel];
		_currentScore = 0;

		_textLeftTime = GameObject.Find ("left_time").GetComponent<Text> ();
		_textTargetScore = GameObject.Find ("target-score").GetComponent<Text> ();
		_textCurrentScore = GameObject.Find ("current-score").GetComponent<Text> ();

		_textTargetScore.text =  (_targetScore <10 ? "0":"" )+_targetScore.ToString ();
		_textCurrentScore.text = (_currentScore < 10 ? "0" : "") + _currentScore.ToString ();


		_terrainGenerator = GetComponent<TerrainGenerator> ();
		
		_terrainGenerator.reduce_rate = reduceRates [_currentLevel];
		_terrainGenerator.init_height_left_bottom = initHeightLB [_currentLevel];
		_terrainGenerator.init_height_left_top = initHeightLT [_currentLevel];
		_terrainGenerator.init_height_right_top = initHeightRT [_currentLevel];
		_terrainGenerator.init_height_right_bottom = initHeightRB [_currentLevel];
		_terrainGenerator.init_random_scope = initRandomScope [_currentLevel];
		_terrainGenerator.random_seed = randomSeedMap[_currentLevel];
		_terrainGenerator.center_deep_scale = centerDeepScales [_currentLevel];
		_terrainGenerator.smooth_normal = smoothNormals [_currentLevel];

		_terrainGenerator.generateTerrain ();

		_goFlyings = GameObject.Find ("Flyings");
		_leftTextOldColor = _textLeftTime.color;
		RefreshTime ();

		Invoke ("StartAddFlying", 3);
	}

	System.Random _flyingRandomGen;
//	List<GameObject> _flyings;
	AFlyingNames _flyingNamesGen = new AFlyingNames();
	int _idIndex = 0;
	void StartAddFlying()
	{
		string	seed = Time.time.ToString();
		_running = true;
//		_flyings = new List<GameObject> ();
		_idIndex = 0;
		
		_flyingRandomGen = new System.Random(seed.GetHashCode());
		StartCoroutine ("AddingFlyings");
	}

	IEnumerator AddingFlyings() {
		for (;;) {
			AddOneFlying();
			yield return new WaitForSeconds(_flyingRandomGen.Next(3,5)*1f);
		}
	}



	void AddOneFlying() {
		string flyName = _flyingNamesGen.getRandomFlyingName ();
		int id = _idIndex++;
		Debug.Log ("Add flying:"+flyName+"  id:"+id.ToString());
		GameObject obj = Instantiate( Resources.Load(flyName) )as GameObject;
		obj.transform.parent = _goFlyings.transform;
		AFlyingController flyCon = obj.AddComponent<AFlyingController>() as AFlyingController;
		flyCon.setId (id);
	}

	// kill a flying!
	public void AddScore()
	{
		_currentScore++;
		_textCurrentScore.text = (_currentScore < 10 ? "0" : "") + _currentScore.ToString ();
		_leftTime = Mathf.Min (init_left_time, _leftTime + time_add_per_score);

		if (_currentScore == _targetScore) {
			Debug.Log("Win");
		}
	}

	void RefreshTime() 
	{
		if (_leftTime > 0) {
			_leftTime = Mathf.Max(_leftTime - Time.deltaTime, 0f);

			int minut = (int)(_leftTime / 60);
			int second = (int)((_leftTime - minut * 60));
			_textLeftTime.text = minut.ToString () + ":" + (second < 10 ? "0" : "") + second.ToString ();

			Color c = _leftTime < 10 ? Color.red : _leftTextOldColor;
			_textLeftTime.color = c;
		}

	}
	
	// Update is called once per frame
	void Update () {

		if (_running) {
			RefreshTime();
			if (_leftTime <= 0) {
				Debug.Log ("GO");
			}
		}
	}
}
