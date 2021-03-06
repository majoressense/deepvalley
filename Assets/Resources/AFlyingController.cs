﻿using UnityEngine;
using System.Collections;


// controll the total life of a flying!
public class AFlyingController : MonoBehaviour {


	// Use this for initialization
	public float existing_time = 20f;
	public float flying_scale = 4f;
	public float colider_radius = 0.8f;
	public float target_scale = 1f;
	public float _emission = 0.35f;

	private TerrainGenerator _terrainGenerator;
	private RadarController _radarController;
	private float _speed = 0;
	private float _acce = 1f;
	private float _maxSpeed = 2f;
	private Vector3 _direction;
	private float _terrainWidthHalf;

	private GameObject _explosion;


	private int _id;
	private bool _hasDamaged = false;
	private float _timeExisted = 0;
	private int _blood = 2;
	private bool _falling = false;
	private float _gravitySpeed = 0;
	private float _gravity = 9f;

	private MainController _mainController;



	public void setId(int id) {
		_id = id;
	}

	void Start () {

		_terrainGenerator = GameObject.Find ("Terrain").GetComponent<TerrainGenerator> ();
		_radarController = GameObject.Find ("Radar").GetComponent<RadarController> ();
		
		transform.position  = _terrainGenerator.nextFlyingPosition();
		_direction = _terrainGenerator.nextFlyingDirection ();

		float angle = Vector3.Angle (new Vector3 (0, 0, 1), _direction);

		transform.localScale = new Vector3 (flying_scale,flying_scale,flying_scale);
		transform.eulerAngles = new Vector3 (0, angle, 0);
		_terrainWidthHalf = 0.5f * _terrainGenerator.getTerrainWidth ();

		// add point to radar
		_radarController.AddPoint (_id);

		Rigidbody rigid = gameObject.AddComponent<Rigidbody> ();
		rigid.isKinematic = true;
		rigid.useGravity = false;

		SphereCollider colider = gameObject.AddComponent<SphereCollider> ();
		colider.radius = colider_radius;
		colider.isTrigger = true;

		gameObject.tag = "Flying";



		_explosion = Instantiate (Resources.Load("Explosion"))as GameObject;
		_explosion.transform.SetParent (transform);
		_explosion.transform.localPosition = Vector3.zero;

		_mainController = GameObject.Find ("Terrain").GetComponent<MainController> ();

		GetComponentInChildren<Renderer>().sharedMaterial.SetColor("_EmissionColor", new Color (_emission,_emission,_emission,_emission));
//		GetComponent<Renderer>().sharedMaterial.SetColor("_EmissionColor", new Color (_emission,_emission,_emission,_emission));

		Appear ();
	}
	
	// Update is called once per frame
	void Update () {
		if (_displeared)
			return;

		if (_speed < _maxSpeed) {
			_speed += _acce * Time.deltaTime;
		}
		transform.position = transform.position + _direction * (_speed * Time.deltaTime);

		if (_falling) {
			_gravitySpeed += Time.deltaTime*_gravity;
			transform.position = transform.position + new Vector3(0f,-1f,0f) * (_gravitySpeed * Time.deltaTime);
		}

		Vector2 pointPosition = ( new Vector2 (transform.position.x - _terrainWidthHalf, transform.position.z - _terrainWidthHalf) )* (1 / _terrainWidthHalf);
		_radarController.UpdatePoint(_id, pointPosition);

		_timeExisted += Time.deltaTime;
		if (_timeExisted > existing_time) {
			Disappear();
		}

		//crash check
//		if (_terrainGenerator.CheckSolid (transform.position)) {
////			Crash();
//			StartSmoke();
//			Fall();
//		}

		if (transform.position.y < -200) {
			Destroy(gameObject);
		}

	}

//	void Crash()
//	{
//		StartSmoke ();
//		Disappear ();
//	}


	void OnTriggerEnter(Collider other) {
		Debug.Log ("trigger enter");
		if (other.gameObject.tag == "Bullet") {
//			_explosion.GetComponent<ExplosionController>().boom();
//			Destroy (other.gameObject);
//			Destroy (gameObject);
			_timeExisted = 0;
			_blood = _blood - 1;
			if (!_hasDamaged) {
				_hasDamaged = true;
				StartSmoke();
			}
			if (_blood == 0) {
				//TODO win one!
				_mainController.AddScore();
				Fall();
			}
//			_direction = (_direction + other.gameObject.transform.forward).normalized;


		}
	}

	private bool _displeared = false;
	void Disappear() {
		_displeared = true;
		StartCoroutine ("SmallOut");
	}

	void Appear() {
		transform.localScale = new Vector3 (0.01f, 0.01f, 0.01f);
		StartCoroutine ("BigIn");
	}

	IEnumerator BigIn()
	{
		while (transform.localScale.x < target_scale) {
			transform.localScale = transform.localScale * 1.2f;
			yield return null;
		}
	}

	IEnumerator SmallOut()
	{
		while (transform.localScale.x > 0.01) {
			transform.localScale = transform.localScale * 0.8f;
			yield return null;
		}
		
		_radarController.DeletePoint (_id);
		Destroy(gameObject);
	}

	void StartSmoke() {
		Debug.Log ("start smoke");
		var smoke = Instantiate (Resources.Load("ASmoke"))as GameObject;
		smoke.transform.SetParent (transform);
		smoke.transform.localPosition = Vector3.zero;
	}

	void Fall() {
		_radarController.DeletePoint (_id);
		Debug.Log ("fall");
		_falling = true;
	}
}
